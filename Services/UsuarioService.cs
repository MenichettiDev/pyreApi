using pyreApi.DTOs.Common;
using pyreApi.DTOs.Usuario;
using pyreApi.Models;
using pyreApi.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.Linq; // agregado
using System; // agregado

namespace pyreApi.Services
{
    public class UsuarioService : GenericService<Usuario>
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly ILogger<UsuarioService> _logger;
        private readonly IConfiguration _configuration;

        public UsuarioService(UsuarioRepository usuarioRepository, ILogger<UsuarioService> logger, IConfiguration configuration) : base(usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
            _configuration = configuration;
        }

        // Método privado para hashear contraseñas
        private string HashPassword(string password)
        {
            // Salt fijo (a modo de aprendizaje)
            string salt = _configuration["Salt"]; // Lee el salt del archivo de configuración
            if (string.IsNullOrEmpty(salt))
            {
                throw new InvalidOperationException("El valor de 'Salt' no está configurado en appsettings.json.");
            }

            // Hashear la contraseña usando el salt fijo
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashedPassword;
        }

        // Método para mapear Usuario a UsuarioResponseDto
        private UsuarioResponseDto MapToResponseDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido,
                Legajo = usuario.Legajo,
                Dni = usuario.Dni,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                AccedeAlSistema = usuario.AccedeAlSistema,
                Activo = usuario.Activo,
                Avatar = usuario.Avatar,
                FechaRegistro = usuario.FechaRegistro,
                FechaModificacion = usuario.FechaModificacion,
                RolNombre = usuario.Rol?.NombreRol ?? string.Empty
            };
        }

        public async Task<BaseResponseDto<IEnumerable<UsuarioResponseDto>>> GetAllUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetAllWithRolAsync();
                var usuariosDto = usuarios.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<UsuarioResponseDto>>
                {
                    Success = true,
                    Data = usuariosDto,
                    Message = "Usuarios obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return new BaseResponseDto<IEnumerable<UsuarioResponseDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los usuarios. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAsync(id);
                if (usuario == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {id}."
                    };
                }

                var usuarioDto = MapToResponseDto(usuario);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = usuarioDto,
                    Message = "Usuario encontrado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = $"Error al buscar el usuario con ID {id}. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> CreateUsuarioAsync(CreateUsuarioDto createDto)
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(createDto.Legajo) && createDto.Legajo.Length > 5)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido."
                    };
                }

                // Validar si el DNI ya existe
                var existingUser = await _usuarioRepository.GetByDniAsync(createDto.Dni);
                if (existingUser != null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"Ya existe un usuario registrado con el DNI {createDto.Dni}. Por favor, verifique los datos ingresados."
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
                            Message = $"Ya existe un usuario registrado con el email {createDto.Email}. Por favor, use un email diferente."
                        };
                    }
                }

                // Validar si el legajo ya existe
                if (!string.IsNullOrEmpty(createDto.Legajo))
                {
                    var existingLegajo = await _usuarioRepository.GetByLegajoAsync(createDto.Legajo);
                    if (existingLegajo != null)
                    {
                        return new BaseResponseDto<Usuario>
                        {
                            Success = false,
                            Message = $"Ya existe un usuario registrado con el legajo {createDto.Legajo}. Por favor, use un legajo diferente."
                        };
                    }
                }

                var usuario = new Usuario
                {
                    Nombre = createDto.Nombre,
                    Apellido = createDto.Apellido,
                    Legajo = createDto.Legajo,
                    Dni = createDto.Dni,
                    Email = createDto.Email,
                    Telefono = createDto.Telefono,
                    RolId = createDto.RolId,
                    AccedeAlSistema = true,
                    Avatar = "default.png",
                    IdUsuarioCrea = createDto.IdUsuarioCrea,
                    FechaRegistro = DateTime.UtcNow,
                    FechaModificacion = DateTime.UtcNow,
                    Activo = true
                };

                // Si el usuario accede al sistema, hashear la contraseña proporcionada
                if (usuario.AccedeAlSistema && !string.IsNullOrEmpty(createDto.Password))
                {
                    usuario.PasswordHash = HashPassword(createDto.Password);
                }

                var result = await _usuarioRepository.AddAsync(usuario);
                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = result,
                    Message = $"El usuario {createDto.Nombre} {createDto.Apellido} ha sido creado exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Message}", ex.Message);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "No se pudo crear el usuario. Por favor, verifique los datos ingresados e intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> UpdateUsuarioAsync(UpdateUsuarioDto updateDto)
        {
            try
            {

                var existingUser = await _usuarioRepository.GetByIdAsync(updateDto.Id);
                if (existingUser == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {updateDto.Id} para actualizar."
                    };
                }

                // --- Nuevas validaciones ---

                // Id del usuario que realiza la modificación (se espera que venga en el DTO)
                var modifierId = updateDto.IdUsuarioModifica;

                // 1) Un usuario no puede cambiar su propio rol
                if (modifierId == existingUser.Id && updateDto.RolId.HasValue && updateDto.RolId.Value != existingUser.RolId)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "No está permitido que un usuario cambie su propio rol."
                    };
                }

                // 2) Un SuperAdmin no puede darse de baja a sí mismo mediante la edición (quitar AccedeAlSistema)
                if (modifierId == existingUser.Id && updateDto.AccedeAlSistema.HasValue && updateDto.AccedeAlSistema.Value == true)
                {
                    var rolNombre = existingUser.Rol?.NombreRol ?? string.Empty;
                    if (string.Equals(rolNombre, "superadmin", StringComparison.OrdinalIgnoreCase))
                    {
                        return new BaseResponseDto<Usuario>
                        {
                            Success = false,
                            Message = "Un SuperAdmin no puede darse de baja a sí mismo."
                        };
                    }
                }

                // --- Fin de nuevas validaciones ---

                // Validar email único si se proporciona uno nuevo
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingUser.Email)
                {
                    var existingEmail = await _usuarioRepository.GetByEmailAsync(updateDto.Email);
                    if (existingEmail != null)
                    {
                        return new BaseResponseDto<Usuario>
                        {
                            Success = false,
                            Message = $"Ya existe otro usuario registrado con el email {updateDto.Email}. Por favor, use un email diferente."
                        };
                    }
                }

                // Actualizar campos
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    existingUser.Nombre = updateDto.Nombre;

                if (updateDto.Apellido != null)
                    existingUser.Apellido = updateDto.Apellido;

                if (updateDto.Email != null)
                    existingUser.Email = updateDto.Email;

                if (updateDto.Telefono != null)
                    existingUser.Telefono = updateDto.Telefono;

                if (updateDto.RolId.HasValue)
                    existingUser.RolId = updateDto.RolId.Value;

                if (updateDto.AccedeAlSistema.HasValue)
                    existingUser.AccedeAlSistema = updateDto.AccedeAlSistema.Value;

                if (updateDto.Avatar != null)
                    existingUser.Avatar = updateDto.Avatar;

                existingUser.IdUsuarioModifica = updateDto.IdUsuarioModifica;
                existingUser.FechaModificacion = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(existingUser);
                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = existingUser,
                    Message = $"Los datos del usuario {existingUser.Nombre} {existingUser.Apellido} han sido actualizados correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", updateDto.Id);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "No se pudieron actualizar los datos del usuario. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
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
                        Message = $"No se encontró un usuario con el DNI {dni}."
                    };
                }

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = "Usuario encontrado correctamente."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = $"Error al buscar el usuario con DNI {dni}. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> GetByLegajoAsync(string legajo)
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido."
                    };
                }

                var usuario = await _usuarioRepository.GetByLegajoAsync(legajo);
                if (usuario == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el legajo {legajo}."
                    };
                }

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = "Usuario encontrado correctamente."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = $"Error al buscar el usuario con legajo {legajo}. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." + ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<bool>> ValidateCredentialsAsync(string legajo, string password)
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
                {
                    return new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido."
                    };
                }

                var isValid = await _usuarioRepository.ValidateCredentialsAsync(legajo, password);
                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid ? "Las credenciales son válidas." : "El legajo o la contraseña son incorrectos."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "No se pudieron validar las credenciales. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." + ex.Message }
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
                    Message = "No se pudieron cargar los usuarios activos. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." + ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> AuthenticateAsync(string legajo, string password)
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido."
                    };
                }

                _logger.LogInformation("Iniciando autenticación para legajo: {Legajo}", legajo);

                var usuario = await _usuarioRepository.GetByLegajoWithRolAsync(legajo);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Las credenciales ingresadas son incorrectas. Por favor, verifique su legajo y contraseña."
                    };
                }

                _logger.LogInformation("Usuario encontrado: {UsuarioId}, Nombre: {Nombre}, Activo: {Activo}, AccedeAlSistema: {AccedeAlSistema}",
                    usuario.Id, usuario.Nombre, usuario.Activo, usuario.AccedeAlSistema);

                // Check if user is active and has system access
                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo para legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. Por favor, contacte al administrador del sistema para más información."
                    };
                }

                if (!usuario.AccedeAlSistema)
                {
                    _logger.LogWarning("Usuario sin acceso al sistema para legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Su cuenta no tiene permisos para acceder al sistema. Por favor, contacte al administrador."
                    };
                }

                _logger.LogInformation("Validando credenciales para legajo: {Legajo}", legajo);

                var isValidPassword = await _usuarioRepository.ValidateCredentialsAsync(legajo, password);

                _logger.LogInformation("Resultado validación de credenciales para legajo {Legajo}: {IsValid}", legajo, isValidPassword);

                if (!isValidPassword)
                {
                    _logger.LogWarning("Contraseña incorrecta para legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Las credenciales ingresadas son incorrectas. Por favor, verifique su legajo y contraseña."
                    };
                }

                _logger.LogInformation("Autenticación exitosa para legajo: {Legajo}", legajo);

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = $"¡Bienvenido/a {usuario.Nombre}! Ha iniciado sesión correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación para legajo: {Legajo}", legajo);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "No se pudo completar el proceso de autenticación. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>> GetAllUsuariosPaginatedAsync(
            int page,
            int pageSize,
            string? legajo = null,
            bool? estado = null,
            string? nombre = null,
            string? apellido = null,
            int? rolId = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                // Obtener todos con rol y aplicar filtros en memoria (si vienen)
                var usuarios = await _usuarioRepository.GetAllWithRolAsync();
                IEnumerable<Usuario> filtered = usuarios;

                if (!string.IsNullOrWhiteSpace(legajo))
                {
                    var legajoTrim = legajo.Trim();
                    filtered = filtered.Where(u => string.Equals(u.Legajo?.Trim(), legajoTrim, StringComparison.OrdinalIgnoreCase));
                }

                if (estado.HasValue)
                {
                    filtered = filtered.Where(u => u.Activo == estado.Value);
                }

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrim = nombre.Trim().ToLowerInvariant();
                    filtered = filtered.Where(u => (u.Nombre ?? string.Empty).ToLowerInvariant().Contains(nombreTrim));
                }

                if (!string.IsNullOrWhiteSpace(apellido))
                {
                    var apellidoTrim = apellido.Trim().ToLowerInvariant();
                    filtered = filtered.Where(u => (u.Apellido ?? string.Empty).ToLowerInvariant().Contains(apellidoTrim));
                }

                if (rolId.HasValue)
                {
                    filtered = filtered.Where(u => u.RolId == rolId.Value);
                }

                var totalRecords = filtered.Count();

                var usuariosPage = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var usuariosDto = usuariosPage.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<UsuarioResponseDto>
                {
                    Data = usuariosDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Usuarios obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios paginados");
                return new BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los usuarios. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> ToggleActivoAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0."
                    };
                }

                var existingUser = await _usuarioRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {id}."
                    };
                }

                existingUser.Activo = !existingUser.Activo;
                existingUser.FechaModificacion = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(existingUser);

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = existingUser,
                    Message = existingUser.Activo ? "Usuario activado correctamente." : "Usuario desactivado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado activo del usuario: {Id}", id);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "No se pudo cambiar el estado del usuario. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud." }
                };
            }
        }
    }
}