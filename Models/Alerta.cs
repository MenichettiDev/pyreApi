using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public enum TipoAlerta
    {
        DevolucionExcedida,
        ReparacionExcedida
    }

    public class Alerta
    {
        [Key]
        public int IdAlerta { get; set; }

        [Required]
        public int IdHerramienta { get; set; }

        public int? IdReparacion { get; set; }

        [Required]
        public TipoAlerta TipoAlerta { get; set; }

        [Required]
        public DateTime FechaGeneracion { get; set; }

        public bool Leida { get; set; } = false;

        [ForeignKey("IdHerramienta")]
        public Herramienta Herramienta { get; set; } = null!;

        [ForeignKey("IdReparacion")]
        public ReparacionHerramienta? Reparacion { get; set; }
    }
}
