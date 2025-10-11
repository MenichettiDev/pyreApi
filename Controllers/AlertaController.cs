using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Alerta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertaController : ControllerBase
    {
        private readonly AlertaService _alertaService;

        public AlertaController(AlertaService alertaService)
        {
            _alertaService = alertaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _alertaService.GetAllAlertasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _alertaService.GetAlertaByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAlertaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _alertaService.CreateAlertaAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdAlerta }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAlertaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdAlerta)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _alertaService.UpdateAlertaAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _alertaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("herramienta/{idHerramienta}")]
        public async Task<IActionResult> GetByHerramienta(int idHerramienta)
        {
            var result = await _alertaService.GetAlertasByHerramientaAsync(idHerramienta);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var result = await _alertaService.GetUnreadAlertasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _alertaService.MarkAlertaAsReadAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("tipo-alerta/{idTipoAlerta}")]
        public async Task<IActionResult> GetByTipoAlerta(int idTipoAlerta)
        {
            var result = await _alertaService.GetByTipoAlertaAsync(idTipoAlerta);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("count-alertas-pendientes")]
        public async Task<IActionResult> GetCountAlertasPendientes()
        {
            var result = await _alertaService.GetCountAlertasPendientesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("count-alertas-vencidas")]
        public async Task<IActionResult> GetCountAlertasVencidas()
        {
            var result = await _alertaService.GetCountAlertasVencidasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
