using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class EstadoHerramienta
    {
        [Key]
        public int IdEstadoHerramienta { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreEstadoHerramienta { get; set; } = string.Empty;

        public ICollection<Herramienta> Herramientas { get; set; } = new List<Herramienta>();
        public ICollection<MovimientoHerramienta> MovimientosHerramienta { get; set; } = new List<MovimientoHerramienta>();
    }
}
