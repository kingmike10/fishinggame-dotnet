using System.Collections.Generic;
using FishingGame.Domain;
using FishingGame.Utilities;

namespace FishingGame.Strategy
{
    /// <summary>
    /// Délégué pour la décision de tour d'un joueur.
    /// </summary>
    /// <param name="snapshot">L'instantané de l'état actuel du jeu.</param>
    /// <param name="hand">La liste des cartes dans la main du joueur.</param>
    /// <returns>L'index de la carte à jouer, ou null si aucun coup n'est possible.</returns>
    public delegate int? TurnDecision(GameSnapshot snapshot, IReadOnlyList<Card> hand);
    
    /// <summary>
    /// Délégué pour la décision de la couleur lorsqu'un Valet est joué.
    /// </summary>
    /// <param name="snapshot">L'instantané de l'état actuel du jeu.</param>
    /// <param name="hand">La liste des cartes dans la main du joueur.</param>
    /// <returns>La couleur choisie, ou null pour laisser le moteur de jeu décider.</returns>
    public delegate CardColor? JackColorDecision(GameSnapshot snapshot, IReadOnlyList<Card> hand);

    /// <summary>
    /// Adaptateur : permet d’instancier une stratégie à partir de délégués ou de lambdas.
    /// </summary>
    public sealed class LambdaPlayStrategy : IPlayStrategy
    {
        private readonly TurnDecision _turn;
        private readonly JackColorDecision _jack;

        public string Name { get; }

        public LambdaPlayStrategy(string name, TurnDecision turn, JackColorDecision? jack = null)
        {
            Name = name;
            _turn = turn;
            _jack = jack ?? ((_, __) => null);
        }

        public int? ChooseIndex(GameSnapshot snapshot, IReadOnlyList<Card> hand)
            => _turn(snapshot, hand);

        public CardColor? ChooseJackColor(GameSnapshot snapshot, IReadOnlyList<Card> hand)
            => _jack(snapshot, hand);
    }
}