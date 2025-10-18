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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByHerramientaAsync(int herramientaId)
        {
            return await _dbSet
                .Include(m => m.Herramienta)
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
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
                .Include(m => m.IdUsuarioGenera)
                .Include(m => m.IdUsuarioResponsable)
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Obra)
                .Include(m => m.EstadoDevolucion)
                .Where(m => m.IdProveedor == proveedorId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }
    }
}
