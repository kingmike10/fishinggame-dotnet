using FishingGame.Domain;

namespace FishingGame.Domain
{
    /// <summary>
    /// Représente une carte composée d'une valeur et d'une couleur.
    /// Deux cartes sont égales si elles ont la même valeur et la même couleur.
    /// </summary>
    public struct Card
    {
        public CardValue Value { get; }     // anciennement Valeur
        public CardColor Color { get; }     // anciennement Couleur

        public Card(CardValue value, CardColor color)
        {
            Value = value;
            Color = color;
        }

        public override string ToString() => $"{Value} de {Color}";

        public override bool Equals(object? obj)
        {
            if (obj is Card other)
                return Value == other.Value && Color.Equals(other.Color);
            return false;
        }

        public override int GetHashCode()
        {
            // Combine les deux champs pour garantir un code unique par carte
            return HashCode.Combine(Value, Color);
        }

        public static bool operator ==(Card a, Card b) => a.Equals(b);
        public static bool operator !=(Card a, Card b) => !a.Equals(b);
    }
}