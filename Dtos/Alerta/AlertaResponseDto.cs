namespace pyreApi.DTOs.Alerta
{
    public class AlertaResponseDto
    {
        public int IdAlerta { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Titulo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public bool Activa { get; set; }
        public string TipoAlerta { get; set; } = string.Empty;
        public string? HerramientaCodigo { get; set; }
        public string? UsuarioNombre { get; set; }
    }
}
