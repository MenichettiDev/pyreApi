namespace pyreApi.DTOs.FamiliaHerramientas
{
    public class FamiliaDto
    {
        public int IdFamilia { get; set; }
        public string NombreFamilia { get; set; } = string.Empty;
        public int TotalHerramientas { get; set; }
        public int HerramientasActivas { get; set; }
        public int HerramientasDisponibles { get; set; }
    }
}
