using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Avisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Propietarios",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Avisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarteraId = table.Column<int>(type: "integer", nullable: false),
                    NumeroAviso = table.Column<int>(type: "integer", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumeroGuia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RutaPdf = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avisos_Cartera_CarteraId",
                        column: x => x.CarteraId,
                        principalTable: "Cartera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_CarteraId",
                table: "Avisos",
                column: "CarteraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Propietarios");
        }
    }
}
