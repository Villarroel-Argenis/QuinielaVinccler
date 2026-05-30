using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuinielaVinccler.UI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrediccionySeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planillas_Lotes_LoteId",
                table: "Planillas");

            migrationBuilder.DropForeignKey(
                name: "FK_Planillas_Users_UserId",
                table: "Planillas");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "Planillas");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Planillas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PuntajeTotal",
                table: "Planillas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    CodigoIso = table.Column<string>(type: "text", nullable: false),
                    Grupo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Partidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroPartido = table.Column<int>(type: "integer", nullable: false),
                    Fase = table.Column<string>(type: "text", nullable: false),
                    FechaHoraUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlotLocal = table.Column<string>(type: "text", nullable: false),
                    SlotVisitante = table.Column<string>(type: "text", nullable: false),
                    EquipoLocalId = table.Column<int>(type: "integer", nullable: true),
                    EquipoVisitanteId = table.Column<int>(type: "integer", nullable: true),
                    ResultadoGrupo = table.Column<string>(type: "text", nullable: true),
                    EquipoGanadorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partidos_Equipos_EquipoGanadorId",
                        column: x => x.EquipoGanadorId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Partidos_Equipos_EquipoLocalId",
                        column: x => x.EquipoLocalId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Partidos_Equipos_EquipoVisitanteId",
                        column: x => x.EquipoVisitanteId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PrediccionesFinal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanillaId = table.Column<int>(type: "integer", nullable: false),
                    CampeonEquipoId = table.Column<int>(type: "integer", nullable: true),
                    SegundoLugarEquipoId = table.Column<int>(type: "integer", nullable: true),
                    TercerLugarEquipoId = table.Column<int>(type: "integer", nullable: true),
                    CuartoLugarEquipoId = table.Column<int>(type: "integer", nullable: true),
                    MasGoleadorEquipoId = table.Column<int>(type: "integer", nullable: true),
                    MasGoleadoEquipoId = table.Column<int>(type: "integer", nullable: true),
                    MenosGoleadoEquipoId = table.Column<int>(type: "integer", nullable: true),
                    GolesLocalGranFinal = table.Column<int>(type: "integer", nullable: true),
                    GolesVisitanteGranFinal = table.Column<int>(type: "integer", nullable: true),
                    GolesLocalSemi1 = table.Column<int>(type: "integer", nullable: true),
                    GolesVisitanteSemi1 = table.Column<int>(type: "integer", nullable: true),
                    GolesLocalSemi2 = table.Column<int>(type: "integer", nullable: true),
                    GolesVisitanteSemi2 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrediccionesFinal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_CampeonEquipoId",
                        column: x => x.CampeonEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_CuartoLugarEquipoId",
                        column: x => x.CuartoLugarEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_MasGoleadoEquipoId",
                        column: x => x.MasGoleadoEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_MasGoleadorEquipoId",
                        column: x => x.MasGoleadorEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_MenosGoleadoEquipoId",
                        column: x => x.MenosGoleadoEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_SegundoLugarEquipoId",
                        column: x => x.SegundoLugarEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Equipos_TercerLugarEquipoId",
                        column: x => x.TercerLugarEquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesFinal_Planillas_PlanillaId",
                        column: x => x.PlanillaId,
                        principalTable: "Planillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrediccionesGrupo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanillaId = table.Column<int>(type: "integer", nullable: false),
                    PartidoId = table.Column<int>(type: "integer", nullable: false),
                    ResultadoPredicho = table.Column<string>(type: "text", nullable: true),
                    PuntosObtenidos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrediccionesGrupo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrediccionesGrupo_Partidos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "Partidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrediccionesGrupo_Planillas_PlanillaId",
                        column: x => x.PlanillaId,
                        principalTable: "Planillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrediccionesKnockout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanillaId = table.Column<int>(type: "integer", nullable: false),
                    PartidoId = table.Column<int>(type: "integer", nullable: false),
                    EquipoPredichoId = table.Column<int>(type: "integer", nullable: true),
                    PuntosObtenidos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrediccionesKnockout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrediccionesKnockout_Equipos_EquipoPredichoId",
                        column: x => x.EquipoPredichoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrediccionesKnockout_Partidos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "Partidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrediccionesKnockout_Planillas_PlanillaId",
                        column: x => x.PlanillaId,
                        principalTable: "Planillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Planillas_Codigo",
                table: "Planillas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Nombre",
                table: "Equipos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_EquipoGanadorId",
                table: "Partidos",
                column: "EquipoGanadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_EquipoLocalId",
                table: "Partidos",
                column: "EquipoLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_EquipoVisitanteId",
                table: "Partidos",
                column: "EquipoVisitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_Fase",
                table: "Partidos",
                column: "Fase");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_NumeroPartido",
                table: "Partidos",
                column: "NumeroPartido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_CampeonEquipoId",
                table: "PrediccionesFinal",
                column: "CampeonEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_CuartoLugarEquipoId",
                table: "PrediccionesFinal",
                column: "CuartoLugarEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_MasGoleadoEquipoId",
                table: "PrediccionesFinal",
                column: "MasGoleadoEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_MasGoleadorEquipoId",
                table: "PrediccionesFinal",
                column: "MasGoleadorEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_MenosGoleadoEquipoId",
                table: "PrediccionesFinal",
                column: "MenosGoleadoEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_PlanillaId",
                table: "PrediccionesFinal",
                column: "PlanillaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_SegundoLugarEquipoId",
                table: "PrediccionesFinal",
                column: "SegundoLugarEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesFinal_TercerLugarEquipoId",
                table: "PrediccionesFinal",
                column: "TercerLugarEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesGrupo_PartidoId",
                table: "PrediccionesGrupo",
                column: "PartidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesGrupo_PlanillaId_PartidoId",
                table: "PrediccionesGrupo",
                columns: new[] { "PlanillaId", "PartidoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesKnockout_EquipoPredichoId",
                table: "PrediccionesKnockout",
                column: "EquipoPredichoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesKnockout_PartidoId",
                table: "PrediccionesKnockout",
                column: "PartidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesKnockout_PlanillaId_PartidoId",
                table: "PrediccionesKnockout",
                columns: new[] { "PlanillaId", "PartidoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Planillas_Lotes_LoteId",
                table: "Planillas",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Planillas_Users_UserId",
                table: "Planillas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planillas_Lotes_LoteId",
                table: "Planillas");

            migrationBuilder.DropForeignKey(
                name: "FK_Planillas_Users_UserId",
                table: "Planillas");

            migrationBuilder.DropTable(
                name: "PrediccionesFinal");

            migrationBuilder.DropTable(
                name: "PrediccionesGrupo");

            migrationBuilder.DropTable(
                name: "PrediccionesKnockout");

            migrationBuilder.DropTable(
                name: "Partidos");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropIndex(
                name: "IX_Planillas_Codigo",
                table: "Planillas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Planillas");

            migrationBuilder.DropColumn(
                name: "PuntajeTotal",
                table: "Planillas");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "Planillas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Planillas_Lotes_LoteId",
                table: "Planillas",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Planillas_Users_UserId",
                table: "Planillas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
