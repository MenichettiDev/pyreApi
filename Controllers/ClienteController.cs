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
        [Authorize(Roles = "SuperAdmin,Administrador")] // SuperAdmin y Administrador pueden crear clientes
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
        [Authorize(Roles = "SuperAdmin,Administrador")] // SuperAdmin y Administrador pueden actualizar clientes
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClienteDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Asignar automáticamente el ID de la URL al DTO para evitar problemas de sincronización
            updateDto.IdCliente = id;

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
        public async Task<IActionResult> GetAllCombo(
            [FromQuery(Name = "q")] string? search = null,
            [FromQuery] bool? activo = null
        )
        {
            // Actualmente el servicio usa por defecto solo activos; mantenemos compatibilidad.
            var result = await _clienteService.GetAllComboAsync(search);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}/toggle-activo")]
        [Authorize(Roles = "SuperAdmin,Administrador")] // SuperAdmin y Administrador pueden cambiar estado activo
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
