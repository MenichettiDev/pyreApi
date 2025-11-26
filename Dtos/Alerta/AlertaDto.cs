namespace pyreApi.DTOs.Alerta
{
    public class AlertaDto
    {
        public int IdAlerta { get; set; }
        public int IdMovimiento { get; set; }
        public string NombreHerramienta { get; set; } = string.Empty;
        public int IdTipoAlerta { get; set; }
        public string NombreTipoAlerta { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? Comentario { get; set; }
        public int IdModifica { get; set; }
        public bool Activo { get; set; }
        public string? HerramientaNombre { get; set; }
        public string? HerramientaCodigo { get; set; }
        public string? ResponsableNombre { get; set; }
        public string? TipoMovimiento { get; set; }
        public string? UsuarioModificaNombre { get; set; }

    }
}
