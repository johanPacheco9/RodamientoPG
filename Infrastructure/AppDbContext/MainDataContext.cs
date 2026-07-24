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
    
    public DbSet<BaseGravableVigencia> BaseGravableVigencias { get; set; } // 💡 Agregada la tabla de precios por vigencia
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
    public DbSet<LiquidacionDetalle> LiquidacionDetalles { get; set; }
    public DbSet<Resolucion> Resolucion { get; set; }
    public DbSet<Aviso> Avisos { get; set; }
    
    public DbSet<AvaluoVigencia> AvaluoVigencias { get; set; }
    public DbSet<AvaluoVehiculo> AvaluoVehiculos { get; set; }
    public DbSet<HistorialEstadoProceso> HistorialEstadoProceso { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🛡️ 1. Configurar Placa como Única en Vehículos Reales
        modelBuilder.Entity<Vehiculo>()
            .HasIndex(v => v.Placa)
            .IsUnique();

        // 🛡️ 2. Ficha Técnica de Base Gravable (Relaciones e Índices Únicos)
        modelBuilder.Entity<BaseGravableVehiculo>(entity =>
        {
            // Relación con Marca
            entity.HasOne(b => b.Marca)
                .WithMany()
                .HasForeignKey(b => b.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Línea
            entity.HasOne(b => b.Linea)
                .WithMany()
                .HasForeignKey(b => b.LineaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con TipoVehiculo (Clase)
            entity.HasOne(b => b.TipoVehiculo)
                .WithMany()
                .HasForeignKey(b => b.TipoVehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice Único: Evita duplicar la misma ficha técnica del Ministerio
            entity.HasIndex(b => new { b.Codigo, b.MarcaId, b.LineaId, b.Cilindraje })
                .IsUnique();
        });

        // 🛡️ 3. Precios de Base Gravable por Vigencia y Año Modelo
        modelBuilder.Entity<BaseGravableVigencia>(entity =>
        {
            // Relación 1 a Muchos (Padre BaseGravableVehiculo -> Hijo BaseGravableVigencia)
            entity.HasOne(bgv => bgv.BaseGravableVehiculo)
                .WithMany(bg => bg.Vigencias)
                .HasForeignKey(bgv => bgv.BaseGravableVehiculoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice Único: Un solo precio por Ficha Técnica + Vigencia Fiscal + Año Modelo
            entity.HasIndex(bgv => new { bgv.BaseGravableVehiculoId, bgv.Vigencia, AnoModelo = bgv.Modelo })
                .IsUnique();
        });
    }
}