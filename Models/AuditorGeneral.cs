using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public enum AccionAuditoria
    {
        INSERT,
        UPDATE,
        DELETE,
        ALERTA_GENERADA,
        FECHA_CAMBIADA
    }

    public class AuditorGeneral
    {
        [Key]
        public long IdAuditoria { get; set; }

        [Required]
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(100)]
        public string Entidad { get; set; } = string.Empty;

        [Required]
        public int IdEntidad { get; set; }

        [Required]
        public AccionAuditoria Accion { get; set; }

        [Column(TypeName = "json")]
        public string? ValorAnterior { get; set; }

        [Column(TypeName = "json")]
        public string? ValorNuevo { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("Id")]
        public Usuario Usuario { get; set; } = null!;
    }
}
