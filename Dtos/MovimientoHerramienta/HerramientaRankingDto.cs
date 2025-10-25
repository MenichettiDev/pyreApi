namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class HerramientaRankingDto
    {
        public int IdHerramienta { get; set; }
        public string? CodigoHerramienta { get; set; }
        public string? NombreHerramienta { get; set; }
        public string? FamiliaHerramienta { get; set; }
        public int TotalPrestamos { get; set; }
        public DateTime? UltimoPrestamo { get; set; }
    }
}
