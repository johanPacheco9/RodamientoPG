namespace Domain.Models.BaseGravable
{
    public class BaseGravableVehiculo
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty; // El código de línea de 8 dígitos del Ministerio
        public string ClaseVehiculo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Linea { get; set; } = string.Empty;
        public int Cilindraje { get; set; }
        public int Capacidad { get; set; }
        public int Pasajeros { get; set; }

        // Relación: Un vehículo de la tabla tiene muchos valores comerciales (uno por cada año)
        public virtual ICollection<BaseGravableVigencia> Vigencias { get; set; } = new List<BaseGravableVigencia>();
    }
}