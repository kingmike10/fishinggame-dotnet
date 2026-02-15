using FishingGame.Domain;
using FishingGame.Engine;
using System;

namespace FishingGame.Observer
{
    // Petit observateur : mémorise le dernier joueur “à 1 carte”.
    // Sert de signal à AntiFinishDecorator sans coupler la stratégie au GameBoard.
    public sealed class ThreatTracker
    {
        public Player? Threat { get; private set; }

        // S’abonne au relais global du plateau (observer)
        public void Attach(GameBoard board)
        {
            if (board is null) throw new ArgumentNullException(nameof(board));
            board.OnPlayerDownToOneCard += p => Threat = p;
        }

        // Remise à zéro si besoin (nouvelle manche, etc.)
        public void Reset() => Threat = null;
    }
}