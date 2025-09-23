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
