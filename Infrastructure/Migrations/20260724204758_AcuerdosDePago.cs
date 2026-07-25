using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AcuerdosDePago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcuerdoPagoId",
                table: "Cartera",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcuerdosDePago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroAcuerdo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaSuscripcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumeroCuotas = table.Column<int>(type: "integer", nullable: false),
                    ValorCapitalFinanciado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorInteresFinanciado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorCuotaInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotalFinanciado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    ProcesoId = table.Column<int>(type: "integer", nullable: true),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcuerdosDePago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcuerdosDePago_Procesos_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "Procesos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcuerdosDePago_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuotasAcuerdoDePagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCuota = table.Column<int>(type: "integer", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorCapital = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorInteres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotalCuota = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NumeroRecibo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AcuerdoPagoId = table.Column<int>(type: "integer", nullable: false),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuotasAcuerdoDePagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuotasAcuerdoDePagos_AcuerdosDePago_AcuerdoPagoId",
                        column: x => x.AcuerdoPagoId,
                        principalTable: "AcuerdosDePago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cartera_AcuerdoPagoId",
                table: "Cartera",
                column: "AcuerdoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_AcuerdosDePago_ProcesoId",
                table: "AcuerdosDePago",
                column: "ProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_AcuerdosDePago_VehiculoId",
                table: "AcuerdosDePago",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuotasAcuerdoDePagos_AcuerdoPagoId",
                table: "CuotasAcuerdoDePagos",
                column: "AcuerdoPagoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cartera_AcuerdosDePago_AcuerdoPagoId",
                table: "Cartera",
                column: "AcuerdoPagoId",
                principalTable: "AcuerdosDePago",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cartera_AcuerdosDePago_AcuerdoPagoId",
                table: "Cartera");

            migrationBuilder.DropTable(
                name: "CuotasAcuerdoDePagos");

            migrationBuilder.DropTable(
                name: "AcuerdosDePago");

            migrationBuilder.DropIndex(
                name: "IX_Cartera_AcuerdoPagoId",
                table: "Cartera");

            migrationBuilder.DropColumn(
                name: "AcuerdoPagoId",
                table: "Cartera");
        }
    }
}
