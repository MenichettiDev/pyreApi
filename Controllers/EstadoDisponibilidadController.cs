using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.EstadoDisponibilidad;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class EstadoDisponibilidadController : ControllerBase
    {
        private readonly EstadoDisponibilidadService _estadoDisponibilidadService;

        public EstadoDisponibilidadController(EstadoDisponibilidadService estadoDisponibilidadService)
        {
            _estadoDisponibilidadService = estadoDisponibilidadService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar estados de disponibilidad
        public async Task<IActionResult> GetAll()
        {
            var result = await _estadoDisponibilidadService.GetAllEstadosDisponibilidadAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar estados específicos
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _estadoDisponibilidadService.GetEstadoDisponibilidadByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear estados
        public async Task<IActionResult> Create([FromBody] CreateEstadoDisponibilidadDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _estadoDisponibilidadService.CreateEstadoDisponibilidadAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdEstadoDisponibilidad }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar estados
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
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar estados
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _estadoDisponibilidadService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
