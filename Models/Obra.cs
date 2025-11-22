using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class Obra
    {
        [Key]
        public int IdObra { get; set; }
        [Required]
        public int IdCliente { get; set; }

        [Required]
        [MaxLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string NombreObra { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }
        [MaxLength(200)]
        public string? Ubicacion { get; set; }

        public DateOnly? FechaInicio { get; set; }

        public DateOnly? FechaFin { get; set; }

        public bool Activo { get; set; } = true;
        public bool Eliminado { get; set; } = false;

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;

    }
}
