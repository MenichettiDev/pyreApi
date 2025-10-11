using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Herramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HerramientaController : ControllerBase
    {
        private readonly HerramientaService _herramientaService;

        public HerramientaController(HerramientaService herramientaService)
        {
            _herramientaService = herramientaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _herramientaService.GetAllHerramientasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _herramientaService.GetHerramientaByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHerramientaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _herramientaService.CreateHerramientaAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdHerramienta }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHerramientaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdHerramienta)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _herramientaService.UpdateHerramientaAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _herramientaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var result = await _herramientaService.GetAvailableToolsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("familia/{familiaId}")]
        public async Task<IActionResult> GetByFamilia(int familiaId)
        {
            var result = await _herramientaService.GetByFamiliaAsync(familiaId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PagedRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _herramientaService.GetPagedAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _herramientaService.UpdateStatusAsync(updateStatusDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("estado-fisico/{estadoFisicoId}")]
        public async Task<IActionResult> GetByEstadoFisico(int estadoFisicoId)
        {
            var result = await _herramientaService.GetByEstadoFisicoAsync(estadoFisicoId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("en-reparacion")]
        public async Task<IActionResult> GetInRepair()
        {
            var result = await _herramientaService.GetInRepairAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("herramientas-totales")]
        public async Task<IActionResult> GetTotalHerramientas()
        {
            var result = await _herramientaService.GetTotalHerramientasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("herramientas-disponibles")]
        public async Task<IActionResult> GetTotalHerramientasDisponibles()
        {
            var result = await _herramientaService.GetTotalHerramientasDisponiblesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("herramientas-en-prestamo")]
        public async Task<IActionResult> GetTotalHerramientasEnPrestamo()
        {
            var result = await _herramientaService.GetTotalHerramientasEnPrestamoAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpGet("herramientas-en-reparacion")]
        public async Task<IActionResult> GetTotalHerramientasEnReparacion()
        {
            var result = await _herramientaService.GetTotalHerramientasEnReparacionAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}