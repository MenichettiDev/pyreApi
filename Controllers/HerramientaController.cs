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
            var response = await _herramientaService.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _herramientaService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var response = await _herramientaService.GetAvailableToolsAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("familia/{familiaId}")]
        public async Task<IActionResult> GetByFamilia(int familiaId)
        {
            var response = await _herramientaService.GetByFamiliaAsync(familiaId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _herramientaService.GetPagedAsync(page, pageSize);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHerramientaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _herramientaService.CreateHerramientaAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.IdHerramienta }, response);
            return BadRequest(response);
        }

        [HttpPut("status")]
        public async Task<IActionResult> ChangeStatus([FromBody] HerramientaStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _herramientaService.ChangeStatusAsync(statusDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _herramientaService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHerramientaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdHerramienta)
                return BadRequest("ID mismatch");

            var response = await _herramientaService.UpdateHerramientaAsync(updateDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("estado-fisico/{estadoFisicoId}")]
        public async Task<IActionResult> GetByEstadoFisico(int estadoFisicoId)
        {
            var response = await _herramientaService.GetByEstadoFisicoAsync(estadoFisicoId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("en-reparacion")]
        public async Task<IActionResult> GetInRepair()
        {
            var response = await _herramientaService.GetInRepairAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
