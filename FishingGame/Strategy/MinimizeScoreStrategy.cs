using System.Collections.Generic;
using System.Linq;
using FishingGame.Domain;
using FishingGame.Utilities;

namespace FishingGame.Strategy
{
    // Stratégie  MinimizeScore : parmi les coups jouables,
    // choisit celui qui minimise le score restant en main
    // (à égalité, retire le plus de points).
    public sealed class MinimizeScoreStrategy : IPlayStrategy
    {
        public string Name => "MinimizeScore";

        public int? ChooseIndex(GameSnapshot s, IReadOnlyList<Card> hand)
        {
            if (s.PlayableIndexes.Count == 0) return null;

            int bestIdx = -1, bestRemaining = int.MaxValue, bestRemoved = -1;

            // Évalue chaque coup jouable
            for (int k = 0; k < s.PlayableIndexes.Count; k++)
            {
                int i = s.PlayableIndexes[k];
                var played = hand[i];

                // Points retirés si je joue cette carte
                int removed = ScoreHelper.CardPoints(played.Value);

                // Score restant en main après ce coup
                int remaining = 0;
                for (int t = 0; t < hand.Count; t++)
                    if (t != i) remaining += ScoreHelper.CardPoints(hand[t].Value);

                // Meilleur si remaining plus faible, puis removed plus fort à égalité
                bool better = remaining < bestRemaining ||
                              (remaining == bestRemaining && removed > bestRemoved);

                if (better) { bestIdx = i; bestRemaining = remaining; bestRemoved = removed; }
            }

            return bestIdx >= 0 ? bestIdx : (int?)null;
        }

        // si on doit choisir une couleur pour Valet,
        // prendre la couleur majoritaire en main (sinon laisser le moteur).
        public CardColor? ChooseJackColor(GameSnapshot s, IReadOnlyList<Card> hand)
            => hand.Count == 0
                ? null
                : hand.GroupBy(c => c.Color).OrderByDescending(g => g.Count()).First().Key;
    }
}