using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreProveedor { get; set; } = string.Empty;

        [MaxLength(11)]
        public string? Cuit { get; set; }
        [MaxLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<ReparacionHerramienta> Reparaciones { get; set; } = new List<ReparacionHerramienta>();
    }
}
