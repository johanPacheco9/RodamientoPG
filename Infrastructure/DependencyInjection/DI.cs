using Infrastructure.Services.Carteras;
using Infrastructure.Services.Colores;
using Infrastructure.Services.EmailNotification;
using Infrastructure.Services.Importados;
using Infrastructure.Services.Intereses;
using Infrastructure.Services.Lineas;
using Infrastructure.Services.Login;
using Infrastructure.Services.Liquidaciones;
using Infrastructure.Services.Marcas;
using Infrastructure.Services.Pagos;
using Infrastructure.Services.Parametros;
using Infrastructure.Services.Procesos.Persuasivo;
using Infrastructure.Services.Propietarios;
using Infrastructure.Services.Rec2ibos;
using Infrastructure.Services.Reportes;
using Infrastructure.Services.Resoluciones;
using Infrastructure.Services.Tarifas;
using Infrastructure.Services.TiposVehiculos;
using Infrastructure.Services.Traspasos;
using Infrastructure.Services.Users;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Services.Uvts;
using Infrastructure.Services.Vehiculos;
using CoactivoService = Infrastructure.Services.Procesos.Coactivo.CoactivoService;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 🔐 Usuarios y Seguridad (Transient)
        services.AddTransient<UserService>();
        services.AddTransient<LoginService>();

        // 🚦 Vehículos y Componentes
        services.AddTransient<VehiculosService>();
        services.AddTransient<LineasService>();
        services.AddTransient<MarcaService>();
        services.AddTransient<ColoresService>();
        services.AddTransient<TiposService>();
        services.AddTransient<PropietarioService>();

        // 💰 Liquidación, Tarifas e Impuestos (Transient)
        services.AddTransient<UvtService>();
        services.AddTransient<InteresService>();
        services.AddTransient<TarifaService>();
        services.AddTransient<LiquidacionService>();
        services.AddTransient<PagoService>();
        services.AddTransient<CarteraService>();

        // ⚖️ Procesos Coactivos e Importaciones (Transient)
        services.AddTransient<CoactivoService>();
        services.AddTransient<PersuasivoService>();
        services.AddTransient<ImportadosService>();
        services.AddTransient<ParametroService>();
        services.AddTransient<TraspasoManager>();
        services.AddTransient<ResolucionService>();
        services.AddTransient<ReportesManager>();
        //Recibo
        services.AddTransient<ReciboService>();
        //Envio emails
        services.AddScoped<EmailService>();
        return services;
    }
}
