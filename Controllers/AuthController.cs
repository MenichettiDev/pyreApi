using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using pyreApi.Services;
using pyreApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace pyreApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioService usuarioService,
            IConfiguration configuration,
            ILogger<AuthController> logger
        )
        {
            _usuarioService = usuarioService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Dni) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning(
                    "Intento de login con campos vacíos. DNI: '{Dni}', Password vacío: {PasswordEmpty}",
                    request.Dni ?? "(nulo)",
                    string.IsNullOrEmpty(request.Password)
                );
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El DNI y la contraseña son obligatorios.",
                    }
                );
            }

            var validationResponse = await _usuarioService.ValidateCredentialsAsync(request.Dni, request.Password);
            if (!validationResponse.Success)
            {
                _logger.LogWarning("Login fallido para el DNI: {Dni}", request.Dni);
                return Unauthorized(
                    new
                    {
                        status = 401,
                        error = "Unauthorized",
                        message = "DNI o contraseña incorrectos.",
                    }
                );
            }

            var userResponse = await _usuarioService.GetByDniAsync(request.Dni);
            if (!userResponse.Success || userResponse.Data == null)
            {
                _logger.LogError("Error al obtener usuario después de validación exitosa: {Dni}", request.Dni);
                return StatusCode(500, new
                {
                    status = 500,
                    error = "Internal Server Error",
                    message = "Error interno del servidor.",
                });
            }

            var usuario = userResponse.Data;
            _logger.LogInformation("Usuario logueado exitosamente: {Dni}", usuario.Dni);
            var token = GenerateJwtToken(usuario);

            return Ok(
                new
                {
                    status = 200,
                    message = "Inicio de sesión exitoso.",
                    token,
                    usuario = new
                    {
                        usuario.Id,
                        usuario.Nombre,
                        usuario.Email,
                        usuario.Dni,
                        usuario.RolId,
                        RolNombre = usuario.Rol?.NombreRol,
                        usuario.Avatar,
                    },
                }
            );
        }

        // Método auxiliar para generar token JWT
        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(
                _configuration["Jwt:Key"]
                    ?? throw new ArgumentNullException("Jwt:Key no está definido")
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Usuario"),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Dni { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}