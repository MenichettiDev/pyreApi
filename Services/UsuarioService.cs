using pyreApi.DTOs.Common;
using pyreApi.DTOs.Usuario;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class UsuarioService : GenericService<Usuario>
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuarioService(UsuarioRepository usuarioRepository) : base(usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<BaseResponseDto<Usuario>> CreateUsuarioAsync(CreateUsuarioDto createDto)
        {
            try
            {
                // Validar si el DNI ya existe
                var existingUser = await _usuarioRepository.GetByDniAsync(createDto.Dni);
                if (existingUser != null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Ya existe un usuario con este DNI"
                    };
                }

                // Validar si el email ya existe
                if (!string.IsNullOrEmpty(createDto.Email))
                {
                    var existingEmail = await _usuarioRepository.GetByEmailAsync(createDto.Email);
                    if (existingEmail != null)
                    {
                        return new BaseResponseDto<Usuario>
                        {
                            Success = false,
                            Message = "Ya existe un usuario con este email"
                        };
                    }
                }

                var usuario = new Usuario
                {
                    Nombre = createDto.Nombre,
                    Apellido = createDto.Apellido,
                    Dni = createDto.Dni,
                    Email = createDto.Email,
                    Telefono = createDto.Telefono,
                    RolId = createDto.RolId,
                    AccedeAlSistema = createDto.AccedeAlSistema,
                    Avatar = createDto.Avatar,
                    IdUsuarioCrea = createDto.IdUsuarioCrea,
                    FechaRegistro = DateTime.UtcNow,
                    FechaModificacion = DateTime.UtcNow,
                    Activo = true
                };

                var result = await _usuarioRepository.AddAsync(usuario);
                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = result,
                    Message = "Usuario creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "Error al crear el usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> GetByDniAsync(string dni)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByDniAsync(dni);
                if (usuario == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = "Usuario encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "Error al buscar el usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<bool>> ValidateCredentialsAsync(string dni, string password)
        {
            try
            {
                // TODO: Hash the password before validation
                var isValid = await _usuarioRepository.ValidateCredentialsAsync(dni, password);
                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid ? "Credenciales válidas" : "Credenciales inválidas"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al validar credenciales",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Usuario>>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _usuarioRepository.GetActiveUsersAsync();
                return new BaseResponseDto<IEnumerable<Usuario>>
                {
                    Success = true,
                    Data = users,
                    Message = "Usuarios activos obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Usuario>>
                {
                    Success = false,
                    Message = "Error al obtener usuarios activos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
