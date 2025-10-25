using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.Planta;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class PlantaController : ControllerBase
    {
        private readonly PlantaService _plantaService;

        public PlantaController(PlantaService plantaService)
        {
            _plantaService = plantaService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar plantas
        public async Task<IActionResult> GetAll()
        {
            var result = await _plantaService.GetAllPlantasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar plantas específicas
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _plantaService.GetPlantaByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear plantas
        public async Task<IActionResult> Create([FromBody] CreatePlantaDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _plantaService.CreatePlantaAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdPlanta }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar plantas
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePlantaDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdPlanta)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _plantaService.UpdatePlantaAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar plantas
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _plantaService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
