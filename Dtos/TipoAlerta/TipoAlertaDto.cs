namespace pyreApi.DTOs.TipoAlerta
{
    public class TipoAlertaDto
    {
        public int IdTipoAlerta { get; set; }
        public string NombreTipoAlerta { get; set; } = string.Empty;
        public int TotalAlertas { get; set; }
        public int AlertasActivas { get; set; }
    }
}
