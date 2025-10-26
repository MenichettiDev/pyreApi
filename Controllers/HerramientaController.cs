using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.Herramienta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class HerramientaController : ControllerBase
    {
        private readonly HerramientaService _herramientaService;

        public HerramientaController(HerramientaService herramientaService)
        {
            _herramientaService = herramientaService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar herramientas
        public async Task<IActionResult> GetAll()
        {
            var result = await _herramientaService.GetAllHerramientasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar herramientas paginadas
        public async Task<IActionResult> GetPaged(
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] string? codigo = null,
           [FromQuery] string? nombre = null,
           [FromQuery] string? marca = null,
           [FromQuery] bool? estado = null)
        {
            var result = await _herramientaService.GetPagedAsync(page, pageSize, codigo, nombre, marca, estado);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar herramientas específicas
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _herramientaService.GetHerramientaByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden crear herramientas
        public async Task<IActionResult> Create([FromBody] CreateHerramientaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _herramientaService.CreateHerramientaAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdHerramienta }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden actualizar herramientas
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
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar herramientas
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _herramientaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("available")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver herramientas disponibles
        public async Task<IActionResult> GetAvailable()
        {
            var result = await _herramientaService.GetAvailableToolsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("familia/{familiaId}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden filtrar por familia
        public async Task<IActionResult> GetByFamilia(int familiaId)
        {
            var result = await _herramientaService.GetByFamiliaAsync(familiaId);
            return result.Success ? Ok(result) : BadRequest(result);
        }



        [HttpPut("status")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden cambiar estado
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _herramientaService.UpdateStatusAsync(updateStatusDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("estado-fisico/{estadoFisicoId}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden filtrar por estado físico
        public async Task<IActionResult> GetByEstadoFisico(int estadoFisicoId)
        {
            var result = await _herramientaService.GetByEstadoFisicoAsync(estadoFisicoId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("en-reparacion")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver herramientas en reparación
        public async Task<IActionResult> GetInRepair()
        {
            var result = await _herramientaService.GetInRepairAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("count-herramientas-totales")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver estadísticas
        public async Task<IActionResult> GetTotalHerramientas()
        {
            var result = await _herramientaService.GetTotalHerramientasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("count-herramientas-disponibles")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver estadísticas
        public async Task<IActionResult> GetTotalHerramientasDisponibles()
        {
            var result = await _herramientaService.GetTotalHerramientasDisponiblesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("count-herramientas-en-prestamo")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver estadísticas
        public async Task<IActionResult> GetTotalHerramientasEnPrestamo()
        {
            var result = await _herramientaService.GetTotalHerramientasEnPrestamoAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpGet("herramientas-en-reparacion")]
        [HttpGet("count-herramientas-en-reparacion")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver estadísticas
        public async Task<IActionResult> GetTotalHerramientasEnReparacion()
        {
            var result = await _herramientaService.GetTotalHerramientasEnReparacionAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("disponibilidad/{disponibilidadId}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden filtrar por disponibilidad
        public async Task<IActionResult> GetByDisponibilidad(int disponibilidadId)
        {
            var result = await _herramientaService.GetByDisponibilidadAsync(disponibilidadId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("disponibilidad")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden filtrar por múltiple disponibilidad
        public async Task<IActionResult> GetByMultipleDisponibilidad(
            [FromQuery] string ids,
            [FromQuery] string? search = null)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return BadRequest("Se requiere al menos un ID de disponibilidad");

            try
            {
                var disponibilidadIds = ids.Split(',')
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();

                var result = await _herramientaService.GetByMultipleDisponibilidadAsync(disponibilidadIds, search);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (FormatException)
            {
                return BadRequest("Los IDs deben ser números válidos separados por comas");
            }
        }

    }

}