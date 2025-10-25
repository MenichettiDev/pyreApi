using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.TipoAlerta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class TipoAlertaController : ControllerBase
    {
        private readonly TipoAlertaService _tipoAlertaService;

        public TipoAlertaController(TipoAlertaService tipoAlertaService)
        {
            _tipoAlertaService = tipoAlertaService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar tipos de alerta
        public async Task<IActionResult> GetAll()
        {
            var result = await _tipoAlertaService.GetAllTiposAlertaAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar tipos de alerta específicos
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _tipoAlertaService.GetTipoAlertaByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear tipos de alerta
        public async Task<IActionResult> Create([FromBody] CreateTipoAlertaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tipoAlertaService.CreateTipoAlertaAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdTipoAlerta }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar tipos de alerta
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoAlertaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdTipoAlerta)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _tipoAlertaService.UpdateTipoAlertaAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar tipos de alerta
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _tipoAlertaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
