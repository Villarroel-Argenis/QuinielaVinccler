using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuinielaVinccler.UI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class KnockoutRediseno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoPredichoId",
                table: "PrediccionesKnockout");

            migrationBuilder.RenameColumn(
                name: "EquipoPredichoId",
                table: "PrediccionesKnockout",
                newName: "EquipoVisitantePredichoId");

            migrationBuilder.RenameIndex(
                name: "IX_PrediccionesKnockout_EquipoPredichoId",
                table: "PrediccionesKnockout",
                newName: "IX_PrediccionesKnockout_EquipoVisitantePredichoId");

            migrationBuilder.AddColumn<int>(
                name: "EquipoGanadorId",
                table: "PrediccionesKnockout",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EquipoLocalPredichoId",
                table: "PrediccionesKnockout",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesKnockout_EquipoGanadorId",
                table: "PrediccionesKnockout",
                column: "EquipoGanadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesKnockout_EquipoLocalPredichoId",
                table: "PrediccionesKnockout",
                column: "EquipoLocalPredichoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoGanadorId",
                table: "PrediccionesKnockout",
                column: "EquipoGanadorId",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoLocalPredichoId",
                table: "PrediccionesKnockout",
                column: "EquipoLocalPredichoId",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoVisitantePredichoId",
                table: "PrediccionesKnockout",
                column: "EquipoVisitantePredichoId",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoGanadorId",
                table: "PrediccionesKnockout");

            migrationBuilder.DropForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoLocalPredichoId",
                table: "PrediccionesKnockout");

            migrationBuilder.DropForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoVisitantePredichoId",
                table: "PrediccionesKnockout");

            migrationBuilder.DropIndex(
                name: "IX_PrediccionesKnockout_EquipoGanadorId",
                table: "PrediccionesKnockout");

            migrationBuilder.DropIndex(
                name: "IX_PrediccionesKnockout_EquipoLocalPredichoId",
                table: "PrediccionesKnockout");

            migrationBuilder.DropColumn(
                name: "EquipoGanadorId",
                table: "PrediccionesKnockout");

            migrationBuilder.DropColumn(
                name: "EquipoLocalPredichoId",
                table: "PrediccionesKnockout");

            migrationBuilder.RenameColumn(
                name: "EquipoVisitantePredichoId",
                table: "PrediccionesKnockout",
                newName: "EquipoPredichoId");

            migrationBuilder.RenameIndex(
                name: "IX_PrediccionesKnockout_EquipoVisitantePredichoId",
                table: "PrediccionesKnockout",
                newName: "IX_PrediccionesKnockout_EquipoPredichoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrediccionesKnockout_Equipos_EquipoPredichoId",
                table: "PrediccionesKnockout",
                column: "EquipoPredichoId",
                principalTable: "Equipos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
