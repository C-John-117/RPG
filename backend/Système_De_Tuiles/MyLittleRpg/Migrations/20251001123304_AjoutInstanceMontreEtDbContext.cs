using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLittleRpg.Migrations
{
    /// <inheritdoc />
    public partial class AjoutInstanceMontreEtDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "domicileX",
                table: "Personnages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "domicileY",
                table: "Personnages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InstanceMonster",
                columns: table => new
                {
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    MonstreId = table.Column<int>(type: "int", nullable: false),
                    Niveau = table.Column<int>(type: "int", nullable: false),
                    PointsVieMax = table.Column<int>(type: "int", nullable: false),
                    PointsVieActuels = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceMonster", x => new { x.PositionX, x.PositionY });
                    table.ForeignKey(
                        name: "FK_InstanceMonster_Monster_MonstreId",
                        column: x => x.MonstreId,
                        principalTable: "Monster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstanceMonster_Tuiles_PositionX_PositionY",
                        columns: x => new { x.PositionX, x.PositionY },
                        principalTable: "Tuiles",
                        principalColumns: new[] { "PositionX", "PositionY" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InstanceMonster_MonstreId",
                table: "InstanceMonster",
                column: "MonstreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstanceMonster");

            migrationBuilder.DropColumn(
                name: "domicileX",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "domicileY",
                table: "Personnages");
        }
    }
}
