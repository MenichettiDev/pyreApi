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
            return await _dbSet
                .Include(h => h.Familia)
                .Include(h => h.EstadoFisico)
                .Include(h => h.EstadoDisponibilidad)
                .Include(h => h.Planta)
                .Where(h => h.IdEstadoFisico == estadoId)
                .ToListAsync();
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
                .Where(h => h.EstadoDisponibilidad.Descripcion.ToLower().Contains("disponible") && h.Activo == true)
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
    }
}