using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.TipoMovimientoHerramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class TipoMovimientoHerramientaController : ControllerBase
    {
        private readonly TipoMovimientoHerramientaService _tipoMovimientoHerramientaService;

        public TipoMovimientoHerramientaController(TipoMovimientoHerramientaService tipoMovimientoHerramientaService)
        {
            _tipoMovimientoHerramientaService = tipoMovimientoHerramientaService;
        }

        [HttpGet]
        [Authorize(Roles = "1,2,3,4")] // Todos los roles pueden consultar tipos de movimiento
        public async Task<IActionResult> GetAll()
        {
            var result = await _tipoMovimientoHerramientaService.GetAllTiposMovimientoAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3,4")] // Todos los roles pueden consultar tipos de movimiento específicos
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tipoMovimientoHerramientaService.GetTipoMovimientoByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede crear tipos de movimiento
        public async Task<IActionResult> Create([FromBody] CreateTipoMovimientoHerramientaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tipoMovimientoHerramientaService.CreateTipoMovimientoAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdTipoMovimiento }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin y Administrador pueden actualizar tipos de movimiento
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoMovimientoHerramientaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdTipoMovimiento)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _tipoMovimientoHerramientaService.UpdateTipoMovimientoAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede eliminar tipos de movimiento
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tipoMovimientoHerramientaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
