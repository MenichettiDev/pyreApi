using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Herramienta
{
    public class CreateHerramientaDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la herramienta es requerido")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string NombreHerramienta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La familia es requerida")]
        public int IdFamilia { get; set; }

        [StringLength(100, ErrorMessage = "El tipo no puede exceder 100 caracteres")]
        public string? Tipo { get; set; }

        [StringLength(100, ErrorMessage = "La marca no puede exceder 100 caracteres")]
        public string? Marca { get; set; }

        [StringLength(100, ErrorMessage = "La serie no puede exceder 100 caracteres")]
        public string? Serie { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es requerida")]
        public DateTime FechaDeIngreso { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0")]
        public decimal? CostoDolares { get; set; }

        [StringLength(150, ErrorMessage = "La ubicación física no puede exceder 150 caracteres")]
        public string? UbicacionFisica { get; set; }

        [Required(ErrorMessage = "El estado físico es requerido")]
        public int IdEstadoFisico { get; set; }

        [Required(ErrorMessage = "La planta es requerida")]
        public int IdPlanta { get; set; } = 1;

        [StringLength(50, ErrorMessage = "La ubicación no puede exceder 50 caracteres")]
        public string? Ubicacion { get; set; }

        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "El estado de disponibilidad es requerido")]
        public int IdDisponibilidad { get; set; }
    }
}
