using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Proveedor
{
    public class CreateProveedorDto
    {
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres")]
        public string NombreProveedor { get; set; } = string.Empty;

        [MaxLength(11, ErrorMessage = "El CUIT no puede exceder los 11 caracteres")]
        public string? Cuit { get; set; }

        [MaxLength(50, ErrorMessage = "El teléfono no puede exceder los 50 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [MaxLength(150, ErrorMessage = "El email no puede exceder los 150 caracteres")]
        public string? Email { get; set; }

        [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
