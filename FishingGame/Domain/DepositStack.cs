using System;
using System.Collections.Generic;

namespace FishingGame.Domain
{
    /// <summary>
    /// Défausse — reçoit les cartes jouées et permet leur recyclage
    /// (en vidant toutes sauf la carte du dessus).
    /// </summary>
    public sealed class DepositStack
    {
        private readonly List<Card> _cards = new();

        /// <summary>
        /// Dépose une carte sur la défausse (en haut de la pile).
        /// </summary>
        public void Deposit(Card card)
        {
            if (card.Equals(default(Card))) // vérifie que la carte n'est pas un struct vide
                throw new ArgumentException("Carte invalide.", nameof(card));

            _cards.Add(card);
        }

        /// <summary>
        /// Extrait toutes les cartes SAUF la carte du dessus (pour recyclage de la pioche).
        /// Si la défausse contient 0 ou 1 carte, retourne une liste vide.
        /// </summary>
        public List<Card> TakeAllButTop()
        {
            if (_cards.Count <= 1)
                return new List<Card>(0);

            int take = _cards.Count - 1;
            var batch = _cards.GetRange(0, take); // copie toutes sauf la dernière
            _cards.RemoveRange(0, take);          // conserve uniquement le top
            return batch;
        }

      

        /// <summary>Carte visible sur la défausse (le "top").</summary>
        public Card? PeekOrNull() => _cards.Count > 0 ? _cards[^1] : (Card?)null;

        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;
    }
}