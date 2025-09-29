using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Obra;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateObraDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Mapear DTO a entidad
            var obra = new Obra
            {
                Codigo = dto.Codigo,
                NombreObra = dto.NombreObra,
                Ubicacion = dto.Ubicacion,
                FechaInicio = dto.FechaInicio,
                FechaFinEstimada = dto.FechaFinEstimada,
                ResponsableObra = dto.ResponsableObra,

            };

            var response = await _service.AddAsync(obra);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.IdObra }, response);
            return BadRequest(response);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CreateObraDto dto) // Reusar el mismo DTO
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Mapear DTO a entidad (necesitarás obtener el ID de alguna forma)
            var obra = new Obra
            {
                // IdObra = ?, // Necesitarás pasar esto de alguna forma
                Codigo = dto.Codigo,
                NombreObra = dto.NombreObra,
                Ubicacion = dto.Ubicacion,
                FechaInicio = dto.FechaInicio,
                FechaFinEstimada = dto.FechaFinEstimada,
                ResponsableObra = dto.ResponsableObra,
            };

            var response = await _service.UpdateAsync(obra);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // protected override object GetEntityId(Obra? entity)
        // {
        //     return entity?.IdObra ?? 0;
        // }
    }

}
