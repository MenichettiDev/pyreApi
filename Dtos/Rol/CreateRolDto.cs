using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Rol
{
    public class CreateRolDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreRol { get; set; } = string.Empty;
    }
}
