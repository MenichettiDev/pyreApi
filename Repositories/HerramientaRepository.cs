using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;

namespace pyreApi.Repositories
{
    public class HerramientaRepository : GenericRepository<Herramienta>
    {
        public HerramientaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Herramienta>> GetByEstadoAsync(int estadoId)
        {
            return await _dbSet.Where(h => h.IdEstadoActual == estadoId).ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByFamiliaAsync(int familiaId)
        {
            return await _dbSet.Where(h => h.IdFamilia == familiaId).ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByPlantaAsync(int plantaId)
        {
            return await _dbSet.Where(h => h.IdPlanta == plantaId).ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetAvailableToolsAsync()
        {
            return await _dbSet
                .Include(h => h.EstadoActual)
                .Where(h => h.EstadoActual.Descripcion.ToLower() == "disponible" && h.Activo == true)
                .ToListAsync();
        }

        public async Task<Herramienta?> GetByCodigoAsync(string codigo)
        {
            return await _dbSet.FirstOrDefaultAsync(h => h.Codigo == codigo);
        }

        public async Task<IEnumerable<Herramienta>> GetInRepairAsync()
        {
            return await _dbSet.Where(h => h.EnReparacion == true).ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetActiveToolsAsync()
        {
            return await _dbSet.Where(h => h.Activo == true).ToListAsync();
        }
    }
}
