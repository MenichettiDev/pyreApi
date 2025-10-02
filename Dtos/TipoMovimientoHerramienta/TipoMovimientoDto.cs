namespace pyreApi.DTOs.TipoMovimientoHerramienta
{
    public class TipoMovimientoDto
    {
        public int IdTipoMovimiento { get; set; }
        public string NombreTipoMovimiento { get; set; } = string.Empty;
        public int TotalMovimientos { get; set; }
    }
}
