using Microsoft.EntityFrameworkCore;

namespace MyLittleRpg.Models
{
    [PrimaryKey(nameof(PositionX), nameof(PositionY))]

    public class InstanceMonster
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int MonstreId { get; set; }
        public Monster? Monstre { get; set; }
        public int Niveau { get; set; }
        public int PointsVieMax { get; set; }
        public int PointsVieActuels { get; set; }

    }
}
