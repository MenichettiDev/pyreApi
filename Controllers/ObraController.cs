using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Obra;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class ObraController : ControllerBase
    {
        private readonly ObraService _obraService;

        public ObraController(ObraService obraService)
        {
            _obraService = obraService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar obras
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? ubicacion = null,
            [FromQuery] string? codigo = null,
            [FromQuery] int? idCliente = null
        )
        {
            var result = await _obraService.GetAllObrasPaginatedAsync(
                page,
                pageSize,
                nombre,
                ubicacion,
                codigo,
                idCliente
            );
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar obras específicas
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _obraService.GetObraByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear obras
        public async Task<IActionResult> Create([FromBody] CreateObraDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createDto.IdCliente <= 0)
                return BadRequest("El ID del cliente debe ser un número válido mayor a 0.");

            var result = await _obraService.CreateObraAsync(createDto);
            return result.Success
                ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdObra }, result)
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar obras
        public async Task<IActionResult> Update(int id, [FromBody] UpdateObraDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdObra)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            if (updateDto.IdCliente <= 0)
                return BadRequest("El ID del cliente debe ser un número válido mayor a 0.");

            var result = await _obraService.UpdateObraAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar obras
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _obraService.DeleteAsyncLogico(id);
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
                        Message = "El ID de la obra debe ser un número válido mayor a 0.",
                    }
                );
            }

            var result = await _obraService.ToggleActivoAsync(id);

            if (result.Success)
                return Ok(result);

            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        [HttpGet("getObrasCombo")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar el combo de obras
        public async Task<IActionResult> GetAllCombo(
            [FromQuery] int? idCliente = null,
            [FromQuery] string? search = null
        )
        {
            var result = await _obraService.GetAllComboAsync(idCliente, search);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
