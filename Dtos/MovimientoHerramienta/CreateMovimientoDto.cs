using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class CreateMovimientoDto
    {
        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdTipoMovimiento { get; set; }

        [MaxLength(150)]
        public string? DestinoObra { get; set; }

        public int? IdObra { get; set; }

        public DateTime? FechaEstimadaDevolucion { get; set; }

        public int? EstadoHerramientaAlDevolver { get; set; }

        public string? Observaciones { get; set; }
    }
}
