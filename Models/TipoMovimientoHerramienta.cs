using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class TipoMovimientoHerramienta
    {
        [Key]
        public int IdTipoMovimiento { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreTipoMovimiento { get; set; } = string.Empty;

        public ICollection<MovimientoHerramienta> Movimientos { get; set; } = new List<MovimientoHerramienta>();
    }
}
