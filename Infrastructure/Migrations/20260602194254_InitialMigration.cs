using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseGravableVehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    ClaseVehiculo = table.Column<string>(type: "text", nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: false),
                    Linea = table.Column<string>(type: "text", nullable: false),
                    Cilindraje = table.Column<int>(type: "integer", nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    Pasajeros = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseGravableVehiculos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Descuentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Desde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Hasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descuentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadoProcesos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Costas = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoProcesos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Intereses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Porcentaje = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Desde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Hasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intereses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Parametros",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nit = table.Column<string>(type: "text", nullable: true),
                    dir = table.Column<string>(type: "text", nullable: true),
                    tel = table.Column<string>(type: "text", nullable: true),
                    ciudad = table.Column<string>(type: "text", nullable: true),
                    correo = table.Column<string>(type: "text", nullable: true),
                    cobra_adicional = table.Column<bool>(type: "boolean", nullable: false),
                    metodo_impuesto = table.Column<int>(type: "integer", nullable: false),
                    fecha_limite_san = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cuenta_transito = table.Column<string>(type: "text", nullable: true),
                    banco_transito = table.Column<string>(type: "text", nullable: true),
                    cuenta_tercero = table.Column<string>(type: "text", nullable: true),
                    banco_tercero = table.Column<string>(type: "text", nullable: true),
                    nom_secretario = table.Column<string>(type: "text", nullable: true),
                    cargo_secretario = table.Column<string>(type: "text", nullable: true),
                    vlr_recibo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    vlr_sistem = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    porc_sancion = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Propietarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Documento = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    TipoDocumento = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propietarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoVehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModalidadServicio = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Uvt = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoVehiculos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsHabilitado = table.Column<bool>(type: "boolean", nullable: false),
                    Auth0Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Uvts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    indesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inhasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uvts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BaseGravableVigencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseGravableVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    AnioVigencia = table.Column<int>(type: "integer", nullable: false),
                    ValorComercial = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseGravableVigencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseGravableVigencia_BaseGravableVehiculos_BaseGravableVehi~",
                        column: x => x.BaseGravableVehiculoId,
                        principalTable: "BaseGravableVehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    IdMarca = table.Column<int>(type: "integer", nullable: false),
                    MarcaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lineas_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tarifas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnioFiscal = table.Column<int>(type: "integer", nullable: false),
                    RangoInicial = table.Column<int>(type: "integer", nullable: false),
                    RangoFinal = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    TipoServicioVehiculo = table.Column<int>(type: "integer", nullable: false),
                    ConceptoTarifa = table.Column<int>(type: "integer", nullable: false),
                    TipoServicio = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarifas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarifas_TipoVehiculos_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "TipoVehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Placa = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Modelo = table.Column<int>(type: "integer", nullable: false),
                    Cilindraje = table.Column<int>(type: "integer", nullable: false),
                    PagoHasta = table.Column<int>(type: "integer", nullable: false),
                    CapacidadCarga = table.Column<int>(type: "integer", nullable: false),
                    Pasajeros = table.Column<int>(type: "integer", nullable: false),
                    DocumentoPropietario = table.Column<string>(type: "text", nullable: false),
                    TipoIdentificacionId = table.Column<int>(type: "integer", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    MarcaId = table.Column<int>(type: "integer", nullable: false),
                    LineaId = table.Column<int>(type: "integer", nullable: false),
                    ColorId = table.Column<int>(type: "integer", nullable: false),
                    TipoServicioVehiculo = table.Column<int>(type: "integer", nullable: false),
                    TipoCarroceriaId = table.Column<int>(type: "integer", nullable: false),
                    EstadoProcesoId = table.Column<int>(type: "integer", nullable: false),
                    PropietarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Colores_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_EstadoProcesos_EstadoProcesoId",
                        column: x => x.EstadoProcesoId,
                        principalTable: "EstadoProcesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Lineas_LineaId",
                        column: x => x.LineaId,
                        principalTable: "Lineas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Propietarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "Propietarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_TipoVehiculos_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "TipoVehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Procesos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Resolucion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaMandamiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumeroProceso = table.Column<int>(type: "integer", nullable: true),
                    TipoProceso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FechaProceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Desde = table.Column<int>(type: "integer", nullable: true),
                    Hasta = table.Column<int>(type: "integer", nullable: true),
                    ResolucionSancion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoProcesoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Procesos_EstadoProcesos_EstadoProcesoId",
                        column: x => x.EstadoProcesoId,
                        principalTable: "EstadoProcesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Procesos_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recibos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaAplica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaProceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Desde = table.Column<int>(type: "integer", nullable: false),
                    Hasta = table.Column<int>(type: "integer", nullable: false),
                    ValorCapital = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InteresMora = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estampillas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotalSistema = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorCargaDatos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorRodamiento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recibos_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Liquidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    VigenciaDesde = table.Column<int>(type: "integer", nullable: false),
                    VigenciaHasta = table.Column<int>(type: "integer", nullable: false),
                    UltimoPagoVigencia = table.Column<int>(type: "integer", nullable: false),
                    Avaluo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDeuda = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProcesoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liquidacion_Procesos_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "Procesos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Liquidacion_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cartera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Placa = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Vigencia = table.Column<int>(type: "integer", nullable: false),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    IsPagado = table.Column<bool>(type: "boolean", nullable: false),
                    ReciboId = table.Column<int>(type: "integer", nullable: true),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    TieneInteres = table.Column<bool>(type: "boolean", nullable: false),
                    EstaEnProcesoCoactivo = table.Column<bool>(type: "boolean", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorInteres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartera_Recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "Recibos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cartera_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVigencia_BaseGravableVehiculoId",
                table: "BaseGravableVigencia",
                column: "BaseGravableVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartera_ReciboId",
                table: "Cartera",
                column: "ReciboId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartera_VehiculoId",
                table: "Cartera",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lineas_MarcaId",
                table: "Lineas",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidacion_ProcesoId",
                table: "Liquidacion",
                column: "ProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidacion_VehiculoId",
                table: "Liquidacion",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Procesos_EstadoProcesoId",
                table: "Procesos",
                column: "EstadoProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Procesos_VehiculoId",
                table: "Procesos",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_VehiculoId",
                table: "Recibos",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarifas_TipoVehiculoId",
                table: "Tarifas",
                column: "TipoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_ColorId",
                table: "Vehiculos",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_EstadoProcesoId",
                table: "Vehiculos",
                column: "EstadoProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_LineaId",
                table: "Vehiculos",
                column: "LineaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_MarcaId",
                table: "Vehiculos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Placa",
                table: "Vehiculos",
                column: "Placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_PropietarioId",
                table: "Vehiculos",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_TipoVehiculoId",
                table: "Vehiculos",
                column: "TipoVehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseGravableVigencia");

            migrationBuilder.DropTable(
                name: "Cartera");

            migrationBuilder.DropTable(
                name: "Descuentos");

            migrationBuilder.DropTable(
                name: "Intereses");

            migrationBuilder.DropTable(
                name: "Liquidacion");

            migrationBuilder.DropTable(
                name: "Parametros");

            migrationBuilder.DropTable(
                name: "Tarifas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Uvts");

            migrationBuilder.DropTable(
                name: "BaseGravableVehiculos");

            migrationBuilder.DropTable(
                name: "Recibos");

            migrationBuilder.DropTable(
                name: "Procesos");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Colores");

            migrationBuilder.DropTable(
                name: "EstadoProcesos");

            migrationBuilder.DropTable(
                name: "Lineas");

            migrationBuilder.DropTable(
                name: "Propietarios");

            migrationBuilder.DropTable(
                name: "TipoVehiculos");

            migrationBuilder.DropTable(
                name: "Marcas");
        }
    }
}
