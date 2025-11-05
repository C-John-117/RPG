namespace MyLittleRpg.DTO
{
    public class PersonnageDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int DomicileX { get; set; }
        public int DomicileY { get; set; }
    }
}
