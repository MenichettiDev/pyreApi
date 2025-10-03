using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.EstadoDisponibilidad;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoDisponibilidadController : ControllerBase
    {
        private readonly EstadoDisponibilidadService _estadoDisponibilidadService;

        public EstadoDisponibilidadController(EstadoDisponibilidadService estadoDisponibilidadService)
        {
            _estadoDisponibilidadService = estadoDisponibilidadService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _estadoDisponibilidadService.GetAllEstadosDisponibilidadAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _estadoDisponibilidadService.GetEstadoDisponibilidadByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEstadoDisponibilidadDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _estadoDisponibilidadService.CreateEstadoDisponibilidadAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdEstadoDisponibilidad }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoDisponibilidadDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdEstadoDisponibilidad)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _estadoDisponibilidadService.UpdateEstadoDisponibilidadAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _estadoDisponibilidadService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
