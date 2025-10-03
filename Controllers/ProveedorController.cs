using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Proveedor;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly ProveedorService _proveedorService;

        public ProveedorController(ProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _proveedorService.GetAllProveedoresAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _proveedorService.GetProveedorByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProveedorDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _proveedorService.CreateProveedorAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.IdProveedor }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProveedorDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdProveedor)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _proveedorService.UpdateProveedorAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _proveedorService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
