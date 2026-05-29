using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuinielaVinccler.UI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResultadosYMarcadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Marcador exacto en Partido (Semis + Final) ────────────────────
            migrationBuilder.AddColumn<int>(
                name: "GolesLocal",
                table: "Partidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolesVisitante",
                table: "Partidos",
                type: "integer",
                nullable: true);

            // ── Puntos desglosados en PrediccionFinal ─────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "PuntosPosicionesFinal",
                table: "PrediccionesFinal",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuntosMarcadorSemi1",
                table: "PrediccionesFinal",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuntosMarcadorSemi2",
                table: "PrediccionesFinal",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuntosMarcadorGranFinal",
                table: "PrediccionesFinal",
                type: "integer",
                nullable: true);

            // ── Tabla ResultadoFinal (singleton) ──────────────────────────────
            migrationBuilder.CreateTable(
                name: "ResultadoFinal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    CampeonEquipoId = table.Column<int>(type: "integer", nullable: true),
                    SegundoLugarEquipoId = table.Column<int>(type: "integer", nullable: true),
                    TercerLugarEquipoId = table.Column<int>(type: "integer", nullable: true),
                    CuartoLugarEquipoId = table.Column<int>(type: "integer", nullable: true),
                    MasGoleadorEquipoId = table.Column<int>(type: "integer", nullable: true),
                    MasGoleadoEquipoId = table.Column<int>(type: "integer", nullable: true),
                    MenosGoleadoEquipoId = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoFinal", x => x.Id);

                    table.ForeignKey("FK_ResultadoFinal_Equipos_CampeonEquipoId",
                        x => x.CampeonEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_ResultadoFinal_Equipos_SegundoLugarEquipoId",
                        x => x.SegundoLugarEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_ResultadoFinal_Equipos_TercerLugarEquipoId",
                        x => x.TercerLugarEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_ResultadoFinal_Equipos_CuartoLugarEquipoId",
                        x => x.CuartoLugarEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_ResultadoFinal_Equipos_MasGoleadorEquipoId",
                        x => x.MasGoleadorEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_ResultadoFinal_Equipos_MasGoleadoEquipoId",
                        x => x.MasGoleadoEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_ResultadoFinal_Equipos_MenosGoleadoEquipoId",
                        x => x.MenosGoleadoEquipoId, "Equipos", "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Índices FK de ResultadoFinal
            migrationBuilder.CreateIndex("IX_ResultadoFinal_CampeonEquipoId",
                "ResultadoFinal", "CampeonEquipoId");
            migrationBuilder.CreateIndex("IX_ResultadoFinal_SegundoLugarEquipoId",
                "ResultadoFinal", "SegundoLugarEquipoId");
            migrationBuilder.CreateIndex("IX_ResultadoFinal_TercerLugarEquipoId",
                "ResultadoFinal", "TercerLugarEquipoId");
            migrationBuilder.CreateIndex("IX_ResultadoFinal_CuartoLugarEquipoId",
                "ResultadoFinal", "CuartoLugarEquipoId");
            migrationBuilder.CreateIndex("IX_ResultadoFinal_MasGoleadorEquipoId",
                "ResultadoFinal", "MasGoleadorEquipoId");
            migrationBuilder.CreateIndex("IX_ResultadoFinal_MasGoleadoEquipoId",
                "ResultadoFinal", "MasGoleadoEquipoId");
            migrationBuilder.CreateIndex("IX_ResultadoFinal_MenosGoleadoEquipoId",
                "ResultadoFinal", "MenosGoleadoEquipoId");

            // Seed singleton — siempre existe una sola fila con Id=1
            migrationBuilder.Sql(
                "INSERT INTO \"ResultadoFinal\" (\"Id\") VALUES (1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ResultadoFinal");

            migrationBuilder.DropColumn(name: "GolesLocal", table: "Partidos");
            migrationBuilder.DropColumn(name: "GolesVisitante", table: "Partidos");

            migrationBuilder.DropColumn(name: "PuntosPosicionesFinal", table: "PrediccionesFinal");
            migrationBuilder.DropColumn(name: "PuntosMarcadorSemi1", table: "PrediccionesFinal");
            migrationBuilder.DropColumn(name: "PuntosMarcadorSemi2", table: "PrediccionesFinal");
            migrationBuilder.DropColumn(name: "PuntosMarcadorGranFinal", table: "PrediccionesFinal");
        }
    }
}