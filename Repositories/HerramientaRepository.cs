using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;

namespace pyreApi.Repositories
{
    public class HerramientaRepository : GenericRepository<Herramienta>
    {
        public HerramientaRepository(ApplicationDbContext context)
            : base(context) { }

        public override async Task<IEnumerable<Herramienta>> GetAllAsync()
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .ToListAsync();
        }

        public override async Task<Herramienta?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .FirstOrDefaultAsync(h => h.IdHerramienta == id);
        }

        public async Task<IEnumerable<Herramienta>> GetByEstadoAsync(int estadoId)
        {
            var query = _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .AsQueryable();

            if (estadoId != 0)
            {
                query = query.Where(h => h.IdEstadoFisico == estadoId);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByFamiliaAsync(int familiaId)
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => h.IdFamilia == familiaId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByPlantaAsync(int plantaId)
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => h.IdPlanta == plantaId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetAvailableToolsAsync()
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h =>
                    h.EstadoDisponibilidad.Descripcion.ToLower().Contains("disponible")
                    && h.Activo == true
                )
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetInRepairAsync()
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => h.EstadoDisponibilidad.Id == 3)
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByDisponibilidadAsync(int disponibilidadId)
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => h.IdDisponibilidad == disponibilidadId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByMultipleDisponibilidadAsync(
            IEnumerable<int> disponibilidadIds
        )
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => disponibilidadIds.Contains(h.IdDisponibilidad))
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetFilteredHerramientasAsync(
            string? codigo,
            string? nombre,
            string? marca,
            bool? estado,
            int? idDisponibilidad
        )
        {
            var query = _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(codigo))
                query = query.Where(h => h.Codigo != null && h.Codigo.Contains(codigo));

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(h =>
                    h.NombreHerramienta != null && h.NombreHerramienta.Contains(nombre)
                );

            if (!string.IsNullOrWhiteSpace(marca))
                query = query.Where(h => h.Marca != null && h.Marca.Contains(marca));

            if (estado.HasValue)
                query = query.Where(h => h.Activo == estado.Value);

            if (idDisponibilidad != null)
                query = query.Where(h => h.IdDisponibilidad == idDisponibilidad); // Aplicar filtro de disponibilidad

            return await query.OrderBy(h => h.IdHerramienta).ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByMultipleDisponibilidadAsync(
            List<int> disponibilidadIds,
            string? searchText = null
        )
        {
            var query = _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => disponibilidadIds.Contains(h.IdDisponibilidad) && h.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(h =>
                    (h.NombreHerramienta != null && h.NombreHerramienta.Contains(searchText)) ||
                    (h.Marca != null && h.Marca.Contains(searchText)) ||
                    (h.Codigo != null && h.Codigo.Contains(searchText)));
            }

            return await query
                .OrderBy(h => h.NombreHerramienta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetHerramientasEnPrestamoByUsuarioAsync(int idUsuarioResponsable)
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Include(h => h.Movimientos)
                .Where(h => h.IdDisponibilidad == 2 &&
                           h.Activo == true &&
                           h.Movimientos.Any(m => m.IdUsuarioResponsable == idUsuarioResponsable))
                .ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetHerramientasEnReparacionByProveedorAsync(int idProveedor)
        {
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Include(h => h.Movimientos)
                .Where(h => h.IdDisponibilidad == 3 &&
                           h.Activo == true &&
                           h.Movimientos.Any(m => m.IdProveedor == idProveedor))
                .ToListAsync();
        }

        public async Task<string?> GetFamiliaNombre(int familiaId)
        {
            return await _context
                .FamiliaHerramientas.Where(f => f.IdFamilia == familiaId)
                .Select(f => f.NombreFamilia)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetEstadoFisicoNombre(int estadoFisicoId)
        {
            return await _context
                .EstadoFisicoHerramienta.Where(e => e.Id == estadoFisicoId)
                .Select(e => e.Descripcion)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetEstadoDisponibilidadNombre(int disponibilidadId)
        {
            return await _context
                .EstadoDisponibilidadHerramienta.Where(e => e.Id == disponibilidadId)
                .Select(e => e.Descripcion)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetPlantaNombre(int plantaId)
        {
            return await _context
                .Planta.Where(p => p.IdPlanta == plantaId)
                .Select(p => p.NombrePlanta)
                .FirstOrDefaultAsync();
        }

        public async Task<object> GetHerramientasByUsuarioAsync(int usuarioId)
        {
            // Herramientas prestadas agrupadas por usuario con sus datos
            var herramientasPrestadas = await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Include(h => h.Movimientos)
                    .ThenInclude(m => m.UsuarioResponsable)
                .Where(h => h.IdDisponibilidad == 2 && h.Activo && !h.Eliminado)
                .ToListAsync();

            var usuariosConHerramientas = herramientasPrestadas
                .SelectMany(h => h.Movimientos
                    .Where(m => m.IdTipoMovimiento == 1)
                    .Select(m => new { Herramienta = h, Movimiento = m }))
                .GroupBy(x => new
                {
                    x.Movimiento.IdUsuarioResponsable,
                    Nombre = x.Movimiento.UsuarioResponsable?.Nombre ?? "Sin nombre",
                    Apellido = x.Movimiento.UsuarioResponsable?.Apellido ?? "Sin apellido",
                    Legajo = x.Movimiento.UsuarioResponsable?.Legajo ?? "Sin legajo"
                })
                .Select(g => new
                {
                    Usuario = new
                    {
                        Id = g.Key.IdUsuarioResponsable,
                        NombreCompleto = $"{g.Key.Nombre} {g.Key.Apellido}",
                        g.Key.Legajo
                    },
                    CantidadHerramientas = g.Count(),
                    Herramientas = g.Select(x => new
                    {
                        IdHerramienta = x.Herramienta.IdHerramienta,
                        Codigo = x.Herramienta.Codigo,
                        NombreHerramienta = x.Herramienta.NombreHerramienta,
                        Familia = x.Herramienta.Familia?.NombreFamilia,
                        FechaPrestamo = x.Movimiento.Fecha
                    }).ToList()
                })
                .OrderByDescending(x => x.CantidadHerramientas)
                .ToList();

            // Herramientas en mantenimiento agrupadas por proveedor con sus datos
            var herramientasEnMantenimiento = await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Include(h => h.Movimientos)
                    .ThenInclude(m => m.Proveedor)
                .Where(h => h.IdDisponibilidad == 3 && h.Activo && !h.Eliminado)
                .ToListAsync();

            var proveedoresConHerramientas = herramientasEnMantenimiento
                .SelectMany(h => h.Movimientos
                    .Where(m => m.IdTipoMovimiento == 3 && m.IdProveedor.HasValue)
                    .Select(m => new { Herramienta = h, Movimiento = m }))
                .GroupBy(x => new
                {
                    IdProveedor = x.Movimiento.IdProveedor.Value,
                    NombreProveedor = x.Movimiento.Proveedor?.NombreProveedor ?? "Proveedor desconocido",
                    Contacto = x.Movimiento.Proveedor?.Contacto ?? "Sin contacto",
                    Telefono = x.Movimiento.Proveedor?.Telefono ?? "Sin teléfono"
                })
                .Select(g => new
                {
                    Proveedor = new
                    {
                        Id = g.Key.IdProveedor,
                        g.Key.NombreProveedor,
                        g.Key.Contacto,
                        g.Key.Telefono
                    },
                    CantidadHerramientas = g.Count(),
                    Herramientas = g.Select(x => new
                    {
                        IdHerramienta = x.Herramienta.IdHerramienta,
                        Codigo = x.Herramienta.Codigo,
                        NombreHerramienta = x.Herramienta.NombreHerramienta,
                        Familia = x.Herramienta.Familia?.NombreFamilia,
                        FechaMantenimiento = x.Movimiento.Fecha,
                        Observaciones = x.Movimiento.Observaciones
                    }).ToList()
                })
                .OrderByDescending(x => x.CantidadHerramientas)
                .ToList();

            return new
            {
                UsuariosConHerramientas = usuariosConHerramientas,
                ProveedoresConHerramientas = proveedoresConHerramientas,
                Resumen = new
                {
                    TotalUsuariosConPrestamos = usuariosConHerramientas.Count,
                    TotalProveedoresConMantenimiento = proveedoresConHerramientas.Count,
                    TotalHerramientasPrestadas = usuariosConHerramientas.Sum(x => x.CantidadHerramientas),
                    TotalHerramientasEnMantenimiento = proveedoresConHerramientas.Sum(x => x.CantidadHerramientas),
                    UsuarioConMasHerramientas = usuariosConHerramientas.FirstOrDefault(),
                    ProveedorConMasHerramientas = proveedoresConHerramientas.FirstOrDefault()
                }
            };
        }
    }
}
