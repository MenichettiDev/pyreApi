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

        public async Task<IEnumerable<MovimientoHerramienta>> GetByHerramientaAsync(int herramientaId)
        {
            return await _dbSet
                .Where(m => m.IdHerramienta == herramientaId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Where(m => m.IdUsuario == usuarioId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByTipoMovimientoAsync(int tipoMovimientoId)
        {
            return await _dbSet
                .Where(m => m.IdTipoMovimiento == tipoMovimientoId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetMovimientosByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(m => m.Fecha >= startDate && m.Fecha <= endDate)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<MovimientoHerramienta?> GetLastMovimientoByHerramientaAsync(int herramientaId)
        {
            return await _dbSet
                .Where(m => m.IdHerramienta == herramientaId)
                .OrderByDescending(m => m.Fecha)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MovimientoHerramienta>> GetByObraAsync(int obraId)
        {
            return await _dbSet
                .Where(m => m.IdObra == obraId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }
    }
}
