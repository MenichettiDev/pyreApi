using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class Alerta
    {
        [Key]
        public int IdAlerta { get; set; }

        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdTipoAlerta { get; set; }

        [Required]
        public DateTime FechaGeneracion { get; set; }

        public bool Leida { get; set; } = false;

        [ForeignKey(nameof(IdHerramienta))]
        public Herramienta Herramienta { get; set; } = null!;

        [ForeignKey(nameof(IdTipoAlerta))]
        public TipoAlerta TipoAlerta { get; set; } = null!;
    }
}