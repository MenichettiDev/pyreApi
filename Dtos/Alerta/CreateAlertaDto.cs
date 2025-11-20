using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Alerta
{
    public class CreateAlertaDto
    {
        [Required(ErrorMessage = "El ID de movimiento es obligatorio")]
        public int IdMovimiento { get; set; }

        [Required(ErrorMessage = "El ID de tipo de alerta es obligatorio")]
        public int IdTipoAlerta { get; set; }

        public string? Comentario { get; set; }
    }
}
