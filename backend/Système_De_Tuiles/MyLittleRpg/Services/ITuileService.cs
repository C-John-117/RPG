using MyLittleRpg.Models;

namespace MyLittleRpg.Services
{
    public interface ITuileService
    {
        Task<Tuile> ObtenirOuCreerAsync(int x, int y, CancellationToken ct = default);
        bool EstTraversable(Tuile.TypeTuile type);
        string ImageDe(Tuile.TypeTuile type);
    }
}
