using System.Collections.Generic;
using FishingGame.Domain;
using FishingGame.Utilities;

namespace FishingGame.Strategy
{
    /// <summary>
    /// Patron de stratégie : propose un coup sans modifier l'état du jeu.
    /// </summary>
    public interface IPlayStrategy
    {
        /// <summary>
        /// Choisit l'index de la carte à jouer dans la main.
        /// </summary>
        /// <param name="snapshot">L'instantané de l'état actuel du jeu.</param>
        /// <param name="hand">La liste des cartes dans la main du joueur.</param>
        /// <returns>L'index de la carte à jouer, ou null si aucun coup n'est possible.</returns>
        int? ChooseIndex(GameSnapshot snapshot, IReadOnlyList<Card> hand);

        /// <summary>
        /// Choisit la couleur lorsqu'un Valet est joué.
        /// </summary>
        /// <param name="snapshot">L'instantané de l'état actuel du jeu.</param>
        /// <param name="hand">La liste des cartes dans la main du joueur.</param>
        /// <returns>La couleur choisie, ou null pour laisser le moteur de jeu décider.</returns>
        CardColor? ChooseJackColor(GameSnapshot snapshot, IReadOnlyList<Card> hand);

        /// <summary>
        /// Obtient le nom de la stratégie.
        /// </summary>
        string Name { get; }
    }
}