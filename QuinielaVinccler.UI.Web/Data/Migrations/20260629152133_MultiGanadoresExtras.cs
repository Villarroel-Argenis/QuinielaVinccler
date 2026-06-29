using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuinielaVinccler.UI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MultiGanadoresExtras : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar columnas viejas
            migrationBuilder.DropColumn(table: "ResultadoFinal", name: "MasGoleadorEquipoId");
            migrationBuilder.DropColumn(table: "ResultadoFinal", name: "MasGoleadoEquipoId");
            migrationBuilder.DropColumn(table: "ResultadoFinal", name: "MenosGoleadoEquipoId");

            // Agregar columnas nuevas
            migrationBuilder.AddColumn<string>(
                name: "MasGoleadorIds",
                table: "ResultadoFinal",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MasGoleadoIds",
                table: "ResultadoFinal",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenosGoleadoIds",
                table: "ResultadoFinal",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(table: "ResultadoFinal", name: "MasGoleadorIds");
            migrationBuilder.DropColumn(table: "ResultadoFinal", name: "MasGoleadoIds");
            migrationBuilder.DropColumn(table: "ResultadoFinal", name: "MenosGoleadoIds");

            migrationBuilder.AddColumn<int>(
                name: "MasGoleadorEquipoId",
                table: "ResultadoFinal",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MasGoleadoEquipoId",
                table: "ResultadoFinal",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MenosGoleadoEquipoId",
                table: "ResultadoFinal",
                nullable: true);
        }
    }
}
