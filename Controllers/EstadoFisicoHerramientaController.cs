using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.EstadoFisicoHerramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class EstadoFisicoHerramientaController : ControllerBase
    {
        private readonly EstadoFisicoHerramientaService _estadoFisicoHerramientaService;

        public EstadoFisicoHerramientaController(EstadoFisicoHerramientaService estadoFisicoHerramientaService)
        {
            _estadoFisicoHerramientaService = estadoFisicoHerramientaService;
        }

        [HttpGet]
        [Authorize(Roles = "1,2,3,4")] // Todos los roles pueden consultar estados físicos
        public async Task<IActionResult> GetAll()
        {
            var result = await _estadoFisicoHerramientaService.GetAllEstadosFisicosAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3,4")] // Todos los roles pueden consultar estados específicos
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _estadoFisicoHerramientaService.GetEstadoFisicoByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede crear estados físicos
        public async Task<IActionResult> Create([FromBody] CreateEstadoFisicoHerramientaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _estadoFisicoHerramientaService.CreateEstadoFisicoAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdEstadoFisico }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede actualizar estados físicos
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoFisicoHerramientaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdEstadoFisico)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _estadoFisicoHerramientaService.UpdateEstadoFisicoAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Solo SuperAdmin puede eliminar estados físicos
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _estadoFisicoHerramientaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
