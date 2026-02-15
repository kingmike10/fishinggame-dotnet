using System;

namespace FishingGame.Domain
{

    public class Person
    {
        //Proprietes en lecture seule:Le nom d'une personne ne doit pas changer apres sa creation
        public string FirstName { get; }
        public string LastName  { get; }

        
        public virtual string FullName => $"{FirstName} {LastName}".Trim();//pour supprimer les vides inutiles

        //Constructeur pour s'assurer de ne pas avoir une personne avec le nom et le prenom vide 
        public Person(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Vous devez fournir aumoins un des champs ");

            FirstName = firstName?.Trim() ?? string.Empty;//evite une exception si le firstname est vide
            LastName  = lastName?.Trim()  ?? string.Empty;//evite une exception si le lastname est vide
        }

        public override string ToString() => FullName;
    }
}