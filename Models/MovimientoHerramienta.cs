using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public enum TipoMovimiento
    {
        Ingreso,
        Egreso,
        Reparación,
        Baja
    }

    public class MovimientoHerramienta
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdActor { get; set; }

        [Required]
        public TipoMovimiento TipoMovimiento { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public TimeSpan Hora { get; set; }

        [MaxLength(150)]
        public string? DestinoObra { get; set; }

        public int? IdObra { get; set; }

        public DateTime? FechaEstimadaDevolucion { get; set; }

        public int? EstadoHerramientaAlDevolver { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("IdHerramienta")]
        public Herramienta Herramienta { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey("IdObra")]
        public Obra? Obra { get; set; }

        [ForeignKey("EstadoHerramientaAlDevolver")]
        public EstadoHerramienta? EstadoDevolucion { get; set; }
    }
}
