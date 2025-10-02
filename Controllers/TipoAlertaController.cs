using Microsoft.AspNetCore.Mvc;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoAlertaController : GenericController<TipoAlerta>
    {
        public TipoAlertaController(GenericService<TipoAlerta> service) : base(service)
        {
        }

        protected override object GetEntityId(TipoAlerta? entity)
        {
            return entity?.IdTipoAlerta ?? 0;
        }
    }
}
