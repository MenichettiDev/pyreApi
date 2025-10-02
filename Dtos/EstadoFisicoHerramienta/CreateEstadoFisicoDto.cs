using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.EstadoFisicoHerramienta
{
    public class CreateEstadoFisicoDto
    {
        [Required(ErrorMessage = "La descripción del estado físico es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
