using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReciboDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cartera_Recibos_ReciboId",
                table: "Cartera");

            migrationBuilder.DropTable(
                name: "ReciboVigencia");

            migrationBuilder.DropIndex(
                name: "IX_Cartera_ReciboId",
                table: "Cartera");

            migrationBuilder.DropColumn(
                name: "ReciboId",
                table: "Cartera");

            migrationBuilder.CreateTable(
                name: "ReciboDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReciboId = table.Column<int>(type: "integer", nullable: false),
                    CarteraId = table.Column<int>(type: "integer", nullable: false),
                    Vigencia = table.Column<int>(type: "integer", nullable: false),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorInteres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReciboDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReciboDetalle_Cartera_CarteraId",
                        column: x => x.CarteraId,
                        principalTable: "Cartera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReciboDetalle_Recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "Recibos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_CarteraId",
                table: "ReciboDetalle",
                column: "CarteraId");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_ReciboId",
                table: "ReciboDetalle",
                column: "ReciboId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReciboDetalle");

            migrationBuilder.AddColumn<int>(
                name: "ReciboId",
                table: "Cartera",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReciboVigencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReciboId = table.Column<int>(type: "integer", nullable: false),
                    Pagado = table.Column<bool>(type: "boolean", nullable: false),
                    Vigencia = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_Cartera_ReciboId",
                table: "Cartera",
                column: "ReciboId");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboVigencia_ReciboId",
                table: "ReciboVigencia",
                column: "ReciboId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cartera_Recibos_ReciboId",
                table: "Cartera",
                column: "ReciboId",
                principalTable: "Recibos",
                principalColumn: "Id");
        }
    }
}
