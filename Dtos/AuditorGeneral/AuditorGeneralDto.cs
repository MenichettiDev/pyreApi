using pyreApi.Models;

namespace pyreApi.DTOs.AuditorGeneral
{
    public class AuditorGeneralDto
    {
        public long IdAuditoria { get; set; }
        public DateTime FechaHora { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public int IdEntidad { get; set; }
        public AccionAuditoria Accion { get; set; }
        public string AccionDescripcion { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
        public string? Observaciones { get; set; }
    }
}
