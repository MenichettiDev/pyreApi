using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Alerta
{
    public class UpdateAlertaDto
    {
        [Required]
        public int IdAlerta { get; set; }

        [Required(ErrorMessage = "El ID de herramienta es obligatorio")]
        public int IdHerramienta { get; set; }

        [Required(ErrorMessage = "El ID de tipo de alerta es obligatorio")]

        public bool Leida { get; set; }
    }
}
