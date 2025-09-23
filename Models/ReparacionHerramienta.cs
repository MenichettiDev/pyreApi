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
        public int IdUsuarioResponsable { get; set; }

        [Required]
        public int IdUsuarioRepara { get; set; }

        [Required]
        public DateTime FechaSalida { get; set; }

        public DateTime? FechaAcordadaReparacion { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("IdHerramienta")]
        public Herramienta Herramienta { get; set; } = null!;

        [ForeignKey("IdUsuarioResponsable")]
        public Usuario UsuarioResponsable { get; set; } = null!;

        [ForeignKey("IdUsuarioRepara")]
        public Usuario UsuarioRepara { get; set; } = null!;

        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }
}
