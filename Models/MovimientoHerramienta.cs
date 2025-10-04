using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class MovimientoHerramienta
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdTipoMovimiento { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int? IdObra { get; set; }

        public DateTime? FechaEstimadaDevolucion { get; set; }

        public int? EstadoHerramientaAlDevolver { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("IdHerramienta")]
        public Herramienta Herramienta { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey("IdTipoMovimiento")]
        public TipoMovimientoHerramienta TipoMovimiento { get; set; } = null!;

        [ForeignKey("IdObra")]
        public Obra? Obra { get; set; }

        [ForeignKey("EstadoHerramientaAlDevolver")]
        public EstadoFisicoHerramienta? EstadoDevolucion { get; set; }
    }
}
