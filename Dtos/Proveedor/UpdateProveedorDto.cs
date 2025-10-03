using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Proveedor
{
    public class UpdateProveedorDto
    {
        [Required]
        public int IdProveedor { get; set; }

        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string NombreProveedor { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "El contacto no puede exceder 150 caracteres")]
        public string Contacto { get; set; } = string.Empty;

        [StringLength(11, ErrorMessage = "El CUIT no puede exceder 11 caracteres")]
        public string? Cuit { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }

        public bool Activo { get; set; }
    }
}
