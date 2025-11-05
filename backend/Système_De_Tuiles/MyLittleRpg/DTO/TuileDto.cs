namespace MyLittleRpg.DTO
{
    public class TuileDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } = "";
        public bool EstTraversable { get; set; }
        public string? ImageUrl { get; set; }
        public InstanceMonstreDto? Monstre { get; set; } 
    }
}
