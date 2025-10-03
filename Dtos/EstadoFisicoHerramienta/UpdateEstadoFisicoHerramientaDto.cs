using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.EstadoFisicoHerramienta
{
    public class UpdateEstadoFisicoHerramientaDto
    {
        [Required]
        public int IdEstadoFisico { get; set; }

        [Required(ErrorMessage = "La descripción del estado es requerida")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        public string DescripcionEstado { get; set; } = string.Empty;

    }
}
