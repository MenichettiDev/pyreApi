using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class EstadoFisicoHerramienta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Herramienta> Herramientas { get; set; } = new List<Herramienta>();
        public ICollection<MovimientoHerramienta> MovimientosHerramienta { get; set; } = new List<MovimientoHerramienta>();
    }
}
