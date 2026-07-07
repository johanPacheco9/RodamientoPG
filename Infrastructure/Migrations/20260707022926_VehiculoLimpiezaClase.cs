using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VehiculoLimpiezaClase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoPropietario",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "TipoIdentificacionId",
                table: "Vehiculos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentoPropietario",
                table: "Vehiculos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TipoIdentificacionId",
                table: "Vehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
