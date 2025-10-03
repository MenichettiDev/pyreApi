using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.FamiliaHerramientas;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliaHerramientasController : ControllerBase
    {
        private readonly FamiliaHerramientasService _familiaHerramientasService;

        public FamiliaHerramientasController(FamiliaHerramientasService familiaHerramientasService)
        {
            _familiaHerramientasService = familiaHerramientasService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _familiaHerramientasService.GetAllFamiliasAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _familiaHerramientasService.GetFamiliaByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFamiliaHerramientasDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _familiaHerramientasService.CreateFamiliaAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdFamilia }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFamiliaHerramientasDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdFamilia)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _familiaHerramientasService.UpdateFamiliaAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _familiaHerramientasService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
