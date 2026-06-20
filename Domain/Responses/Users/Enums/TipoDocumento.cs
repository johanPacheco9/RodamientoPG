using System.ComponentModel.DataAnnotations;
namespace Domain.Responses.Users.Enums;

public enum TipoDocumento
{
    [Display(Name = "Cédula de Ciudadanía")]
    Cc = 1,

    [Display(Name = "Tarjeta de Identidad")]
    Ti = 2,

    [Display(Name = "Cédula de Extranjería")]
    Ce = 3,

    [Display(Name = "NIT")]
    Nit = 4,

    [Display(Name = "Pasaporte")]
    Pasaporte = 6,

    [Display(Name = "Carnet Diplomático")]
    CarnetDiplomatico = 7,

    [Display(Name = "Cédula Venezolana")]
    CedulaVenezolana = 9
}