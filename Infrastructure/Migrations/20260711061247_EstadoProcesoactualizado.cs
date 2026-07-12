using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EstadoProcesoactualizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avisos_Cartera_CarteraId",
                table: "Avisos");

            migrationBuilder.DropColumn(
                name: "EstadoProceso",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "EstadoProcesoId",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "EstaEnProcesoCoactivo",
                table: "Cartera");

            migrationBuilder.RenameColumn(
                name: "CarteraId",
                table: "Avisos",
                newName: "ProcesoId");

            migrationBuilder.RenameIndex(
                name: "IX_Avisos_CarteraId",
                table: "Avisos",
                newName: "IX_Avisos_ProcesoId");

            migrationBuilder.CreateTable(
                name: "HistorialEstadoProceso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcesoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "integer", nullable: false),
                    EstadoNuevo = table.Column<int>(type: "integer", nullable: false),
                    EsAutomatico = table.Column<bool>(type: "boolean", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadoProceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoProceso_Procesos_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "Procesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoProceso_ProcesoId",
                table: "HistorialEstadoProceso",
                column: "ProcesoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Avisos_Procesos_ProcesoId",
                table: "Avisos",
                column: "ProcesoId",
                principalTable: "Procesos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avisos_Procesos_ProcesoId",
                table: "Avisos");

            migrationBuilder.DropTable(
                name: "HistorialEstadoProceso");

            migrationBuilder.RenameColumn(
                name: "ProcesoId",
                table: "Avisos",
                newName: "CarteraId");

            migrationBuilder.RenameIndex(
                name: "IX_Avisos_ProcesoId",
                table: "Avisos",
                newName: "IX_Avisos_CarteraId");

            migrationBuilder.AddColumn<int>(
                name: "EstadoProceso",
                table: "Vehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstadoProcesoId",
                table: "Vehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EstaEnProcesoCoactivo",
                table: "Cartera",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Avisos_Cartera_CarteraId",
                table: "Avisos",
                column: "CarteraId",
                principalTable: "Cartera",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
