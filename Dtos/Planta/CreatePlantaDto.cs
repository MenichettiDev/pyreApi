using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Planta
{
    public class CreatePlantaDto
    {
        [Required(ErrorMessage = "El nombre de la planta es obligatorio")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres")]
        public string NombrePlanta { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres")]
        public string? Ubicacion { get; set; }

        [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        public bool Activa { get; set; } = true;
    }
}
