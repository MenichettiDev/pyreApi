namespace pyreApi.DTOs.Alerta
{
    public class AlertaDto
    {
        public int IdAlerta { get; set; }
        public int IdHerramienta { get; set; }
        public string NombreHerramienta { get; set; } = string.Empty;
        public int IdTipoAlerta { get; set; }
        public string NombreTipoAlerta { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public bool Leida { get; set; }
    }
}
