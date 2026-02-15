using System;
using System.Collections.Generic;

namespace FishingGame.Domain
{
 
    

    /// <summary>
    /// Player: possède une main (lecture seule), pioche depuis DrawStack et défausse via DepositStack.
    /// Prépare la stratégie (délégué) et l'événement "une carte restante".
    /// </summary>
    public class Player : Person
    {
        private readonly List<Card> _hand = new();
        
        // Événement déclenché quand il reste UNE carte
        public event Action<Player>? HasOneCard;
        
        
        public Guid Id { get; }
        public IReadOnlyList<Card> Hand => _hand;
        
        // Capacité maximale de la main
        private const int MaxHandSize = 8;
        
        public Player(string firstName, string lastName, Guid? id = null)
            : base(firstName, lastName)
        {
            Id = id ?? Guid.NewGuid();
        }
        
        // Pioche 1 carte depuis la DrawStack 
        public void DrawFrom(DrawStack drawStack)
        {
            if (drawStack is null) throw new ArgumentNullException(nameof(drawStack));

            var card = drawStack.DrawOne();
            _hand.Add(card);
            CheckOneCardSignal();
        }
        
        public void RemoveAt(int index, bool suppressOneCardEvent = false)
        {
            if (index < 0 || index >= _hand.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _hand.RemoveAt(index);

            if (!suppressOneCardEvent)
                CheckOneCardSignal();
        }
        
        /// <summary>
        /// Reçoit une carte donnée (par distribution)
        /// </summary>
        public void Receive(Card card)
        {
            _hand.Add(card);
            CheckOneCardSignal();
        }
        
        private void OnHasOneCard()
        {
            // Copie locale du délégué pour éviter les conditions de course
            Action<Player>? handlers = HasOneCard;
            // Utilisation de Invoke() pour déclencher l’événement
            handlers?.Invoke(this);
        }
        
        public void CheckOneCardSignal()
        {
            if (_hand.Count == 1)
                OnHasOneCard();
        }
        
        
        /// <summary>
        /// Verifie que la main du joueur est vide
        /// </summary>
        public bool IsEmpty => _hand.Count == 0;
        public int Count => _hand.Count;
        public override string ToString()
            => FullName;
      
        
    
      
    }
}
