using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class TipoAlerta
    {
        [Key]
        public int IdTipoAlerta { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreTipoAlerta { get; set; } = string.Empty;

        public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }
}
