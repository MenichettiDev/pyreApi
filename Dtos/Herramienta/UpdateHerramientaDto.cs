using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Herramienta
{
    public class UpdateHerramientaDto
    {
        [Required]
        public int IdHerramienta { get; set; }

        [MaxLength(150)]
        public string? NombreHerramienta { get; set; }

        public int? IdFamilia { get; set; }

        [MaxLength(100)]
        public string? Tipo { get; set; }

        [MaxLength(100)]
        public string? Marca { get; set; }

        [MaxLength(100)]
        public string? Serie { get; set; }

        public decimal? CostoDolares { get; set; }

        [MaxLength(150)]
        public string? UbicacionFisica { get; set; }

        public int? IdEstadoFisico { get; set; }

        public int? IdDisponibilidad { get; set; }

        public int? IdPlanta { get; set; }

        [MaxLength(50)]
        public string? Ubicacion { get; set; }
    }
}
