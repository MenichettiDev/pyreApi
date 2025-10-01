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

        // [HttpPost("create")]
        // public async Task<IActionResult> CreateFromDto([FromBody] CreateObraDto dto)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);

        //     var obra = new Obra
        //     {
        //         Codigo = dto.Codigo,
        //         NombreObra = dto.NombreObra,
        //         Ubicacion = dto.Ubicacion,
        //         FechaInicio = dto.FechaInicio,
        //         FechaFin = dto.FechaFin,
        //     };

        //     var response = await _service.AddAsync(obra);
        //     if (response.Success)
        //         return CreatedAtAction(nameof(GetById), new { id = response.Data?.IdObra }, response);
        //     return BadRequest(response);
        // }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFromDto(int id, [FromBody] UpdateObraDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.IdObra)
                return BadRequest("ID mismatch");

            var obra = new Obra
            {
                IdObra = dto.IdObra,
                Codigo = dto.Codigo,
                NombreObra = dto.NombreObra,
                Ubicacion = dto.Ubicacion,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFinEstimada,
            };

            var response = await _service.UpdateAsync(obra);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}