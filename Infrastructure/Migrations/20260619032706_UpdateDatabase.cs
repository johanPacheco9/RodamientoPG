using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Procesos_EstadoProcesos_EstadoProcesoId",
                table: "Procesos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_EstadoProcesos_EstadoProcesoId",
                table: "Vehiculos");

            migrationBuilder.DropTable(
                name: "EstadoProcesos");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculos_EstadoProcesoId",
                table: "Vehiculos");

            migrationBuilder.DropIndex(
                name: "IX_Procesos_EstadoProcesoId",
                table: "Procesos");

            migrationBuilder.DropColumn(
                name: "Avaluo",
                table: "Liquidacion");

            migrationBuilder.RenameColumn(
                name: "EstadoProcesoId",
                table: "Procesos",
                newName: "EstadoProceso");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Parametros",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "nit",
                table: "Parametros",
                newName: "Nit");

            migrationBuilder.RenameColumn(
                name: "correo",
                table: "Parametros",
                newName: "Correo");

            migrationBuilder.RenameColumn(
                name: "ciudad",
                table: "Parametros",
                newName: "Ciudad");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Parametros",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vlr_sistem",
                table: "Parametros",
                newName: "ValorSistema");

            migrationBuilder.RenameColumn(
                name: "vlr_recibo",
                table: "Parametros",
                newName: "ValorRecibo");

            migrationBuilder.RenameColumn(
                name: "tel",
                table: "Parametros",
                newName: "Telefono");

            migrationBuilder.RenameColumn(
                name: "porc_sancion",
                table: "Parametros",
                newName: "PorcentajeSancion");

            migrationBuilder.RenameColumn(
                name: "nom_secretario",
                table: "Parametros",
                newName: "NombreSecretario");

            migrationBuilder.RenameColumn(
                name: "metodo_impuesto",
                table: "Parametros",
                newName: "MetodoImpuesto");

            migrationBuilder.RenameColumn(
                name: "fecha_limite_san",
                table: "Parametros",
                newName: "FechaLimiteSancion");

            migrationBuilder.RenameColumn(
                name: "dir",
                table: "Parametros",
                newName: "Direccion");

            migrationBuilder.RenameColumn(
                name: "cuenta_transito",
                table: "Parametros",
                newName: "CuentaTransito");

            migrationBuilder.RenameColumn(
                name: "cuenta_tercero",
                table: "Parametros",
                newName: "CuentaTercero");

            migrationBuilder.RenameColumn(
                name: "cobra_adicional",
                table: "Parametros",
                newName: "CobraAdicional");

            migrationBuilder.RenameColumn(
                name: "cargo_secretario",
                table: "Parametros",
                newName: "CargoSecretario");

            migrationBuilder.RenameColumn(
                name: "banco_transito",
                table: "Parametros",
                newName: "BancoTransito");

            migrationBuilder.RenameColumn(
                name: "banco_tercero",
                table: "Parametros",
                newName: "BancoTercero");

            migrationBuilder.AddColumn<int>(
                name: "EstadoProceso",
                table: "Vehiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorSistema",
                table: "Parametros",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorRecibo",
                table: "Parametros",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PorcentajeSancion",
                table: "Parametros",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorCostasCoactivo",
                table: "Parametros",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorCostasPersuasivo",
                table: "Parametros",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "UltimoPagoVigencia",
                table: "Liquidacion",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLiquidacion",
                table: "Liquidacion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "HistorialPropietarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    PropietarioId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialPropietarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialPropietarios_Propietarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "Propietarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialPropietarios_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    CarteraId = table.Column<int>(type: "integer", nullable: true),
                    Vigencia = table.Column<int>(type: "integer", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorInteres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalle_Cartera_CarteraId",
                        column: x => x.CarteraId,
                        principalTable: "Cartera",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalle_Liquidacion_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPropietarios_PropietarioId",
                table: "HistorialPropietarios",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPropietarios_VehiculoId",
                table: "HistorialPropietarios",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalle_CarteraId",
                table: "LiquidacionDetalle",
                column: "CarteraId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalle_LiquidacionId",
                table: "LiquidacionDetalle",
                column: "LiquidacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialPropietarios");

            migrationBuilder.DropTable(
                name: "LiquidacionDetalle");

            migrationBuilder.DropColumn(
                name: "EstadoProceso",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ValorCostasCoactivo",
                table: "Parametros");

            migrationBuilder.DropColumn(
                name: "ValorCostasPersuasivo",
                table: "Parametros");

            migrationBuilder.DropColumn(
                name: "FechaLiquidacion",
                table: "Liquidacion");

            migrationBuilder.RenameColumn(
                name: "EstadoProceso",
                table: "Procesos",
                newName: "EstadoProcesoId");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Parametros",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "Nit",
                table: "Parametros",
                newName: "nit");

            migrationBuilder.RenameColumn(
                name: "Correo",
                table: "Parametros",
                newName: "correo");

            migrationBuilder.RenameColumn(
                name: "Ciudad",
                table: "Parametros",
                newName: "ciudad");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Parametros",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ValorSistema",
                table: "Parametros",
                newName: "vlr_sistem");

            migrationBuilder.RenameColumn(
                name: "ValorRecibo",
                table: "Parametros",
                newName: "vlr_recibo");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "Parametros",
                newName: "tel");

            migrationBuilder.RenameColumn(
                name: "PorcentajeSancion",
                table: "Parametros",
                newName: "porc_sancion");

            migrationBuilder.RenameColumn(
                name: "NombreSecretario",
                table: "Parametros",
                newName: "nom_secretario");

            migrationBuilder.RenameColumn(
                name: "MetodoImpuesto",
                table: "Parametros",
                newName: "metodo_impuesto");

            migrationBuilder.RenameColumn(
                name: "FechaLimiteSancion",
                table: "Parametros",
                newName: "fecha_limite_san");

            migrationBuilder.RenameColumn(
                name: "Direccion",
                table: "Parametros",
                newName: "dir");

            migrationBuilder.RenameColumn(
                name: "CuentaTransito",
                table: "Parametros",
                newName: "cuenta_transito");

            migrationBuilder.RenameColumn(
                name: "CuentaTercero",
                table: "Parametros",
                newName: "cuenta_tercero");

            migrationBuilder.RenameColumn(
                name: "CobraAdicional",
                table: "Parametros",
                newName: "cobra_adicional");

            migrationBuilder.RenameColumn(
                name: "CargoSecretario",
                table: "Parametros",
                newName: "cargo_secretario");

            migrationBuilder.RenameColumn(
                name: "BancoTransito",
                table: "Parametros",
                newName: "banco_transito");

            migrationBuilder.RenameColumn(
                name: "BancoTercero",
                table: "Parametros",
                newName: "banco_tercero");

            migrationBuilder.AlterColumn<decimal>(
                name: "vlr_sistem",
                table: "Parametros",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "vlr_recibo",
                table: "Parametros",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "porc_sancion",
                table: "Parametros",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "UltimoPagoVigencia",
                table: "Liquidacion",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Avaluo",
                table: "Liquidacion",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EstadoProcesos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Costas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoProcesos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_EstadoProcesoId",
                table: "Vehiculos",
                column: "EstadoProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Procesos_EstadoProcesoId",
                table: "Procesos",
                column: "EstadoProcesoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Procesos_EstadoProcesos_EstadoProcesoId",
                table: "Procesos",
                column: "EstadoProcesoId",
                principalTable: "EstadoProcesos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_EstadoProcesos_EstadoProcesoId",
                table: "Vehiculos",
                column: "EstadoProcesoId",
                principalTable: "EstadoProcesos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
