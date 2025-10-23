using System;

namespace pyreApi.DTOs.MovimientoHerramienta
{
    public class MovimientoHerramientaDetailDto
    {
        public int Id { get; set; }
        public HerramientaMinDto Herramienta { get; set; } = new HerramientaMinDto();
        public UsuarioContainerDto Usuario { get; set; } = new UsuarioContainerDto();
        public TipoMovimientoDto TipoMovimiento { get; set; } = new TipoMovimientoDto();
        public ObraDto? Obra { get; set; }
        public ProveedorDto? Proveedor { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public DateTime? FechaEstimadaDevolucion { get; set; }
        public int? EstadoHerramientaAlDevolver { get; set; }
        public string? EstadoDevolucion { get; set; }
        public string? Observaciones { get; set; }
    }

    public class HerramientaMinDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
    }

    public class UsuarioContainerDto
    {
        public UsuarioFullDto? Genera { get; set; }
        public UsuarioFullDto? Responsable { get; set; }
    }

    public class UsuarioFullDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Legajo { get; set; }
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public int RolId { get; set; }
        public bool AccedeAlSistema { get; set; }
        public bool Activo { get; set; }
        public string? Avatar { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class TipoMovimientoDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class ObraDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Ubicacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Activa { get; set; }
    }

    public class ProveedorDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Cuit { get; set; }
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
