namespace pyreApi.DTOs.Planta
{
    public class PlantaDto
    {
        public int IdPlanta { get; set; }
        public string NombrePlanta { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public string? Direccion { get; set; }
        public bool Activa { get; set; }
        public int TotalHerramientas { get; set; }
        public int HerramientasActivas { get; set; }
    }
}
