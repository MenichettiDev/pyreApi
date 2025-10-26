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

        public async Task<IEnumerable<Herramienta>> GetByMultipleDisponibilidadAsync(IEnumerable<int> disponibilidadIds)
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
            bool? estado)
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
                query = query.Where(h => h.NombreHerramienta != null && h.NombreHerramienta.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(marca))
                query = query.Where(h => h.Marca != null && h.Marca.Contains(marca));

            if (estado.HasValue)
                query = query.Where(h => h.Activo == estado.Value);

            return await query.OrderBy(h => h.IdHerramienta).ToListAsync();
        }

        public async Task<IEnumerable<Herramienta>> GetByMultipleDisponibilidadAsync(List<int> disponibilidadIds, string? searchText = null)
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
                    (h.Marca != null && h.Marca.Contains(searchText)));
            }

            return await query
                .OrderBy(h => h.NombreHerramienta)
                .Take(15) // Límite de 15 herramientas
                .ToListAsync();
        }
    }
}