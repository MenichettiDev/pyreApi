using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoMovimientoHerramientaController : GenericController<TipoMovimientoHerramienta>
    {
        public TipoMovimientoHerramientaController(GenericService<TipoMovimientoHerramienta> service) : base(service)
        {
        }

        protected override object GetEntityId(TipoMovimientoHerramienta? entity)
        {
            return entity?.IdTipoMovimiento ?? 0;
        }
    }
}
