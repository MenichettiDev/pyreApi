using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliaHerramientasController : GenericController<FamiliaHerramientas>
    {
        public FamiliaHerramientasController(GenericService<FamiliaHerramientas> service) : base(service)
        {
        }

        protected override object GetEntityId(FamiliaHerramientas? entity)
        {
            return entity?.IdFamilia ?? 0;
        }
    }
}
