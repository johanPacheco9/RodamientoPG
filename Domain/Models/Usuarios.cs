using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Responses.Users.Enums;

namespace Domain.Models;

public class Usuario : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = null!;

    public bool IsHabilitado { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Auth0Id { get; set; } = null!;
    
    public Role Role { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Correo { get; set; }

    public string Password { get; set; } = null!;
}