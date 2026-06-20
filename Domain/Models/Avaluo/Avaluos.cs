namespace Domain.Models.Avaluo;

public class AvaluoVehiculo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string ClaseVehiculo { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Linea { get; set; } = string.Empty;
    public int Cilindraje { get; set; }
    public int Capacidad { get; set; }
    public int Pasajeros { get; set; }
    // Propiedad de navegación para los valores anuales
    public virtual ICollection<AvaluoVigencia> Vigencias { get; set; } = new List<AvaluoVigencia>();
}