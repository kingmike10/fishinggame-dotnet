using System.Collections.Generic;
using FishingGame.Domain;
using FishingGame.Utilities;

namespace FishingGame.Strategy
{
    // Décorateur de stratégie : ajoute un réflexe "anti-finish"
    // Si le prochain joueur est menaçant (1 carte), on tente de le freiner
    // en priorisant les coups speciaux
    public sealed class AntiFinishDecorator : IPlayStrategy
    {
        private readonly IPlayStrategy _inner;      // stratégie de base décorée
        public string Name => $"{_inner.Name}+AntiFinish";

        public AntiFinishDecorator(IPlayStrategy inner) => _inner = inner;

        public int? ChooseIndex(GameSnapshot s, IReadOnlyList<Card> hand)
        {
            if (s.PlayableIndexes.Count == 0) return null;

            // Déviation défensive seulement si le prochain est en "menace"
            if (s.IsThreatNext)
            {
                // Recherche la première carte jouable d'une valeur précise
                int pick(CardValue v)
                {
                    for (int m = 0; m < s.PlayableIndexes.Count; m++)
                    {
                        int i = s.PlayableIndexes[m];
                        if (hand[i].Value == v) return i;
                    }
                    return -1;
                }

                // Ordre de priorité défensive
                int ix;
                if ((ix = pick(CardValue.Deux))  >= 0) return ix; // +2 / passe le tour
                if ((ix = pick(CardValue.As))    >= 0) return ix; // skip
                if ((ix = pick(CardValue.Valet)) >= 0) return ix; // impose une couleur
                if ((ix = pick(CardValue.Dix))   >= 0) return ix; // inverse le sens
            }

            // Sinon, on retombe sur la stratégie décorée
            return _inner.ChooseIndex(s, hand);
        }

        // Le décorateur ne change pas la politique de couleur du Valet : délégation pure
        public CardColor? ChooseJackColor(GameSnapshot s, IReadOnlyList<Card> hand)
            => _inner.ChooseJackColor(s, hand);
    }
}