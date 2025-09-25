using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class ReparacionHerramienta
    {
        [Key]
        public int IdReparacion { get; set; }

        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        public int? IdProveedor { get; set; }

        [Required]
        public DateTime FechaSalida { get; set; }

        public DateTime? FechaAcordadaReparacion { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("IdHerramienta")]
        public Herramienta Herramienta { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey("IdProveedor")]
        public Proveedor? Proveedor { get; set; }

        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }
}

