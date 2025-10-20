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
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombreHerramienta = null,
            [FromQuery] int? idFamiliaHerramienta = null,
            [FromQuery] int? idUsuarioGenera = null,
            [FromQuery] int? idUsuarioResponsable = null,
            [FromQuery] int? idTipoMovimiento = null,
            [FromQuery] int? idObra = null,
            [FromQuery] int? idProveedor = null,
            [FromQuery] int? idEstadoFisico = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var result = await _movimientoService.GetAllMovimientosPaginatedAsync(
                page, pageSize, nombreHerramienta, idFamiliaHerramienta, idUsuarioGenera,
                idUsuarioResponsable, idTipoMovimiento, idObra, idProveedor,
                idEstadoFisico, fechaDesde, fechaHasta);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all-unpaginated")]
        public async Task<IActionResult> GetAllUnpaginated()
        {
            var result = await _movimientoService.GetAllMovimientosAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _movimientoService.GetMovimientoByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("herramienta/{herramientaId}")]
        public async Task<IActionResult> GetByHerramienta(int herramientaId)
        {
            var result = await _movimientoService.GetByHerramientaAsync(herramientaId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _movimientoService.GetByDateRangeAsync(startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMovimientoDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _movimientoService.CreateMovimientoAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdMovimiento }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMovimientoHerramientaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdMovimiento)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _movimientoService.UpdateMovimientoAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _movimientoService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
