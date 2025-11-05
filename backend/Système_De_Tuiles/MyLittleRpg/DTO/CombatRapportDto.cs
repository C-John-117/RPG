namespace MyLittleRpg.DTO
{
    public class CombatRapportDto
    {
        public int DegatsAuMonstre { get; set; }
        public int DegatsAuJoueur { get; set; }
        public int HpMonstreApres { get; set; }
        public int HpJoueurApres { get; set; }
        public bool JoueurVainqueur { get; set; }
        public bool MonstreVainqueur { get; set; }
    }
}
