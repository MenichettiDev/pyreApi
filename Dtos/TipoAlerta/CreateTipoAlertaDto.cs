using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.TipoAlerta
{
    public class CreateTipoAlertaDto
    {
        [Required(ErrorMessage = "El nombre del tipo de alerta es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreTipoAlerta { get; set; } = string.Empty;
    }
}
