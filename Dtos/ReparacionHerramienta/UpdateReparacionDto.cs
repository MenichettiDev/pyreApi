using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.ReparacionHerramienta
{
    public class UpdateReparacionDto
    {
        [Required]
        public int IdReparacion { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public decimal? CostoReparacion { get; set; }

        public string? ResultadoReparacion { get; set; }

        public string? Observaciones { get; set; }

        public bool Finalizada { get; set; }
    }
}
