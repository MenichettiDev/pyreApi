using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Cliente;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class ClienteController : ControllerBase
    {
        private readonly ClienteService _clienteService;

        public ClienteController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar clientes
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? cuit = null,
            [FromQuery] bool? activo = null
        )
        {
            var result = await _clienteService.GetAllClientesPaginatedAsync(
                page,
                pageSize,
                nombre,
                cuit,
                activo
            );
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar clientes específicos
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _clienteService.GetClienteByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear clientes
        public async Task<IActionResult> Create([FromBody] CreateClienteDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _clienteService.CreateClienteAsync(createDto);
            return result.Success
                ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdCliente }, result)
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar clientes
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClienteDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdCliente)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _clienteService.UpdateClienteAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar clientes
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _clienteService.DeleteAsyncLogico(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getClientesCombo")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar el combo de clientes
        public async Task<IActionResult> GetAllCombo([FromQuery] string? search = null)
        {
            var result = await _clienteService.GetAllComboAsync(search);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}/toggle-activo")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede cambiar estado activo
        public async Task<IActionResult> ToggleActivo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID del cliente debe ser un número válido mayor a 0.",
                    }
                );
            }

            var result = await _clienteService.ToggleActivoAsync(id);

            if (result.Success)
                return Ok(result);

            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(result);

            return BadRequest(result);
        }
    }
}
