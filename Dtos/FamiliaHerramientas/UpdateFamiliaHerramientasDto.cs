using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.FamiliaHerramientas
{
    public class UpdateFamiliaHerramientasDto
    {
        [Required]
        public int IdFamilia { get; set; }

        [Required(ErrorMessage = "El nombre de la familia es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreFamilia { get; set; } = string.Empty;
    }
}
