using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.EstadoFisicoHerramienta
{
    public class CreateEstadoFisicoHerramientaDto
    {
        [Required(ErrorMessage = "La descripción del estado es requerida")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        public string DescripcionEstado { get; set; } = string.Empty;

    }
}
