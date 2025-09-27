using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.ReparacionHerramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReparacionHerramientaController : ControllerBase
    {
        private readonly ReparacionHerramientaService _reparacionService;

        public ReparacionHerramientaController(ReparacionHerramientaService reparacionService)
        {
            _reparacionService = reparacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _reparacionService.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _reparacionService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveRepairs()
        {
            var response = await _reparacionService.GetActiveRepairsAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReparacionDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _reparacionService.CreateReparacionAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.IdReparacion }, response);
            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _reparacionService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
