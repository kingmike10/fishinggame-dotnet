namespace FishingGame.Domain
{
    /// <summary>
    /// Représente une couleur de carte à jouer.
    /// (Type valeur léger, utilisé dans la structure Card)
    /// </summary>
    public struct CardColor: IEquatable<CardColor>
    {
    
        public string Name { get; }
        
        private CardColor(string nom)
        {
            Name = nom;
        }

        // Couleurs standards 
        public static readonly CardColor Trefle  = new CardColor("Trefle");
        public static readonly CardColor Carreau = new CardColor("Carreau");
        public static readonly CardColor Coeur   = new CardColor("Coeur");
        public static readonly CardColor Pique   = new CardColor("Pique");

        //  Redéfinition de l’opérateur ==
        public static bool operator ==(CardColor left, CardColor right)
        {
            return string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
        }

        // Redéfinition de l’opérateur !=
        public static bool operator !=(CardColor left, CardColor right)
        {
            return !(left == right);
        }

        // Implémentation IEquatable<CardColor>
        public bool Equals(CardColor other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        

        // Redéfinition de GetHashCode
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        }
        public override string ToString()
        {
            return Name;
        }
    }
}