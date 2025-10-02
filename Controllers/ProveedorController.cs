using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorController : GenericController<Proveedor>
    {
        public ProveedorController(GenericService<Proveedor> service) : base(service)
        {
        }

        protected override object GetEntityId(Proveedor? entity)
        {
            return entity?.IdProveedor ?? 0;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProviders()
        {
            var response = await _service.GetAllAsync();
            if (response.Success && response.Data != null)
            {
                var activeProviders = response.Data.Where(p => p.Activo).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = "Proveedores activos obtenidos correctamente",
                    Data = activeProviders
                });
            }
            return BadRequest(response);
        }
    }
}
