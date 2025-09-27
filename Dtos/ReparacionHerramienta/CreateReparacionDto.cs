using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.ReparacionHerramienta
{
    public class CreateReparacionDto
    {
        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        [Required]
        public int IdUsuarioResponsable { get; set; }

        [MaxLength(500)]
        public string? DescripcionProblema { get; set; }

        public decimal? CostoReparacion { get; set; }

        public DateTime? FechaEstimadaFinalizacion { get; set; }

        public string? Observaciones { get; set; }
    }
}
