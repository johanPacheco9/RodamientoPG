using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedBaseGravable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseGravableVigencia_BaseGravableVehiculos_BaseGravableVehi~",
                table: "BaseGravableVigencia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaseGravableVigencia",
                table: "BaseGravableVigencia");

            migrationBuilder.DropIndex(
                name: "IX_BaseGravableVigencia_BaseGravableVehiculoId",
                table: "BaseGravableVigencia");

            migrationBuilder.DropColumn(
                name: "ClaseVehiculo",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropColumn(
                name: "Linea",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "BaseGravableVehiculos");

            migrationBuilder.RenameTable(
                name: "BaseGravableVigencia",
                newName: "BaseGravableVigencias");

            migrationBuilder.RenameColumn(
                name: "ValorComercial",
                table: "BaseGravableVigencias",
                newName: "Valor");

            migrationBuilder.RenameColumn(
                name: "AnioVigencia",
                table: "BaseGravableVigencias",
                newName: "Vigencia");

            migrationBuilder.AddColumn<int>(
                name: "LineaId",
                table: "BaseGravableVehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarcaId",
                table: "BaseGravableVehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoVehiculoId",
                table: "BaseGravableVehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Modelo",
                table: "BaseGravableVigencias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaseGravableVigencias",
                table: "BaseGravableVigencias",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVehiculos_Codigo_MarcaId_LineaId_Cilindraje",
                table: "BaseGravableVehiculos",
                columns: new[] { "Codigo", "MarcaId", "LineaId", "Cilindraje" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVehiculos_LineaId",
                table: "BaseGravableVehiculos",
                column: "LineaId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVehiculos_MarcaId",
                table: "BaseGravableVehiculos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVehiculos_TipoVehiculoId",
                table: "BaseGravableVehiculos",
                column: "TipoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVigencias_BaseGravableVehiculoId_Vigencia_Modelo",
                table: "BaseGravableVigencias",
                columns: new[] { "BaseGravableVehiculoId", "Vigencia", "Modelo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseGravableVehiculos_Lineas_LineaId",
                table: "BaseGravableVehiculos",
                column: "LineaId",
                principalTable: "Lineas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseGravableVehiculos_Marcas_MarcaId",
                table: "BaseGravableVehiculos",
                column: "MarcaId",
                principalTable: "Marcas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseGravableVehiculos_TipoVehiculos_TipoVehiculoId",
                table: "BaseGravableVehiculos",
                column: "TipoVehiculoId",
                principalTable: "TipoVehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseGravableVigencias_BaseGravableVehiculos_BaseGravableVeh~",
                table: "BaseGravableVigencias",
                column: "BaseGravableVehiculoId",
                principalTable: "BaseGravableVehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseGravableVehiculos_Lineas_LineaId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseGravableVehiculos_Marcas_MarcaId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseGravableVehiculos_TipoVehiculos_TipoVehiculoId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseGravableVigencias_BaseGravableVehiculos_BaseGravableVeh~",
                table: "BaseGravableVigencias");

            migrationBuilder.DropIndex(
                name: "IX_BaseGravableVehiculos_Codigo_MarcaId_LineaId_Cilindraje",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropIndex(
                name: "IX_BaseGravableVehiculos_LineaId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropIndex(
                name: "IX_BaseGravableVehiculos_MarcaId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropIndex(
                name: "IX_BaseGravableVehiculos_TipoVehiculoId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaseGravableVigencias",
                table: "BaseGravableVigencias");

            migrationBuilder.DropIndex(
                name: "IX_BaseGravableVigencias_BaseGravableVehiculoId_Vigencia_Modelo",
                table: "BaseGravableVigencias");

            migrationBuilder.DropColumn(
                name: "LineaId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropColumn(
                name: "MarcaId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropColumn(
                name: "TipoVehiculoId",
                table: "BaseGravableVehiculos");

            migrationBuilder.DropColumn(
                name: "Modelo",
                table: "BaseGravableVigencias");

            migrationBuilder.RenameTable(
                name: "BaseGravableVigencias",
                newName: "BaseGravableVigencia");

            migrationBuilder.RenameColumn(
                name: "Vigencia",
                table: "BaseGravableVigencia",
                newName: "AnioVigencia");

            migrationBuilder.RenameColumn(
                name: "Valor",
                table: "BaseGravableVigencia",
                newName: "ValorComercial");

            migrationBuilder.AddColumn<string>(
                name: "ClaseVehiculo",
                table: "BaseGravableVehiculos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Linea",
                table: "BaseGravableVehiculos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "BaseGravableVehiculos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaseGravableVigencia",
                table: "BaseGravableVigencia",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVigencia_BaseGravableVehiculoId",
                table: "BaseGravableVigencia",
                column: "BaseGravableVehiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseGravableVigencia_BaseGravableVehiculos_BaseGravableVehi~",
                table: "BaseGravableVigencia",
                column: "BaseGravableVehiculoId",
                principalTable: "BaseGravableVehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
