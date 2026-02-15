// ScoreHelper.cs — NOUVELLE CLASSE

using FishingGame.Domain;

namespace FishingGame.Utilities
{
    public static class ScoreHelper
    {
        public static int CardPoints(CardValue v) => v switch
        {
            CardValue.As    => 11,
            CardValue.Valet => 2,
            CardValue.Dame  => 2,
            CardValue.Roi   => 2,
            CardValue.Deux  => 2,
            CardValue.Trois => 3,
            CardValue.Quatre=> 4,
            CardValue.Cinq  => 5,
            CardValue.Six   => 6,
            CardValue.Sept  => 7,
            CardValue.Huit  => 8,
            CardValue.Neuf  => 9,
            CardValue.Dix   => 10,
            _ => 0
        };
    }
}