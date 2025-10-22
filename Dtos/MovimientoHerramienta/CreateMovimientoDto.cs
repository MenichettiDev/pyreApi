using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class CreateMovimientoDto
    {
        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdUsuarioGenera { get; set; }
        public int? IdUsuarioResponsable { get; set; }

        [Required]
        public int IdTipoMovimiento { get; set; }

        public int? IdObra { get; set; }
        public int? IdProveedor { get; set; }

        public DateTime? FechaEstimadaDevolucion { get; set; }

        public int? EstadoHerramientaAlDevolver { get; set; }

        public string? Observaciones { get; set; }
    }
}
