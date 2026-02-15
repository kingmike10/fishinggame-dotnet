// GameResult.cs
using System.Collections.Generic;
using FishingGame.Domain;

namespace FishingGame.Utilities
{
    /// <summary>
    /// Contient le résultat final d'une partie :
    /// - Le vainqueur
    /// - Le nombre de tours joués
    /// - La liste des joueurs (état final)
    /// </summary>
    public readonly struct GameResult
    {
        public Player Winner { get; }
        public int Turns { get; }
        public IReadOnlyList<Player> Players { get; }

        public GameResult(Player winner, int turns, IReadOnlyList<Player> players)
        {
            Winner = winner;
            Turns = turns;
            Players = players;
        }

        public override string ToString()
            => $"Vainqueur = {Winner}, Tours = {Turns}";
    }
}