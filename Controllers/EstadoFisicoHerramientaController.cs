using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoFisicoHerramientaController : GenericController<EstadoFisicoHerramienta>
    {
        public EstadoFisicoHerramientaController(GenericService<EstadoFisicoHerramienta> service) : base(service)
        {
        }

        protected override object GetEntityId(EstadoFisicoHerramienta? entity)
        {
            return entity?.Id ?? 0;
        }
    }
}
