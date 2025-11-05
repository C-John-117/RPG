using MyLittleRpg.Models;


namespace MyLittleRpg.Services
{
    public interface IInstanceMonstreService
    {
        Task<int> AssurerNombreAsync(int cible, CancellationToken ct = default(CancellationToken));
        Task<int> SupprimerTousLesMonstresAsync(CancellationToken ct = default(CancellationToken));
        Task<InstanceMonster> ObtenirAsync(int positionX, int positionY, CancellationToken ct = default(CancellationToken));
        Task<InstanceMonster> CreerAsync(int positionX, int positionY, int monstreId, int niveau, int pointsVieMax, CancellationToken ct = default(CancellationToken));

        int CalculerNiveauDepuisVilleProche(int x, int y, IEnumerable<Tuile> listeVilles);
        (int force, int defense, int pointsVieMax) CalculerStats(Monster monstreBase, int niveau);
    }
}
