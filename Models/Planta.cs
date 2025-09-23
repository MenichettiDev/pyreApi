using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class Planta
    {
        [Key]
        public int IdPlanta { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombrePlanta { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Ubicacion { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<Herramienta> Herramientas { get; set; } = new List<Herramienta>();
    }
}
