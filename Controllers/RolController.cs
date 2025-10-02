using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly GenericService<Rol> _service;
        private readonly ApplicationDbContext _context;

        public RolController(GenericService<Rol> service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        // GET: api/rol (Lista de todos los roles)
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Rol.ToListAsync();

                return Ok(new
                {
                    status = 200,
                    message = "Lista de roles obtenida correctamente.",
                    // totalRoles = roles.Count,
                    data = roles
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "Error al obtener los roles.",
                    details = ex.Message
                });
            }
        }

        // GET: api/rol/{id} (Un rol específico)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRol(int id)
        {
            try
            {
                var rol = await _context.Rol.FindAsync(id);
                if (rol == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "El rol no existe."
                    });
                }

                return Ok(new
                {
                    status = 200,
                    message = "Rol encontrado.",
                    data = rol
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "Error al buscar el rol.",
                    details = ex.Message
                });
            }
        }

        // POST: api/rol (Crear un nuevo rol)
        [HttpPost]
        public async Task<IActionResult> PostRol([FromBody] Rol rol)
        {
            try
            {
                // Validar que el nombre de rol no esté vacío
                if (string.IsNullOrEmpty(rol.NombreRol))
                {
                    return BadRequest(new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El nombre del rol es obligatorio."
                    });
                }

                _context.Rol.Add(rol);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetRol),
                    new { id = rol.Id },
                    new
                    {
                        status = 201,
                        message = "Rol creado correctamente.",
                        rol
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "Error al crear el rol.",
                    details = ex.Message
                });
            }
        }

        // PUT: api/rol/{id} (Actualizar rol)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRol(int id, [FromBody] Rol rol)
        {
            try
            {
                if (id != rol.Id)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ID en la URL no coincide con el ID del cuerpo de la solicitud."
                    });
                }

                _context.Entry(rol).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Rol.AnyAsync(e => e.Id == id))
                    {
                        return NotFound(new
                        {
                            status = 404,
                            error = "Not Found",
                            message = "El rol no existe."
                        });
                    }
                    throw;
                }

                return Ok(new
                {
                    status = 200,
                    message = "Rol actualizado correctamente.",
                    rol
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "Error al actualizar el rol.",
                    details = ex.Message
                });
            }
        }

        // DELETE: api/rol/{id} (Eliminar rol)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            try
            {
                var rol = await _context.Rol.FindAsync(id);
                if (rol == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "El rol no existe."
                    });
                }

                // Verificar si el rol está asignado a usuarios
                var tieneUsuarios = await _context.Usuario.AnyAsync(u => u.RolId == id);
                if (tieneUsuarios)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "No se puede eliminar el rol porque está asignado a usuarios.",
                        details = new
                        {
                            RolId = id,
                            Relacion = "Usuario",
                            Motivo = "Restricción de clave foránea"
                        }
                    });
                }

                _context.Rol.Remove(rol);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = 200,
                    message = "Rol eliminado correctamente.",
                    rolEliminadoId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "Error al eliminar el rol.",
                    details = ex.Message
                });
            }
        }
    }
}
