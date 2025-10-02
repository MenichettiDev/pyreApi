using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.EstadoDisponibilidadHerramienta
{
    public class CreateEstadoDisponibilidadDto
    {
        [Required(ErrorMessage = "La descripción del estado de disponibilidad es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
