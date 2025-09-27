using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Herramienta
{
    public class HerramientaStatusDto
    {
        [Required]
        public int IdHerramienta { get; set; }

        [Required]
        public bool Activo { get; set; }
    }
}
