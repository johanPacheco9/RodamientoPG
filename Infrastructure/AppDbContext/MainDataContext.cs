using Domain.Models;
using Domain.Models.Avaluo;
using Domain.Models.BaseGravable;
using Domain.Models.Notificaciones;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Recibos;
using Domain.Models.Resoluciones;
using Domain.Models.Vehiculos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.AppDbContext;

public class MainDataContext(DbContextOptions<MainDataContext> options) : DbContext(options)
{
    public DbSet<Usuario>? Usuarios { get; set; }
    public DbSet<Proceso> Procesos { get; set; }
    public DbSet<Color> Colores { get; set; }
    public DbSet<BaseGravableVehiculo> BaseGravableVehiculos { get; set; }
    public DbSet<Recibo> Recibos { get; set; }
    public DbSet<ReciboDetalle> ReciboDetalle { get; set; }
    public DbSet<Interes> Intereses { get; set; }
    public DbSet<Linea> Lineas { get; set; }
    public DbSet<Liquidacion> Liquidacion { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Parametro> Parametros { get; set; }
    public DbSet<Descuento> Descuentos { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Cartera> Cartera { get; set; }
    public DbSet<Propietario> Propietarios { get; set; }
    public DbSet<Tarifa> Tarifas { get; set; }
    public DbSet<TipoVehiculo> TipoVehiculos { get; set; }
    public DbSet<Uvt> Uvts { get; set; }
    public DbSet<HistorialPropietario> HistorialPropietarios { get; set; }
    public DbSet<LiquidacionDetalle> LiquidacionDetalles  { get; set; }
    public DbSet<Resolucion> Resolucion { get; set; }
    public DbSet<Aviso> Avisos { get; set; }
    
    public DbSet<AvaluoVigencia>AvaluoVigencias { get; set; }
    
    public DbSet<AvaluoVehiculo>AvaluoVehiculos { get; set; }
    
    public DbSet<HistorialEstadoProceso> HistorialEstadoProceso {get; set;}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🛡️ Configurar Placa como Única
        modelBuilder.Entity<Vehiculo>()
            .HasIndex(v => v.Placa)
            .IsUnique();
    }
}
