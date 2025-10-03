using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.TipoMovimientoHerramienta
{
    public class CreateTipoMovimientoHerramientaDto
    {
        [Required(ErrorMessage = "El nombre del tipo de movimiento es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreTipoMovimiento { get; set; } = string.Empty;
    }
}
