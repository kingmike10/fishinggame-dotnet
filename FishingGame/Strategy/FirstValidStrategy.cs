using System.Collections.Generic;
using FishingGame.Domain;
using FishingGame.Utilities;

namespace FishingGame.Strategy
{
    /// <summary>
    /// Stratégie la plus simple : joue la première carte valide proposée par l'instantané du jeu.
    /// Utile comme baseline ou pour des bots "simples".
    /// </summary>
    public sealed class FirstValidStrategy : IPlayStrategy
    {
        public string Name => "FirstValid";

        public int? ChooseIndex(GameSnapshot s, IReadOnlyList<Card> hand)
            => s.PlayableIndexes.Count > 0 ? s.PlayableIndexes[0] : (int?)null;

        /// <summary>
        /// Pas d'avis particulier sur la couleur du Valet : laisse le moteur décider.
        /// </summary>
        public CardColor? ChooseJackColor(GameSnapshot s, IReadOnlyList<Card> hand) => null;
    }
}