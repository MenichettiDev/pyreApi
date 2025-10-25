using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;

namespace pyreApi.Repositories
{
    public class MovimientoHerramientaRepository : GenericRepository<MovimientoHerramienta>
    {
        public MovimientoHerramientaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<MovimientoHerramienta>> GetAllAsync()
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public override async Task<MovimientoHerramienta?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByHerramientaAsync(int herramientaId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdHerramienta == herramientaId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByUsuarioResponsableAsync(int usuarioId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdUsuarioResponsable == usuarioId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByTipoMovimientoAsync(int tipoMovimientoId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdTipoMovimiento == tipoMovimientoId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetMovimientosByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.Fecha >= startDate && m.Fecha <= endDate)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<MovimientoHerramienta?> GetLastMovimientoByHerramientaAsync(int herramientaId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdHerramienta == herramientaId)
                .OrderByDescending(m => m.Fecha)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByObraAsync(int obraId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdObra == obraId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByProveedorAsync(int proveedorId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdProveedor == proveedorId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetFilteredMovimientosAsync(
            string? nombreHerramienta,
            int? idFamiliaHerramienta,
            int? idUsuarioGenera,
            int? idUsuarioResponsable,
            int? idTipoMovimiento,
            int? idObra,
            int? idProveedor,
            int? idEstadoFisico,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var query = _dbSet
                .Include(m => m.Herramienta)
                    .ThenInclude(h => h.Familia)
                .Include(m => m.Herramienta)
                    .ThenInclude(h => h.EstadoFisico)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Include(m => m.Proveedor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombreHerramienta))
                query = query.Where(m => m.Herramienta.NombreHerramienta != null &&
                    m.Herramienta.NombreHerramienta.Contains(nombreHerramienta));

            if (idFamiliaHerramienta.HasValue)
                query = query.Where(m => m.Herramienta.IdFamilia == idFamiliaHerramienta.Value);

            if (idUsuarioGenera.HasValue)
                query = query.Where(m => m.IdUsuarioGenera == idUsuarioGenera.Value);

            if (idUsuarioResponsable.HasValue)
                query = query.Where(m => m.IdUsuarioResponsable == idUsuarioResponsable.Value);

            if (idTipoMovimiento.HasValue)
                query = query.Where(m => m.IdTipoMovimiento == idTipoMovimiento.Value);

            if (idObra.HasValue)
                query = query.Where(m => m.IdObra == idObra.Value);

            if (idProveedor.HasValue)
                query = query.Where(m => m.IdProveedor == idProveedor.Value);

            if (idEstadoFisico.HasValue)
                query = query.Where(m => m.Herramienta.IdEstadoFisico == idEstadoFisico.Value);

            if (fechaDesde.HasValue)
                query = query.Where(m => m.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(m => m.Fecha <= fechaHasta.Value);

            return await query.OrderByDescending(m => m.Fecha).ToListAsync();
        }

        public async Task<MovimientoHerramienta?> GetLatestMovimientoByHerramientaAsync(int herramientaId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdHerramienta == herramientaId)
                .OrderByDescending(m => m.Fecha)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetLatest5BorrowedToolsAsync()
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.UsuarioGenera)
                .Include(m => m.UsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdTipoMovimiento == 1) // Solo préstamos
                .OrderByDescending(m => m.Fecha)
                .Take(5)
                .ToListAsync();
        }
    }
}
