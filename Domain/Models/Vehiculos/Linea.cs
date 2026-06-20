using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Vehiculos
{
    public class Linea
    {
        [Required]
        [Key]
        public int Id { get; set;}

        public string Nombre { get; set; } = null!;
        
        public int IdMarca { get; set; }
        
        public Marca Marca { get; set; } = null!;
    }
}
