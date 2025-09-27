using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObraController : GenericController<Obra>
    {
        public ObraController(GenericService<Obra> service) : base(service)
        {
        }

        protected override object GetEntityId(Obra? entity)
        {
            return entity?.IdObra ?? 0;
        }
    }
}
