using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoDisponibilidadHerramientaController : GenericController<EstadoDisponibilidadHerramienta>
    {
        public EstadoDisponibilidadHerramientaController(GenericService<EstadoDisponibilidadHerramienta> service) : base(service)
        {
        }

        protected override object GetEntityId(EstadoDisponibilidadHerramienta? entity)
        {
            return entity?.Id ?? 0;
        }
    }
}
