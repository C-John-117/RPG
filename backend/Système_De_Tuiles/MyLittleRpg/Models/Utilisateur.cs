namespace MyLittleRpg.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string Pseudo { get; set;}
        public DateTime DateInscription { get; set; }
        public Personnage? Personnage { get; set; }
        public Utilisateur() { }

        public Utilisateur(int idUtilisateur, string email, string motDePasse, string pseudo, DateTime dateInscription)
        {
            Id = idUtilisateur;
            Email = email;
            MotDePasse = motDePasse;
            Pseudo = pseudo;
            DateInscription = dateInscription;
        }
    }
}
