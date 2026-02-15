using System;
using System.Collections.Generic;
using FishingGame.Domain;

namespace FishingGame.Domain
{
    
    /// <summary>
    /// Classe representant un jeu complet de 52 cartes
    /// </summary>
    public sealed class CardPair
    {
        private readonly List<Card> _cards = new();
        public IReadOnlyList<Card> Cards => _cards;
        public int Count => _cards.Count;

        public CardPair()
        {
            BuildFullDeck();
        }

        // Methode permettant la creation du jeu 
        private void BuildFullDeck()
        {
            var colors = new[]
            {
                CardColor.Coeur,
                CardColor.Carreau,
                CardColor.Trefle,
                CardColor.Pique
            };

            foreach (var color in colors)
            {
                foreach (var value in Enum.GetValues<CardValue>()) 
                {
                    _cards.Add(new Card(value, color));
                }
            }

            if (_cards.Count != 52)
                throw new InvalidOperationException("Le jeu doit contenir exactement 52 cartes.");
        }

    }
}