using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Cliente
{
    public class UpdateClienteDto
    {
        [Required]
        public int IdCliente { get; set; }

        [StringLength(11, ErrorMessage = "El CUIT no puede exceder 11 caracteres")]
        public string? Cuit { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres")]
        public string? Direccion { get; set; }

        public bool Activo { get; set; }
    }
}
