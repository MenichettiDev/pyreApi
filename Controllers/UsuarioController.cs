using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.Usuario;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Permitir acceso anónimo a todos los endpoints
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
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
            var response = await _usuarioService.GetUsuarioByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("dni/{dni}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDni(string dni)
        {
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _usuarioService.CreateUsuarioAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.Id)
                return BadRequest("ID mismatch");

            var response = await _usuarioService.UpdateUsuarioAsync(updateDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequestDto loginRequest)
        {
            var response = await _usuarioService.ValidateCredentialsAsync(loginRequest.Legajo, loginRequest.Password);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _usuarioService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
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