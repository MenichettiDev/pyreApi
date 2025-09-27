using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Obra
{
    public class CreateObraDto
    {
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [MaxLength(200)]
        public string? Ubicacion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFinEstimada { get; set; }

        [MaxLength(100)]
        public string? ResponsableObra { get; set; }
    }
}
