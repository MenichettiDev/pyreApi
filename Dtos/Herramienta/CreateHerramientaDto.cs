using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Herramienta
{
    public class CreateHerramientaDto
    {
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

        public decimal? CostoDolares { get; set; }

        [MaxLength(150)]
        public string? UbicacionFisica { get; set; }

        [Required]
        public int IdEstadoFisico { get; set; }
        [Required]
        public int IdDisponibilidad { get; set; }

        [Required]
        public int IdPlanta { get; set; }

        [MaxLength(50)]
        public string? Ubicacion { get; set; }
    }
}
