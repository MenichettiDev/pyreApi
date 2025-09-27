using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Usuario;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _usuarioService.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _usuarioService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("dni/{dni}")]
        public async Task<IActionResult> GetByDni(string dni)
        {
            var response = await _usuarioService.GetByDniAsync(dni);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var response = await _usuarioService.GetActiveUsersAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _usuarioService.CreateUsuarioAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequest loginRequest)
        {
            var response = await _usuarioService.ValidateCredentialsAsync(loginRequest.Dni, loginRequest.Password);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _usuarioService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
public class LoginDto
{
    public string Dni { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

