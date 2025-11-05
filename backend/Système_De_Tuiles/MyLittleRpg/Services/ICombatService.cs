using MyLittleRpg.Models;

namespace MyLittleRpg.Services
{
    public struct ResultatCombat
    {
        public int DegatsAuMonstre;
        public int DegatsAuJoueur;
        public int HpMonstreApres;
        public int HpJoueurApres;
        public bool JoueurVainqueur;
        public bool MonstreVainqueur;
    }
    public interface ICombatService
    {
         double FacteurAleatoire();
        ResultatCombat Resoudre(Personnage joueur, InstanceMonster monstreInstance, Monster monstreBase);
    }
}
