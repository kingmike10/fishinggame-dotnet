using System;
using System.Collections.Generic;
using System.Linq;

namespace FishingGame.Domain
{
 
    public delegate IReadOnlyList<Card> ShuffleStrategy(IEnumerable<Card> source);

    /// <summary>
    /// Pioche (mutable) — responsable de tirer et (re)charger des cartes via un délégué de mélange.
    /// </summary>
    public sealed class DrawStack
    {
        private readonly List<Card> _cards = new();

        // Charge la pioche avec un lot et un mélangeur fourni.
        public void Load(IEnumerable<Card> cards, ShuffleStrategy shuffler)
        {
            if (cards is null) throw new ArgumentNullException(nameof(cards));
            if (shuffler is null) throw new ArgumentNullException(nameof(shuffler));
            if (_cards.Count != 0)
                throw new InvalidOperationException("La pile de pioche doit etre vide avant le chargement.");

            var mixed = shuffler(cards); // Invocation du délégué 
            _cards.AddRange(mixed);//Ajoute toutes les cartes contenues dans mixed à la fin de la liste _cards
        }

        // Tire une seule carte (le gameboard va s'assurer que la pile n'est jamais vide)
        
        public Card DrawOne()
        {
            var lastIndex = _cards.Count - 1;
            var card = _cards[lastIndex];
            _cards.RemoveAt(lastIndex);//tire la carte qui est au dessus de la pile de pioche
            return card;
        }


        // Tire jusqu’à n cartes
        public IReadOnlyList<Card> DrawUpTo(int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));

            int k = Math.Min(n, _cards.Count);//Détermine combien de cartes on peut réellement tirer
            var result = _cards.GetRange(_cards.Count - k, k);//Cree une sous liste contenant las cartes a extraire sans les supprmier
            _cards.RemoveRange(_cards.Count - k, k);// enleve les cartes en question de la liste 
            return result;//retourne la liste 
        }

        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;
    }
}
