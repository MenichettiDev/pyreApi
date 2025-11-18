using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [MaxLength(11)]
        public string? Cuit { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
