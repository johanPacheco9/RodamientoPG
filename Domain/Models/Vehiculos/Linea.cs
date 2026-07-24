using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Vehiculos
{
    public class Linea
    {
        [Required]
        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        [ForeignKey(nameof(Marca))]
        public int IdMarca { get; set; }

        public Marca Marca { get; set; } = null!;
    }
}