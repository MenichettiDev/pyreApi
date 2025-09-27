namespace pyreApi.DTOs.Herramienta
{
    public class HerramientaResponseDto
    {
        public int IdHerramienta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NombreHerramienta { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public string? Marca { get; set; }
        public string? Serie { get; set; }
        public DateTime FechaDeIngreso { get; set; }
        public decimal? CostoDolares { get; set; }
        public string? UbicacionFisica { get; set; }
        public string? Ubicacion { get; set; }
        public bool Activo { get; set; }
        public int IdFamilia { get; set; }
        public int IdEstadoFisico { get; set; }
        public int IdDisponibilidad { get; set; }
        public int IdPlanta { get; set; }
        public string FamiliaNombre { get; set; } = string.Empty;
        public string EstadoFisicoNombre { get; set; } = string.Empty;
        public string EstadoDisponibilidadNombre { get; set; } = string.Empty;
        public string PlantaNombre { get; set; } = string.Empty;
    }
}
