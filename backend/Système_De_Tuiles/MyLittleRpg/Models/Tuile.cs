using Microsoft.EntityFrameworkCore;

namespace MyLittleRpg.Models
{
    [PrimaryKey(nameof(PositionX), nameof(PositionY))]
    public class Tuile
    {
        public enum TypeTuile
        {
            HERBE,
            EAU,
            MONTAGNE,
            FORET,
            VILLE,
            ROUTE
        }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public TypeTuile Type { get; set; }
        public bool EstTraversable { get; set; }
        public string? ImageUrl { get; set; }

        public Tuile() { }

        public Tuile( int positionX, int positionY, TypeTuile type, bool estTraversable, string imageUrl)
        {
            PositionX = positionX;
            PositionY = positionY;
            Type = type;
            EstTraversable = estTraversable;
            ImageUrl = imageUrl;
        }
    }
}
