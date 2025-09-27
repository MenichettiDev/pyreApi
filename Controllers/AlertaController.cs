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
            var response = await _alertaService.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _alertaService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAlerts()
        {
            var response = await _alertaService.GetActiveAlertsAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAlertaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _alertaService.CreateAlertaAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.IdAlerta }, response);
            return BadRequest(response);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var response = await _alertaService.MarkAsReadAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _alertaService.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
