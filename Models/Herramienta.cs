using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class Herramienta
    {
        [Key]
        public int IdHerramienta { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string NombreHerramienta { get; set; } = string.Empty;

        [Required]
        public int IdFamilia { get; set; }

        [MaxLength(100)]
        public string? Tipo { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Serie { get; set; }

        [Required]
        public DateTime FechaDeIngreso { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? CostoDolares { get; set; }

        [MaxLength(150)]
        public string? UbicacionFisica { get; set; }

        [Required]
        public int IdEstadoFisico { get; set; }

        [Required]
        public int IdPlanta { get; set; } = 1;

        [MaxLength(50)]
        public string? Ubicacion { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        public int IdDisponibilidad { get; set; }

        [ForeignKey(nameof(IdFamilia))]
        public FamiliaHerramientas Familia { get; set; } = null!;

        [ForeignKey(nameof(IdEstadoFisico))]
        public EstadoFisicoHerramienta EstadoFisico { get; set; } = null!;
        [ForeignKey(nameof(IdDisponibilidad))]
        public EstadoDisponibilidadHerramienta EstadoDisponibilidad { get; set; } = null!;

        [ForeignKey(nameof(IdPlanta))]
        public Planta Planta { get; set; } = null!;

        public ICollection<MovimientoHerramienta> Movimientos { get; set; } = new List<MovimientoHerramienta>();
        public ICollection<ReparacionHerramienta> Reparaciones { get; set; } = new List<ReparacionHerramienta>();
        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }
}