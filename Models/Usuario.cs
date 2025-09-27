using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [MaxLength(100)]
        public string? Apellido { get; set; }
        [Required]
        public String? Legajo { get; set; }

        [MaxLength(20)]
        public string? Dni { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [Required]
        public int RolId { get; set; }

        public bool AccedeAlSistema { get; set; } = false;

        public bool Activo { get; set; } = true;

        [MaxLength(45)]
        public string? Avatar { get; set; }


        public DateTime FechaModificacion { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public int? IdUsuarioCrea { get; set; }

        public int? IdUsuarioModifica { get; set; }
        [NotMapped]

        public string? PasswordHash { get; set; } // Contraseña encriptada

        [ForeignKey("RolId")]
        public Rol Rol { get; set; } = null!;
    }
}
