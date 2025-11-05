using MyLittleRpg.Models;

namespace MyLittleRpg.Services
{
    public interface IProgressionService
    {
        int XpGagnee(Monster monstreBase, int niveauInstance);
        int SeuilXpPourNiveau(int niveauActuel);
        bool AppliquerMonteesDeNiveau(Personnage joueur);
    }
}
