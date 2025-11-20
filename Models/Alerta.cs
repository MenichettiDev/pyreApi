using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class Alerta
    {
        [Key]
        public int IdAlerta { get; set; }

        [Required]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdTipoAlerta { get; set; }

        [Required]
        public DateTime FechaGeneracion { get; set; }

        public string? Comentario { get; set; }
        public int? IdModifica { get; set; }

        public bool Activo { get; set; } = true;

        [ForeignKey(nameof(IdMovimiento))]
        public MovimientoHerramienta MovimientoHerramienta { get; set; } = null!;

        [ForeignKey(nameof(IdTipoAlerta))]
        public TipoAlerta TipoAlerta { get; set; } = null!;
        [ForeignKey(nameof(IdModifica))]
        public Usuario Usuario { get; set; } = null!;
    }
}