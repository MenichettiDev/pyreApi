using System.ComponentModel.DataAnnotations;

namespace pyreApi.Models
{
    public class FamiliaHerramientas
    {
        [Key]
        public int IdFamilia { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreFamilia { get; set; } = string.Empty;

        public ICollection<Herramienta> Herramientas { get; set; } = new List<Herramienta>();
    }
}
// 1	Eléctrica
// 4	Ferretería
// 6	Hidráulica
// 2	Mecánica
// 3	Medición
// 7	Neumática
// 5	Seguridad