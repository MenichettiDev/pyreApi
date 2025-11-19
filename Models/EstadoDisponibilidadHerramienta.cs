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
// 1	Disponible
// 2	Prestada
// 3	Mantenimiento
// 4	Extraviada
// 5	Bloqueada