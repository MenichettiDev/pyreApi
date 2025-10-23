namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class MovimientoHerramientaDto
    {
        public int IdMovimiento { get; set; }
        public int IdHerramienta { get; set; }
        public string? CodigoHerramienta { get; set; }
        public string? NombreHerramienta { get; set; }
        public int IdUsuarioGenera { get; set; }
        public string? NombreUsuarioGenera { get; set; }
        public int? IdUsuarioResponsable { get; set; }
        public string? NombreUsuarioResponsable { get; set; }
        public int IdTipoMovimiento { get; set; }
        public string? TipoMovimiento { get; set; }
        public int? IdObra { get; set; }
        public string? NombreObra { get; set; }
        public int? IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaEstimadaDevolucion { get; set; }
        public int? EstadoHerramientaAlDevolver { get; set; }
        public string? EstadoDevolucion { get; set; }
        public string? Observaciones { get; set; }
    }
}
