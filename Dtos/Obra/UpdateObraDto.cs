using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Obra
{
    public class UpdateObraDto
    {
        [Required]
        public int IdObra { get; set; }

        [Required(ErrorMessage = "El código es requerido")]
        [MaxLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la obra es requerido")]
        [MaxLength(150, ErrorMessage = "El nombre de la obra no puede exceder 150 caracteres")]
        public string NombreObra { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Ubicacion { get; set; }

        public DateOnly? FechaInicio { get; set; }

        public DateOnly? FechaFin { get; set; }
    }
}
