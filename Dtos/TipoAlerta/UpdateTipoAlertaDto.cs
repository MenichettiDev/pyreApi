using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.TipoAlerta
{
    public class UpdateTipoAlertaDto
    {
        [Required]
        public int IdTipoAlerta { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de alerta es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreTipoAlerta { get; set; } = string.Empty;
    }
}
