namespace pyreApi.DTOs.Herramienta
{
    public class HerramientaDto
    {
        public int IdHerramienta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NombreHerramienta { get; set; } = string.Empty;
        public int IdFamilia { get; set; }
        public string? Tipo { get; set; }
        public string? Marca { get; set; }
        public string? Serie { get; set; }
        public DateTime FechaDeIngreso { get; set; }
        public decimal? CostoDolares { get; set; }
        public string? UbicacionFisica { get; set; }
        public int IdEstadoFisico { get; set; }
        public int IdPlanta { get; set; }
        public string? Ubicacion { get; set; }
        public bool Activo { get; set; }
        public int IdDisponibilidad { get; set; }
        public string? NombreFamilia { get; set; }
        public string? EstadoFisico { get; set; }
        public string? EstadoDisponibilidad { get; set; }
        public string? NombrePlanta { get; set; }
    }
}
