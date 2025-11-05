namespace MyLittleRpg.Models
{
    public class Personnage
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int UtilisateurId { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public DateTime DateCreation { get; set; }

        // Coordonnées où l'on garder le domicile ville ( pour le depart on le met à la position de depart ) 
        public int domicileX { get; set; } = 10;
        public int domicileY { get; set; } = 10;
        public Personnage() { }

        public Personnage(int idPersonnage, string nom, int niveau, int experience, int pointsVie, int pointsVieMax, int force, int defense, int positionX, int positionY, int utilisateurId, DateTime dateCreation)
        {
            Id = idPersonnage;
            Nom = nom;
            Niveau = niveau;
            Experience = experience;
            PointsVie = pointsVie;
            PointsVieMax = pointsVieMax;
            Force = force;
            Defense = defense;
            PositionX = positionX;
            PositionY = positionY;
            UtilisateurId = utilisateurId;
            DateCreation = dateCreation;
        }
    }
}
