using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreRol { get; set; } = string.Empty; // no null permitido

        // Navegación inversa (opcional)
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
