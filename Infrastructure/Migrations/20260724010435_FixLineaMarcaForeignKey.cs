using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLineaMarcaForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lineas_Marcas_MarcaId",
                table: "Lineas");

            migrationBuilder.DropIndex(
                name: "IX_Lineas_MarcaId",
                table: "Lineas");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "Lineas");

            migrationBuilder.CreateIndex(
                name: "IX_Lineas_IdMarca",
                table: "Lineas",
                column: "IdMarca");

            migrationBuilder.AddForeignKey(
                name: "FK_Lineas_Marcas_IdMarca",
                table: "Lineas",
                column: "IdMarca",
                principalTable: "Marcas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lineas_Marcas_IdMarca",
                table: "Lineas");

            migrationBuilder.DropIndex(
                name: "IX_Lineas_IdMarca",
                table: "Lineas");

            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "Lineas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lineas_MarcaId",
                table: "Lineas",
                column: "MarcaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lineas_Marcas_MarcaId",
                table: "Lineas",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
