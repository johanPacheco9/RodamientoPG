using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.ProcesoLiquidacion;
namespace Domain.Models.Notificaciones;

public class Aviso
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProcesoId { get; set; } // El expediente al que corresponde el aviso

    [Required]
    public int NumeroAviso { get; set; } // 1, 2, 3 o 4

    [Required]
    public DateTime FechaEnvio { get; set; }

    [StringLength(50)]
    public string? NumeroGuia { get; set; } 

    [StringLength(250)]
    public string? RutaPdf { get; set; } // Ruta del archivo generado en el servidor

    public string Estado { get; set; } = "Enviado"; // Enviado, Entregado, Devuelto

    [ForeignKey(nameof(ProcesoId))]
    public virtual Proceso Proceso { get; set; } = null!;
}