using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AcuerdosDePagoRecibo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuotaAcuerdoPagoId",
                table: "Recibos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_CuotaAcuerdoPagoId",
                table: "Recibos",
                column: "CuotaAcuerdoPagoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recibos_CuotasAcuerdoDePagos_CuotaAcuerdoPagoId",
                table: "Recibos",
                column: "CuotaAcuerdoPagoId",
                principalTable: "CuotasAcuerdoDePagos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recibos_CuotasAcuerdoDePagos_CuotaAcuerdoPagoId",
                table: "Recibos");

            migrationBuilder.DropIndex(
                name: "IX_Recibos_CuotaAcuerdoPagoId",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "CuotaAcuerdoPagoId",
                table: "Recibos");
        }
    }
}
