namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class MovimientoResponseDto
    {
        public int IdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaEstimadaDevolucion { get; set; }
        public string? Observaciones { get; set; }
        public string HerramientaCodigo { get; set; } = string.Empty;
        public string HerramientaNombre { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public string? ObraNombre { get; set; }
        public string? EstadoDevolucion { get; set; }
    }
}
