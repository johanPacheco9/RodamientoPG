using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecibo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiquidacionDetalle_Cartera_CarteraId",
                table: "LiquidacionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_LiquidacionDetalle_Liquidacion_LiquidacionId",
                table: "LiquidacionDetalle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LiquidacionDetalle",
                table: "LiquidacionDetalle");

            migrationBuilder.DropColumn(
                name: "Desde",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Hasta",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "TipoProceso",
                table: "Procesos");

            migrationBuilder.RenameTable(
                name: "LiquidacionDetalle",
                newName: "LiquidacionDetalles");

            migrationBuilder.RenameIndex(
                name: "IX_LiquidacionDetalle_LiquidacionId",
                table: "LiquidacionDetalles",
                newName: "IX_LiquidacionDetalles_LiquidacionId");

            migrationBuilder.RenameIndex(
                name: "IX_LiquidacionDetalle_CarteraId",
                table: "LiquidacionDetalles",
                newName: "IX_LiquidacionDetalles_CarteraId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LiquidacionDetalles",
                table: "LiquidacionDetalles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ReciboVigencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReciboId = table.Column<int>(type: "integer", nullable: false),
                    Vigencia = table.Column<int>(type: "integer", nullable: false),
                    Pagado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReciboVigencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReciboVigencia_Recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "Recibos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReciboVigencia_ReciboId",
                table: "ReciboVigencia",
                column: "ReciboId");

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidacionDetalles_Cartera_CarteraId",
                table: "LiquidacionDetalles",
                column: "CarteraId",
                principalTable: "Cartera",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidacionDetalles_Liquidacion_LiquidacionId",
                table: "LiquidacionDetalles",
                column: "LiquidacionId",
                principalTable: "Liquidacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiquidacionDetalles_Cartera_CarteraId",
                table: "LiquidacionDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_LiquidacionDetalles_Liquidacion_LiquidacionId",
                table: "LiquidacionDetalles");

            migrationBuilder.DropTable(
                name: "ReciboVigencia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LiquidacionDetalles",
                table: "LiquidacionDetalles");

            migrationBuilder.RenameTable(
                name: "LiquidacionDetalles",
                newName: "LiquidacionDetalle");

            migrationBuilder.RenameIndex(
                name: "IX_LiquidacionDetalles_LiquidacionId",
                table: "LiquidacionDetalle",
                newName: "IX_LiquidacionDetalle_LiquidacionId");

            migrationBuilder.RenameIndex(
                name: "IX_LiquidacionDetalles_CarteraId",
                table: "LiquidacionDetalle",
                newName: "IX_LiquidacionDetalle_CarteraId");

            migrationBuilder.AddColumn<int>(
                name: "Desde",
                table: "Recibos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hasta",
                table: "Recibos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoProceso",
                table: "Procesos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LiquidacionDetalle",
                table: "LiquidacionDetalle",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidacionDetalle_Cartera_CarteraId",
                table: "LiquidacionDetalle",
                column: "CarteraId",
                principalTable: "Cartera",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LiquidacionDetalle_Liquidacion_LiquidacionId",
                table: "LiquidacionDetalle",
                column: "LiquidacionId",
                principalTable: "Liquidacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
