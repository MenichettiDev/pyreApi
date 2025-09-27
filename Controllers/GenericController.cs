using Microsoft.AspNetCore.Mvc;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    public abstract class GenericController<T> : ControllerBase where T : class
    {
        protected readonly GenericService<T> _service;

        protected GenericController(GenericService<T> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _service.GetPagedAsync(page, pageSize);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] T entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _service.AddAsync(entity);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = GetEntityId(response.Data) }, response);
            return BadRequest(response);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] T entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _service.UpdateAsync(entity);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        protected abstract object GetEntityId(T? entity);
    }
}
