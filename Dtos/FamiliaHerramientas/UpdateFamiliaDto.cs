using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.FamiliaHerramientas
{
    public class UpdateFamiliaDto
    {
        [Required(ErrorMessage = "El ID de la familia es obligatorio")]
        public int IdFamilia { get; set; }

        [Required(ErrorMessage = "El nombre de la familia es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreFamilia { get; set; } = string.Empty;
    }
}
