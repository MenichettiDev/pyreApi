using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class EstadoDisponibilidadHerramienta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Herramienta> Herramientas { get; set; } = new List<Herramienta>();
    }
}
