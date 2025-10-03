using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.EstadoDisponibilidad
{
    public class UpdateEstadoDisponibilidadDto
    {
        [Required]
        public int IdEstadoDisponibilidad { get; set; }

        [Required(ErrorMessage = "La descripción del estado es requerida")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        public string DescripcionEstado { get; set; } = string.Empty;

    }
}
