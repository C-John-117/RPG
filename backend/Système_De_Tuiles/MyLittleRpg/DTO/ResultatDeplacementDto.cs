namespace MyLittleRpg.DTO
{
    public enum ResultatDeplacement
    {
        Deplacer,
        BloquerMontrer,
        TeleporterAlaMaison
    }
    public class ResultatDeplacementDto
    {
        public ResultatDeplacement resultat { get; set; }
        public PositionDto NouvellePosition { get; set; } = null!;
        public TuileDto? TuileCourante { get; set; }
        public CombatRapportDto? Combat { get; set; }
        public PersonnageDto Joueur { get; set; } = null!;
    }

    public class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
