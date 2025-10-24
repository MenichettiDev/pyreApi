using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.Proveedor;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class ProveedorController : ControllerBase
    {
        private readonly ProveedorService _proveedorService;

        public ProveedorController(ProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        [Authorize(Roles = "1,2,3,4")] // Todos pueden consultar proveedores
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? cuit = null,
            [FromQuery] bool? activo = null)
        {
            var result = await _proveedorService.GetAllProveedoresPaginatedAsync(page, pageSize, nombre, cuit, activo);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3,4")] // Todos pueden consultar proveedores específicos
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _proveedorService.GetProveedorByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede crear proveedores
        public async Task<IActionResult> Create([FromBody] CreateProveedorDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _proveedorService.CreateProveedorAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdProveedor }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede actualizar proveedores
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProveedorDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdProveedor)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _proveedorService.UpdateProveedorAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede eliminar proveedores
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _proveedorService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
