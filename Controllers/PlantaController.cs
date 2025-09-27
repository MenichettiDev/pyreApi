using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantaController : GenericController<Planta>
    {
        public PlantaController(GenericService<Planta> service) : base(service)
        {
        }

        protected override object GetEntityId(Planta? entity)
        {
            return entity?.IdPlanta ?? 0;
        }
    }
}
