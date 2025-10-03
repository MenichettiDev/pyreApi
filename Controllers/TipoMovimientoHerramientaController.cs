using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.TipoMovimientoHerramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoMovimientoHerramientaController : ControllerBase
    {
        private readonly TipoMovimientoHerramientaService _tipoMovimientoHerramientaService;

        public TipoMovimientoHerramientaController(TipoMovimientoHerramientaService tipoMovimientoHerramientaService)
        {
            _tipoMovimientoHerramientaService = tipoMovimientoHerramientaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _tipoMovimientoHerramientaService.GetAllTiposMovimientoAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tipoMovimientoHerramientaService.GetTipoMovimientoByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTipoMovimientoHerramientaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tipoMovimientoHerramientaService.CreateTipoMovimientoAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdTipoMovimiento }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
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
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tipoMovimientoHerramientaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
