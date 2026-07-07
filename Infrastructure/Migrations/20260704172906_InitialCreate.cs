using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Nit = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Ciudad = table.Column<string>(type: "text", nullable: true),
                    Correo = table.Column<string>(type: "text", nullable: true),
                    CobraAdicional = table.Column<bool>(type: "boolean", nullable: false),
                    MetodoImpuesto = table.Column<int>(type: "integer", nullable: false),
                    FechaLimiteSancion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CuentaTransito = table.Column<string>(type: "text", nullable: true),
                    BancoTransito = table.Column<string>(type: "text", nullable: true),
                    CuentaTercero = table.Column<string>(type: "text", nullable: true),
                    BancoTercero = table.Column<string>(type: "text", nullable: true),
                    NombreSecretario = table.Column<string>(type: "text", nullable: true),
                    CargoSecretario = table.Column<string>(type: "text", nullable: true),
                    ValorRecibo = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorSistema = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorCostasPersuasivo = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorCostasCoactivo = table.Column<decimal>(type: "numeric", nullable: false),
                    PorcentajeSancion = table.Column<decimal>(type: "numeric", nullable: false),
                    PorcentajeInteresACobrar = table.Column<decimal>(type: "numeric", nullable: false),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametros", x => x.Id);
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
                    Correo = table.Column<string>(type: "text", nullable: true),
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
                    Tipo = table.Column<int>(type: "integer", nullable: false),
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
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "AvaluoVehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    ClaseVehiculo = table.Column<string>(type: "text", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: false),
                    Linea = table.Column<string>(type: "text", nullable: false),
                    Cilindraje = table.Column<int>(type: "integer", nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    Pasajeros = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaluoVehiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaluoVehiculos_TipoVehiculos_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "TipoVehiculos",
                        principalColumn: "Id",
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
                    TipoServicio = table.Column<int>(type: "integer", nullable: true),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    PropietarioId = table.Column<int>(type: "integer", nullable: false),
                    EstadoProceso = table.Column<int>(type: "integer", nullable: false)
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
                name: "AvaluoVigencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvaluoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    AnioVigencia = table.Column<int>(type: "integer", nullable: false),
                    ValorComercial = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorUvtVigencia = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaluoVigencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaluoVigencias_AvaluoVehiculos_AvaluoVehiculoId",
                        column: x => x.AvaluoVehiculoId,
                        principalTable: "AvaluoVehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    FechaProceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Desde = table.Column<int>(type: "integer", nullable: true),
                    Hasta = table.Column<int>(type: "integer", nullable: true),
                    ResolucionSancion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    EstadoProceso = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procesos", x => x.Id);
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
                    FechaLiquidacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VigenciaDesde = table.Column<int>(type: "integer", nullable: false),
                    VigenciaHasta = table.Column<int>(type: "integer", nullable: false),
                    UltimoPagoVigencia = table.Column<int>(type: "integer", nullable: true),
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
                name: "Resolucion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroResolucion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaProceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipoResolucion = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ProcesoId = table.Column<int>(type: "integer", nullable: true),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resolucion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resolucion_Procesos_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "Procesos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Resolucion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resolucion_Vehiculos_VehiculoId",
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
                    Concepto = table.Column<int>(type: "integer", nullable: false),
                    IsPagado = table.Column<bool>(type: "boolean", nullable: false),
                    IsAnulled = table.Column<bool>(type: "boolean", nullable: false),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    TieneInteres = table.Column<bool>(type: "boolean", nullable: false),
                    EstaEnProcesoCoactivo = table.Column<bool>(type: "boolean", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorInteres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ResolucionId = table.Column<int>(type: "integer", nullable: true),
                    usuario_creo = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_modifico = table.Column<int>(type: "integer", nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartera_Resolucion_ResolucionId",
                        column: x => x.ResolucionId,
                        principalTable: "Resolucion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cartera_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "LiquidacionDetalles",
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
                    table.PrimaryKey("PK_LiquidacionDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Cartera_CarteraId",
                        column: x => x.CarteraId,
                        principalTable: "Cartera",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Liquidacion_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReciboDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReciboId = table.Column<int>(type: "integer", nullable: false),
                    CarteraId = table.Column<int>(type: "integer", nullable: false),
                    Vigencia = table.Column<int>(type: "integer", nullable: false),
                    Concepto = table.Column<int>(type: "integer", nullable: false),
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
                name: "IX_AvaluoVehiculos_TipoVehiculoId",
                table: "AvaluoVehiculos",
                column: "TipoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaluoVigencias_AvaluoVehiculoId",
                table: "AvaluoVigencias",
                column: "AvaluoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_CarteraId",
                table: "Avisos",
                column: "CarteraId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseGravableVigencia_BaseGravableVehiculoId",
                table: "BaseGravableVigencia",
                column: "BaseGravableVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartera_ResolucionId",
                table: "Cartera",
                column: "ResolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartera_VehiculoId",
                table: "Cartera",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPropietarios_PropietarioId",
                table: "HistorialPropietarios",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPropietarios_VehiculoId",
                table: "HistorialPropietarios",
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
                name: "IX_LiquidacionDetalles_CarteraId",
                table: "LiquidacionDetalles",
                column: "CarteraId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalles_LiquidacionId",
                table: "LiquidacionDetalles",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Procesos_VehiculoId",
                table: "Procesos",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_CarteraId",
                table: "ReciboDetalle",
                column: "CarteraId");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboDetalle_ReciboId",
                table: "ReciboDetalle",
                column: "ReciboId");

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_VehiculoId",
                table: "Recibos",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Resolucion_ProcesoId",
                table: "Resolucion",
                column: "ProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Resolucion_UsuarioId",
                table: "Resolucion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Resolucion_VehiculoId",
                table: "Resolucion",
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
                name: "AvaluoVigencias");

            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.DropTable(
                name: "BaseGravableVigencia");

            migrationBuilder.DropTable(
                name: "Descuentos");

            migrationBuilder.DropTable(
                name: "HistorialPropietarios");

            migrationBuilder.DropTable(
                name: "Intereses");

            migrationBuilder.DropTable(
                name: "LiquidacionDetalles");

            migrationBuilder.DropTable(
                name: "Parametros");

            migrationBuilder.DropTable(
                name: "ReciboDetalle");

            migrationBuilder.DropTable(
                name: "Tarifas");

            migrationBuilder.DropTable(
                name: "Uvts");

            migrationBuilder.DropTable(
                name: "AvaluoVehiculos");

            migrationBuilder.DropTable(
                name: "BaseGravableVehiculos");

            migrationBuilder.DropTable(
                name: "Liquidacion");

            migrationBuilder.DropTable(
                name: "Cartera");

            migrationBuilder.DropTable(
                name: "Recibos");

            migrationBuilder.DropTable(
                name: "Resolucion");

            migrationBuilder.DropTable(
                name: "Procesos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Colores");

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
