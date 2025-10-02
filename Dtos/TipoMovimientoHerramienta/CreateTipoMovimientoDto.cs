using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.TipoMovimientoHerramienta
{
    public class CreateTipoMovimientoDto
    {
        [Required(ErrorMessage = "El nombre del tipo de movimiento es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreTipoMovimiento { get; set; } = string.Empty;
    }
}
