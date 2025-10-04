using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Herramienta
{
    public class UpdateStatusDto
    {
        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public bool Activo { get; set; }
    }
}
