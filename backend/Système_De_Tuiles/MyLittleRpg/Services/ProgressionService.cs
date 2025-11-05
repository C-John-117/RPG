using MyLittleRpg.Models;

namespace MyLittleRpg.Services
{
    public class ProgressionService : IProgressionService
    {
        public int XpGagnee(Monster monstreDeBase, int niveauInstanceMonstre)
        {
            int experienceGagnee = monstreDeBase.ExperienceBase + (niveauInstanceMonstre * 10);
            return experienceGagnee;
        }

        // Seuil d’XP pour atteindre le prochain niveau. regle : 100 × niveau actuel.

        public int SeuilXpPourNiveau(int niveauActuel)
        {
            int seuil = 100 * niveauActuel;
            return seuil;
        }

        public bool AppliquerMonteesDeNiveau(Personnage joueur)
        {
            bool auMoinsUnNiveauGagne = false;

            // Tant que l’XP suffit pour passer le niveau courant, on fait monter
            bool peutMonter = true;
            while (peutMonter)
            {
                int seuilPourNiveauCourant = SeuilXpPourNiveau(joueur.Niveau);

                if (joueur.Experience >= seuilPourNiveauCourant)
                {
                    joueur.Experience = joueur.Experience - seuilPourNiveauCourant;
                    joueur.Niveau = joueur.Niveau + 1;

                    joueur.Force = joueur.Force + 1;
                    joueur.Defense = joueur.Defense + 1;
                    joueur.PointsVieMax = joueur.PointsVieMax + 1;
                    joueur.PointsVie = joueur.PointsVieMax; // restauration des PV

                    auMoinsUnNiveauGagne = true;
                }
                else
                {
                    peutMonter = false;
                }
            }

            return auMoinsUnNiveauGagne;
        }
    }
}
