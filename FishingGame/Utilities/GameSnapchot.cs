using System.Collections.Generic;
using FishingGame.Domain;

namespace FishingGame.Utilities
{
    /// <summary>Photographie minimale du plateau pour la stratégie.</summary>
    public sealed class GameSnapshot
    {
        public required Player Current { get; init; }
        public required Player Next { get; init; }
        public required IReadOnlyList<Player> Players { get; init; }

        public required Card? TopDeposit { get; init; }
        public required CardColor CurrentColor { get; init; }
        public required CardValue CurrentValue { get; init; }

        public required IReadOnlyList<int> PlayableIndexes { get; init; }

        /// <summary>Le prochain joueur est en menace (1 carte)?</summary>
        public bool IsThreatNext { get; init; }
    }
}