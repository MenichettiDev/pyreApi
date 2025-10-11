using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.Usuario;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? legajo = null,
            [FromQuery] bool? estado = null,
            [FromQuery] string? nombre = null,
            [FromQuery] string? apellido = null,
            [FromQuery(Name = "rol")] int? rolId = null)
        {
            // Validar longitud del legajo si se proporcionó
            if (!string.IsNullOrWhiteSpace(legajo) && legajo.Length > 5)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "El legajo no puede tener más de 5 caracteres.",
                    Errors = new List<string> { "Legajo excede la longitud máxima permitida (5 caracteres)." }
                });
            }

            var response = await _usuarioService.GetAllUsuariosPaginatedAsync(page, pageSize, legajo, estado, nombre, apellido, rolId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("all-unpaginated")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUnpaginated()
        {
            var response = await _usuarioService.GetAllUsuariosAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del usuario debe ser un número válido mayor a 0." });
            }

            var response = await _usuarioService.GetUsuarioByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("dni/{dni}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return BadRequest(new { Success = false, Message = "El DNI es requerido y no puede estar vacío." });
            }

            var response = await _usuarioService.GetByDniAsync(dni);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveUsers()
        {
            var response = await _usuarioService.GetActiveUsersAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto createDto)
        {
            // Validar longitud del legajo
            if (!string.IsNullOrEmpty(createDto.Legajo) && createDto.Legajo.Length > 5)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "El legajo no puede tener más de 5 caracteres.",
                    Errors = new List<string> { "Legajo excede la longitud máxima permitida (5 caracteres)." }
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    Success = false,
                    Message = "Los datos proporcionados no son válidos. Por favor, revise la información ingresada.",
                    Errors = errors
                });
            }

            var response = await _usuarioService.CreateUsuarioAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del usuario debe ser un número válido mayor a 0." });
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int idUsuarioModifica = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            updateDto.IdUsuarioModifica = idUsuarioModifica;

            // Evitar que el usuario cambie su propio rol
            if (idUsuarioModifica == id && updateDto.RolId.HasValue)
            {
                return BadRequest(new { Success = false, Message = "No está permitido que un usuario cambie su propio rol." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    Success = false,
                    Message = "Los datos proporcionados para la actualización no son válidos. Por favor, revise la información ingresada.",
                    Errors = errors
                });
            }

            if (id != updateDto.Id)
                return BadRequest(new { Success = false, Message = "El ID proporcionado en la URL no coincide con el ID del usuario a actualizar." });

            updateDto.IdUsuarioModifica = idUsuarioModifica;

            var response = await _usuarioService.UpdateUsuarioAsync(updateDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequestDto loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Legajo))
            {
                return BadRequest(new { Success = false, Message = "El legajo es requerido para la validación de credenciales." });
            }

            if (loginRequest.Legajo.Length > 5)
            {
                return BadRequest(new { Success = false, Message = "El legajo no puede tener más de 5 caracteres." });
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest(new { Success = false, Message = "La contraseña es requerida para la validación de credenciales." });
            }

            var response = await _usuarioService.ValidateCredentialsAsync(loginRequest.Legajo, loginRequest.Password);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del usuario debe ser un número válido mayor a 0." });
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int idUsuarioActual = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

            // Evitar que el usuario se elimine a sí mismo
            if (idUsuarioActual == id)
            {
                return BadRequest(new { Success = false, Message = "No está permitido que un usuario se elimine a sí mismo." });
            }

            var response = await _usuarioService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);

            if (response.Message?.Contains("no encontrado") == true)
                return NotFound(response);

            return BadRequest(response);
        }

        [HttpPatch("{id}/toggle-activo")]
        [AllowAnonymous]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del usuario debe ser un número válido mayor a 0." });
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int idUsuarioActual = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

            // Evitar que el usuario cambie su propio estado activo
            if (idUsuarioActual == id)
            {
                return BadRequest(new { Success = false, Message = "No está permitido que un usuario cambie su propio estado activo." });
            }

            var response = await _usuarioService.ToggleActivoAsync(id);
            if (response.Success)
                return Ok(response);

            if (response.Message?.Contains("No se encontró") == true || response.Message?.Contains("no encontrado") == true)
                return NotFound(response);

            return BadRequest(response);
        }
    }

    // Clase única para login por legajo
    public class LoginRequestDto
    {
        public string Legajo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}