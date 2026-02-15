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
    public sealed class FishingGame
    {
        // État moteur minimal : plateau, RNG et tempo entre les tours
        public GameBoard Board { get; }
        private readonly Random _rng;
        private readonly int _turnDelayMs;

        // Stratégies par joueur (pattern Strategy) et suivi de menace (Observer)
        private readonly Dictionary<Player, IPlayStrategy> _strategies = new();
        private readonly ThreatTracker _tracker;

        public FishingGame(GameBoard board, ThreatTracker tracker, int turnDelayMs = 400, int? seed = null)
        {
            // Invariants d’injection (dépendances obligatoires)
            Board = board ?? throw new ArgumentNullException(nameof(board));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            _turnDelayMs = Math.Max(0, turnDelayMs);
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();

            // Affichage Console quand un joueur tombe à 1 carte (événement relayé par le Board)
            Board.OnPlayerDownToOneCard += p => Console.WriteLine($"[EVENT] {p} à 1 carte !");
        }

        // Branche (ou remplace) la stratégie d’un joueur donné
        public void SetStrategy(Player p, IPlayStrategy strategy)
            => _strategies[p] = strategy ?? throw new ArgumentNullException(nameof(strategy));

        public async Task<GameResult> RunAsync(int? perPlayerOverride = null, CancellationToken ct = default)
        {
            // Taille de main initiale : fournie ou tirée au hasard [5..8]
            int perPlayer = perPlayerOverride ?? _rng.Next(5, 9);
            Console.WriteLine($"[SETUP] {Board.Players.Count} joueurs, {perPlayer} cartes chacun.");

            // Mise en place : distribution, premier joueur aléatoire, affichage de l’état initial
            Board.DealToAll(perPlayer);
            RandomizeFirstPlayer();
            PrintInitialSetup();

            int turns = 0;

            // Boucle principale de la partie : un passage = un tour joué (ou pioché)
            while (true)
            {
                ct.ThrowIfCancellationRequested(); // permet un arrêt propre depuis l’extérieur
                var current = Board.CurrentPlayer; // snapshot du joueur actif pour ce tour

                if (current.IsEmpty) // victoire stricte : seulement quand la main est vide
                    return AnnounceWin(current, turns);

                // 1) Gestion de la pénalité (cartes à piocher après un '2')
                if (await TryHandlePendingPenaltyAsync(current, ct))
                    continue;

                // 2) Gestion de l’As (tour sauté)
                if (await TryHandleSkipAsync(current, ct))
                    continue;

                // 3) Tour normal : on prend une capture du plateau pour conseiller la décision
                var snap = Board.BuildSnapshot();
                int? pick = ChooseIndex(current, snap);               // choix de la carte (via stratégie + fallback)
                await ResolvePickAsync(current, pick, snap, ct);      // exécution : jouer, effets spéciaux, ou piocher

                if (current.IsEmpty) // re-vérification après l’action du tour
                    return AnnounceWin(current, turns + 1);

                // Passage au joueur suivant + petite pause pour lisibilité en console
                turns++;
                Board.AdvanceTurn();
                await Task.Delay(_turnDelayMs, ct);
            }
        }

        // Helpers setup

        // Décale le premier joueur de manière aléatoire pour varier les parties
        private void RandomizeFirstPlayer()
        {
            int shift = _rng.Next(Board.Players.Count);
            for (int i = 0; i < shift; i++) Board.AdvanceTurn();
        }

        // Affiche l’état de départ : qui commence, sens de jeu, ordre des joueurs
        private void PrintInitialSetup()
        {
            Console.WriteLine($"[SETUP] Commence: {Board.CurrentPlayer}");
            Console.WriteLine($"[SETUP] Sens initial: {Board.Direction}");
            Console.WriteLine($"[SETUP] Ordre des joueurs: {FormatInitialOrder()}");
        }

        // Helpers début de tour : cas spéciaux 

        private async Task<bool> TryHandlePendingPenaltyAsync(Player current, CancellationToken ct)
        {
            if (Board.PendingDrawPenalty <= 0) return false;

            int need = Board.PendingDrawPenalty;
            int drawn = 0;

            // Pioche exactement 'need' cartes, en recyclant si nécessaire
            while (drawn < need)
            {
                if (Board.Draw.IsEmpty)
                    EnsureRecycleOrFail(); // stoppe la partie si recyclage impossible

                current.DrawFrom(Board.Draw);
                drawn++;
            }

            Console.WriteLine($"[EFFECT] {current} pioche {need} (pénalité).");

            // La pénalité consomme le tour suivant
            Board.PendingDrawPenalty = 0;
            Board.SkipNextTurn = false;
            Board.AdvanceTurn();
            await Task.Delay(_turnDelayMs, ct);
            return true;
        }

        private async Task<bool> TryHandleSkipAsync(Player current, CancellationToken ct)
        {
            if (!Board.SkipNextTurn) return false;

            Console.WriteLine($"[EFFECT] Tour sauté: {current}");
            Board.SkipNextTurn = false;
            Board.AdvanceTurn();
            await Task.Delay(_turnDelayMs, ct);
            return true;
        }

        // Helpers tour normal : décision & exécution 

        // Sélectionne l’index de la carte à jouer : stratégie si présente, sinon première jouable
        private int? ChooseIndex(Player current, GameSnapshot snap)
        {
            if (_strategies.TryGetValue(current, out var strat))
            {
                var pick = strat.ChooseIndex(snap, current.Hand);
                if (pick.HasValue) return pick;
            }
            return (snap.PlayableIndexes.Count > 0 ? snap.PlayableIndexes[0] : (int?)null);
        }

        // Applique la décision : tenter de jouer, gérer le Valet (choix de couleur), sinon piocher
        private async Task ResolvePickAsync(Player current, int? pick, GameSnapshot snap, CancellationToken ct)
        {
            if (pick is int iCard)
            {
                bool ok = Board.TryPlayFromHand(iCard, () => ChooseColorOnJack(current, snap));
                if (!ok && snap.PlayableIndexes.Count > 0)
                    ok = Board.TryPlayFromHand(snap.PlayableIndexes[0], () => ChooseColorOnJack(current, snap));

                if (ok)
                {
                    Console.WriteLine($"[PLAY] {current} a joué: {Board.LastDeposited}");
                    Console.WriteLine($"      Top défausse: {Board.LastDeposited}");
                    var played = Board.LastDeposited!.Value;
                    LogSpecialEffectsAfterPlay(played);

                    // Conserve ton signal manuel après jeu (évite un déclenchement prématuré ailleurs)
                    current.CheckOneCardSignal();
                    return;
                }

                // Choix inapplicable au final => on tombe sur la pioche
                await EnsureDrawAsync(current, ct);
                return;
            }

            // Pas de proposition et/ou aucune carte jouable => pioche
            await EnsureDrawAsync (current, ct);
        }

        // Pioche résiliente : essaie de recycler si vide ; sinon, on logge et on laisse passer le tour
        private async Task EnsureDrawAsync(Player current, CancellationToken ct)
        {
            if (Board.Draw.IsEmpty)
                EnsureRecycleOrFail(); // stoppe la partie si recyclage impossible

            Console.WriteLine($"[TURN] {current} ne peut pas jouer → pioche 1.");
            current.DrawFrom(Board.Draw);
            await Task.Delay(_turnDelayMs, ct);
        }


        // Logging des effets spéciaux : purement informatif console 

        private void LogSpecialEffectsAfterPlay(Card played)
        {
            switch (played.Value)
            {
                case CardValue.As:
                    Console.WriteLine("[EFFECT][AS] Le prochain joueur verra son tour sauté.");
                    break;
                case CardValue.Dix:
                    Console.WriteLine($"[EFFECT][10] Sens du jeu inversé. Nouveau sens: {Board.Direction}.");
                    break;
                case CardValue.Deux:
                    Console.WriteLine($"[EFFECT][2] Pénalité active: +{Board.PendingDrawPenalty} carte(s) à piocher pour le prochain joueur (tour sauté).");
                    break;
                case CardValue.Valet:
                    if (Board.CurrentColorOverride is CardColor forced)
                        Console.WriteLine($"[EFFECT][VALET] Couleur imposée: {forced} (jusqu’à changement).");
                    else
                        Console.WriteLine("[EFFECT][VALET] Couleur imposée non détectée (sécurité).");
                    break;
            }
        }

        // Choix de couleur après Valet : priorité à la stratégie, sinon heuristique locale (couleur majoritaire)
        private CardColor ChooseColorOnJack(Player current, GameSnapshot snap)
        {
            if (_strategies.TryGetValue(current, out var st))
            {
                var chosen = st.ChooseJackColor(snap, current.Hand);
                if (chosen is CardColor c) return c;
            }

            if (current.Hand.Count == 0)
            {
                var all = new[] { CardColor.Trefle, CardColor.Carreau, CardColor.Coeur, CardColor.Pique };
                return all[_rng.Next(all.Length)];
            }

            return current.Hand.GroupBy(c => c.Color).OrderByDescending(g => g.Count()).First().Key;
        }

        // Fin de partie & affichages utilitaires 

        private GameResult AnnounceWin(Player winner, int turns)
        {
            Console.WriteLine($"[END] Victoire de {winner} en {turns} tour(s).");
            return new GameResult(winner, turns, Board.Players.ToArray());
        }

        // Construit une chaîne “J1 → J2 → …” dans le sens courant, en partant du joueur actif
        private string FormatInitialOrder()
        {
            var list = (List<Player>)Board.Players;
            int n = list.Count;
            int start = list.IndexOf(Board.CurrentPlayer);
            int dir = (int)Board.Direction;

            var names = new List<string>(n);
            for (int t = 0; t < n; t++)
            {
                int idx = ((start + dir * t) % n + n) % n;
                names.Add(list[idx].ToString());
            }
            return string.Join(" → ", names);
        }
        //s'assure que le jeu ne se poursuit pas si la pile de pioche ne peut etre recharge
        private void EnsureRecycleOrFail()
        {
            // Le recyclage doit TOUJOURS réussir pour continuer la partie
            if (!Board.TryRecycleDrawFromDeposit())
                throw new InvalidOperationException(
                    "Pioche vide et recyclage impossible : partie interrompue (défausse insuffisante).");
        }

    }
}
