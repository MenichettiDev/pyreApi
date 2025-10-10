namespace pyreApi.DTOs.Obra
{
    public class ObraDto
    {
        public int IdObra { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NombreObra { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
    }
}
