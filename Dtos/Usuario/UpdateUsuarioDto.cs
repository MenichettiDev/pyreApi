using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Usuario
{
    public class UpdateUsuarioDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Nombre { get; set; }

        [MaxLength(100)]
        public string? Apellido { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        public int? RolId { get; set; }

        public bool? AccedeAlSistema { get; set; }

        [MaxLength(45)]
        public string? Avatar { get; set; }

        public int IdUsuarioModifica { get; set; }
    }
}
