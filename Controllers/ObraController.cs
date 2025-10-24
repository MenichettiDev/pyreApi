using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "1,2,3,4")] // Todos los roles pueden consultar obras
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] string? ubicacion = null,
            [FromQuery] string? codigo = null)
        {
            var result = await _obraService.GetAllObrasPaginatedAsync(page, pageSize, nombre, ubicacion, codigo);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3,4")] // Todos los roles pueden consultar obras específicas
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _obraService.GetObraByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede crear obras
        public async Task<IActionResult> Create([FromBody] CreateObraDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _obraService.CreateObraAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdObra }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede actualizar obras
        public async Task<IActionResult> Update(int id, [FromBody] UpdateObraDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdObra)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _obraService.UpdateObraAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede eliminar obras
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _obraService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}