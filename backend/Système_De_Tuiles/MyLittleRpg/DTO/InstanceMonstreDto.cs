namespace MyLittleRpg.DTO
{
    public class InstanceMonstreDto
    {
        public int X { get; set; }
        public int Y { get; set; }

        //les infos de mon monstre
        public int MonstreId { get; set; }
        public string Nom { get; set; } = "";
        public string? SpriteURL { get; set; }
        public string? Type1 { get; set; }
        public string? Type2 { get; set; }

        // Statisitques calculées
        public int Niveau { get; set; }
        public int PointsVieActuels { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
    }
}
