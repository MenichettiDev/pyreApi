using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.MovimientoHerramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientoHerramientaController : ControllerBase
    {
        private readonly MovimientoHerramientaService _movimientoService;

        public MovimientoHerramientaController(MovimientoHerramientaService movimientoService)
        {
            _movimientoService = movimientoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _movimientoService.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _movimientoService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("herramienta/{herramientaId}")]
        public async Task<IActionResult> GetByHerramienta(int herramientaId)
        {
            var response = await _movimientoService.GetByHerramientaAsync(herramientaId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var response = await _movimientoService.GetByDateRangeAsync(startDate, endDate);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMovimientoDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _movimientoService.CreateMovimientoAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.IdMovimiento }, response);
            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _movimientoService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
