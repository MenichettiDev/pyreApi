using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Alerta
{
    public class UpdateAlertaMovimientoDto
    {
        [Required]
        public int IdAlerta { get; set; }

        [Required]
        public DateTime FechaEstimadaDevolucion { get; set; }

        public string? Comentario { get; set; }

        [Required]
        public int IdModifica { get; set; }
    }
}
