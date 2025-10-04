namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class MovimientoHerramientaDto
    {
        public int IdMovimiento { get; set; }
        public int IdHerramienta { get; set; }
        public int IdUsuario { get; set; }
        public int IdTipoMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public int? IdObra { get; set; }
        public DateTime? FechaEstimadaDevolucion { get; set; }
        public int? EstadoHerramientaAlDevolver { get; set; }
        public string? Observaciones { get; set; }

        // Navigation properties
        public string? CodigoHerramienta { get; set; }
        public string? NombreHerramienta { get; set; }
        public string? NombreUsuario { get; set; }
        public string? TipoMovimiento { get; set; }
        public string? NombreObra { get; set; }
        public string? EstadoDevolucion { get; set; }
    }
}
