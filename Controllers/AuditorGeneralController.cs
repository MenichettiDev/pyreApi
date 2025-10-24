using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1")] // Solo SuperAdmin y Administrador pueden acceder a auditoría
    public class AuditorGeneralController : ControllerBase
    {
        private readonly GenericService<AuditorGeneral> _service;

        public AuditorGeneralController(GenericService<AuditorGeneral> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var response = await _service.GetByIdAsync((int)id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _service.GetPagedAsync(page, pageSize);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("entity/{entityName}")]
        public async Task<IActionResult> GetByEntity(string entityName)
        {
            var response = await _service.GetAllAsync();
            if (response.Success && response.Data != null)
            {
                var filteredAudits = response.Data.Where(a => a.Entidad.Equals(entityName, StringComparison.OrdinalIgnoreCase)).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = $"Auditorías de la entidad {entityName} obtenidas correctamente",
                    Data = filteredAudits
                });
            }
            return BadRequest(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var response = await _service.GetAllAsync();
            if (response.Success && response.Data != null)
            {
                var userAudits = response.Data.Where(a => a.IdUsuario == userId).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = $"Auditorías del usuario {userId} obtenidas correctamente",
                    Data = userAudits
                });
            }
            return BadRequest(response);
        }

        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var response = await _service.GetAllAsync();
            if (response.Success && response.Data != null)
            {
                var rangeAudits = response.Data.Where(a => a.FechaHora >= startDate && a.FechaHora <= endDate).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = "Auditorías en el rango de fechas obtenidas correctamente",
                    Data = rangeAudits
                });
            }
            return BadRequest(response);
        }
    }
}
