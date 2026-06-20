using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Parametro
{
    [Key]
    public int Id { get; set; }
    
    public string Nombre { get; set; } = null!;
    public string? Nit { get; set; }
    
    public string? Direccion { get; set; }
    
    public string? Telefono { get; set; }
    
    public string? Ciudad { get; set; }
    
    public string? Correo { get; set; }
    
    public bool CobraAdicional { get; set; }
    
    public int MetodoImpuesto { get; set; }
    
    public DateTime FechaLimiteSancion { get; set; }
    
    public string? CuentaTransito { get; set; }
    public string? BancoTransito { get; set; }
    
    public string? CuentaTercero { get; set; }
    
    public string? BancoTercero { get; set; }
    
    public string? NombreSecretario { get; set; }
    
    public string? CargoSecretario { get; set; }

    // =========================================================
    // 💰 PROTECCIÓN FINANCIERA: Tipos corregidos a decimal
    // =========================================================
    
    public decimal ValorRecibo { get; set; }
    public decimal ValorSistema { get; set; }
    
    public decimal ValorCostasPersuasivo { get; set; }
    
    public decimal ValorCostasCoactivo { get; set; }
    public decimal PorcentajeSancion { get; set; }
}