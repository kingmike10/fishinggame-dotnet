namespace FishingGame.Domain
{
    /// <summary>
    /// Représente la valeur nominale d'une carte standard (As à Roi).
    /// Les valeurs ordinales facilitent les comparaisons et tris.
    /// </summary>
    public enum CardValue : byte
    {
        As    = 1,   
        Deux  = 2,
        Trois = 3,
        Quatre= 4,
        Cinq  = 5,
        Six   = 6,
        Sept  = 7,
        Huit  = 8,
        Neuf  = 9,
        Dix   = 10,
        Valet = 11,
        Dame  = 12,
        Roi   = 13
    }
}