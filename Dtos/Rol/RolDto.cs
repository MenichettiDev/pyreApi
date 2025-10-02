namespace pyreApi.DTOs.Rol
{
    public class RolDto
    {
        public int Id { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
    }
}
