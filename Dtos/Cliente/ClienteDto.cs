namespace pyreApi.DTOs.Cliente
{
    public class ClienteDto
    {
        public int IdCliente { get; set; }
        public string? Cuit { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
