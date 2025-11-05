using MyLittleRpg.Models;

namespace MyLittleRpg.Services
{
    public class CombatService : ICombatService
    {
        private static readonly Random GenerateurAleatoire = new Random();

        public double FacteurAleatoire()
        {
            double borneMinimale = 0.8;
            double amplitude = 1.25 - 0.8; // 0.45
            double facteur = borneMinimale + GenerateurAleatoire.NextDouble() * amplitude;
            return facteur;
        }

        public ResultatCombat Resoudre(Personnage joueur, InstanceMonster monstreInstance, Monster monstreDeBase)
        {
            // Stat du monstre selon l'énoncé: ForceBase + Niveau
            int niveauMonstre = monstreInstance.Niveau;
            
            // DEBUG: Afficher les stats pour diagnostic
            Console.WriteLine($"=== COMBAT DEBUG ===");
            Console.WriteLine($"Monstre: {monstreDeBase?.Nom ?? "NULL"}");
            Console.WriteLine($"ForceBase: {monstreDeBase?.ForceBase ?? 0}, DefenseBase: {monstreDeBase?.DefenseBase ?? 0}");
            Console.WriteLine($"Niveau: {niveauMonstre}");
            
            // Stats selon la formule de l'énoncé (sans division)
            int forceMonstreEffective = monstreDeBase.ForceBase + niveauMonstre;
            int defenseMonstreEffective = monstreDeBase.DefenseBase + niveauMonstre;
            
            Console.WriteLine($"Force effective: {forceMonstreEffective}, Defense effective: {defenseMonstreEffective}");
            Console.WriteLine($"Joueur - Force: {joueur.Force}, Defense: {joueur.Defense}");
            Console.WriteLine($"==================");

            double facteurMultiplicatif = FacteurAleatoire();

            // Dégâts monstre : (ForceJoueur – DéfenseMonstre) * Facteur
            double degatsMonstreReels = (joueur.Force - defenseMonstreEffective) * facteurMultiplicatif;
            int degatsAuMonstre = (int)Math.Floor(degatsMonstreReels);
            if (degatsAuMonstre < 0)
                degatsAuMonstre = 0;

            // Dégâts au joueur : (ForceMonstre – DéfenseJoueur) * Facteur
            double degatsJoueurReels = (forceMonstreEffective - joueur.Defense) * facteurMultiplicatif;
            int degatsAuJoueur = (int)Math.Floor(degatsJoueurReels);
            if (degatsAuJoueur < 0)
                degatsAuJoueur = 0;

            // 5) Application simultanée des dégâts
            int pointsVieMonstreApres = monstreInstance.PointsVieActuels - degatsAuMonstre;
            if (pointsVieMonstreApres < 0)
                pointsVieMonstreApres = 0;

            int pointsVieJoueurApres = joueur.PointsVie - degatsAuJoueur;
            if (pointsVieJoueurApres < 0)
                pointsVieJoueurApres = 0;

            //  Détermination de l’issue du round
            bool joueurVainqueur = false;
            bool monstreVainqueur = false;

            if (pointsVieMonstreApres <= 0 && pointsVieJoueurApres > 0)
            {
                joueurVainqueur = true;
            }

            if (pointsVieJoueurApres <= 0 && pointsVieMonstreApres > 0)
            {
                monstreVainqueur = true;
            }


            ResultatCombat resultat = new ResultatCombat
            {
                DegatsAuMonstre = degatsAuMonstre,
                DegatsAuJoueur = degatsAuJoueur,
                HpMonstreApres = pointsVieMonstreApres,
                HpJoueurApres = pointsVieJoueurApres,
                JoueurVainqueur = joueurVainqueur,
                MonstreVainqueur = monstreVainqueur
            };
            return resultat;
        }
    }
}
