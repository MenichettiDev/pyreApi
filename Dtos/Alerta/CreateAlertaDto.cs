using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Alerta
{
    public class CreateAlertaDto
    {
        [Required]
        public int IdTipoAlerta { get; set; }

        public int? IdHerramienta { get; set; }

        public int? IdUsuario { get; set; }

        public int? IdMovimiento { get; set; }

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Titulo { get; set; }
    }
}
