namespace MyLittleRpg.Models
{
    public class Monster
    {
        public int Id { get; set; }
        public int PokemonId { get; set; }
        public string? Nom { get; set; }
        public int PointsVieBase { get; set; }
        public int ForceBase { get; set; }
        public int DefenseBase { get; set; }
        public int ExperienceBase { get; set; }
        public string? SpriteURL { get; set; }
        public string? Type1 { get; set; }
        public string? Type2 { get; set; }

        public Monster() { }

        public Monster(int id, int PokeminId, string nom, int pointsVieBase, int forceBase, int defenseBase, int experienceBase, string spriteUrl, string type1, string type2)
        {
            Id = id;
            PokemonId = PokeminId;
            Nom = nom;
            PointsVieBase = pointsVieBase;
            ForceBase = forceBase;
            DefenseBase = defenseBase;
            ExperienceBase = experienceBase;
            SpriteURL = spriteUrl;
            Type1 = type1;
            Type2 = type2;
        }
    }
}

