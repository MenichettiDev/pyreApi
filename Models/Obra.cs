using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class Obra
    {
        [Key]
        public int IdObra { get; set; }

        [Required]
        [MaxLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string NombreObra { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Ubicacion { get; set; }

        public DateOnly? FechaInicio { get; set; }

        public DateOnly? FechaFin { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<MovimientoHerramienta> MovimientosHerramienta { get; set; } = new List<MovimientoHerramienta>();
    }
}
