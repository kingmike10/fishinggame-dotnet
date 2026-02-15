using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FishingGame.Domain;
using FishingGame.Observer;
using FishingGame.Strategy;
using FishingGame.Utilities;

namespace FishingGame.Engine
{
    public enum PlayDirection { Clockwise = 1, CounterClockwise = -1 }

    public sealed class GameBoard
    {
        private readonly List<Player> _players;
        private readonly DrawStack _draw = new();
        private readonly DepositStack _deposit = new();
        private int _currentIndex;

        public IReadOnlyList<Player> Players => _players;
        public int CurrentIndex => _currentIndex;
        public Player CurrentPlayer => _players[_currentIndex];
        public DrawStack Draw => _draw;
        public DepositStack Deposit => _deposit;

        public event Action<Player>? OnPlayerDownToOneCard;

        public int PendingDrawPenalty { get; set; } = 0;
        public bool SkipNextTurn { get; set; } = false;
        public CardColor? CurrentColorOverride { get; private set; } = null;
        public Card? LastDeposited { get; private set; } = null;
        public PlayDirection Direction { get; private set; } = PlayDirection.Clockwise;
        private readonly ShuffleStrategy _shuffler;

        public GameBoard(List<Player> players, CardPair deck, ShuffleStrategy shuffler)
        {
            if (players is null)   throw new ArgumentNullException(nameof(players), "La liste des joueurs doit être fournie.");
            if (deck is null)      throw new ArgumentNullException(nameof(deck), "Le jeu complet doit être fourni.");
            if (shuffler is null)  throw new ArgumentNullException(nameof(shuffler), "La méthode de mélange doit être fournie.");
            if (players.Count < 2 || players.Count > 4)
                throw new ArgumentException("Au moins 2 joueurs et pas plus de 4 joueurs sont requis.", nameof(players));

            _players = new List<Player>(players);             // copie locale
            _shuffler = shuffler; 
            _draw.Load(deck.Cards, shuffler);                 // init pioche

            foreach (var p in _players)                       // Observer: relais
                p.HasOneCard += RelayPlayerDownToOneCard;

            _currentIndex = 0;
        }

        public void DealToAll(int perPlayer)
        {
            if (perPlayer < 5 || perPlayer > 8)
                throw new ArgumentOutOfRangeException(nameof(perPlayer), "Doit être entre 5 et 8.");
            if (perPlayer * _players.Count > _draw.Count)
                throw new InvalidOperationException("Pas assez de cartes dans la pioche.");

            UnsubscribeAll();
            try
            {
                for (int i = 0; i < perPlayer; i++)
                    foreach (var player in _players)
                        player.Receive(_draw.DrawOne());
            }
            finally
            {
                foreach (var p in _players)
                    p.HasOneCard += RelayPlayerDownToOneCard;
            }
        }
        
        
        //  recycle la pioche à partir de la défausse (en laissant la carte du dessus)
        public bool TryRecycleDrawFromDeposit()
        {
            // Récupère toutes les cartes SAUF le top de la défausse
            var payload = _deposit.TakeAllButTop();
            if (payload.Count == 0)
                return false;

            // Recharge la pioche avec un mélange (même stratégie que l'initialisation)
            _draw.Load(payload, _shuffler);
            Console.WriteLine($"[INFO] Recyclage pioche: {_draw.Count} carte(s) depuis la défausse.");
            return true;
        }


        private void UnsubscribeAll()
        {
            foreach (var p in _players)
                p.HasOneCard -= RelayPlayerDownToOneCard;
        }

        private void RelayPlayerDownToOneCard(Player p)
        {
            var handler = OnPlayerDownToOneCard;
            handler?.Invoke(p);
        }

        private int NextIndex(int fromIndex, int steps = 1)
        {
            int dir = (int)Direction;
            int n = _players.Count;
            int raw = fromIndex + dir * steps;
            return ((raw % n) + n) % n;
        }

        public Player NextPlayer() => _players[NextIndex(_currentIndex, 1)];

        public void AdvanceTurn() => _currentIndex = NextIndex(_currentIndex, 1);

        // règles de pose 
        private bool CanPlayOnTop(Card candidate)
        {
            if (LastDeposited is null) return true; // premier coup autorisé

            var top = LastDeposited.Value;

            // Valet interdit sur 2
            if (candidate.Value == CardValue.Valet && top.Value == CardValue.Deux)
                return false;

            // Couleur imposée par Valet actif ?
            if (CurrentColorOverride is CardColor forced)
            {
                if (candidate.Value == CardValue.Valet) return true; // peut changer la couleur à venir
                return candidate.Color.Equals(forced);
            }

            // règles classiques
            if (candidate.Value == CardValue.Valet) return true;
            if (candidate.Color.Equals(top.Color)) return true;
            if (candidate.Value == top.Value) return true;

            return false;
        }

        private void InvertDirection()
            => Direction = Direction == PlayDirection.Clockwise
                         ? PlayDirection.CounterClockwise
                         : PlayDirection.Clockwise;

        public void ApplySpecialEffectsAfterDeposit(Card played, Func<CardColor>? chooseColor = null)
        {
            LastDeposited = played;

            switch (played.Value)
            {
                case CardValue.As:
                    SkipNextTurn = true;
                    break;

                case CardValue.Dix:
                    InvertDirection();
                    break;

                case CardValue.Deux:
                    PendingDrawPenalty += 2;
                    SkipNextTurn = true;
                    break;

                case CardValue.Valet:
                    if (chooseColor is null)
                        throw new ArgumentNullException(nameof(chooseColor),
                            "Un délégué de choix de couleur est requis quand un Valet est joué.");
                    CurrentColorOverride = chooseColor();
                    break;

                default:
                    CurrentColorOverride = null; // on efface un éventuel override restant
                    break;
            }
        }

        public IReadOnlyList<int> GetPlayableIndexesForCurrent()
        {
            var hand = CurrentPlayer.Hand;
            var idx = new List<int>(hand.Count);
            for (int i = 0; i < hand.Count; i++)
                if (CanPlayOnTop(hand[i])) idx.Add(i);
            return idx;
        }

        public bool TryPlayFromHand(int handIndex, Func<CardColor>? chooseColorOnJack)
        {
            var p = CurrentPlayer;
            if (handIndex < 0 || handIndex >= p.Hand.Count) return false;
            var candidate = p.Hand[handIndex];

            if (!CanPlayOnTop(candidate)) return false;
            
            p.RemoveAt(handIndex, suppressOneCardEvent: true);

            _deposit.Deposit(candidate);
            ApplySpecialEffectsAfterDeposit(candidate, chooseColorOnJack);
            return true;
        }


        //helpers pour Strategy/snapshot
        public CardColor EffectiveCurrentColor()
        {
            if (CurrentColorOverride is CardColor forced) return forced;
            if (LastDeposited is Card top) return top.Color;
            // par défaut, n'importe quelle couleur (choix moteur au premier coup)
            return CardColor.Trefle; // valeur arbitraire, peu utilisée avant premier dépôt
        }

        public CardValue EffectiveCurrentValue()
            => LastDeposited?.Value ?? CardValue.As; // arbitraire avant premier dépôt

        public GameSnapshot BuildSnapshot()
        {
            var current = CurrentPlayer;
            var next = NextPlayer();
            return new GameSnapshot
            {
                Current = current,
                Next = next,
                Players = _players,
                TopDeposit = LastDeposited,
                CurrentColor = EffectiveCurrentColor(),
                CurrentValue = EffectiveCurrentValue(),
                PlayableIndexes = GetPlayableIndexesForCurrent(),
                IsThreatNext = next.Hand.Count == 1
            };
        }
    }
}
