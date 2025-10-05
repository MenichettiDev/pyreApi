using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;

namespace pyreApi.Repositories
{
    public class AlertaRepository : GenericRepository<Alerta>
    {
        public AlertaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Alerta>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.Herramienta)
                .Include(a => a.TipoAlerta)
                .ToListAsync();
        }

        public override async Task<Alerta?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Herramienta)
                .Include(a => a.TipoAlerta)
                .FirstOrDefaultAsync(a => a.IdAlerta == id);
        }

        public async Task<IEnumerable<Alerta>> GetByHerramientaAsync(int idHerramienta)
        {
            return await _dbSet
                .Include(a => a.Herramienta)
                .Include(a => a.TipoAlerta)
                .Where(a => a.IdHerramienta == idHerramienta)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alerta>> GetUnreadAsync()
        {
            return await _dbSet
                .Include(a => a.Herramienta)
                .Include(a => a.TipoAlerta)
                .Where(a => !a.Leida)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alerta>> GetByTipoAlertaAsync(int idTipoAlerta)
        {
            return await _dbSet
                .Include(a => a.Herramienta)
                .Include(a => a.TipoAlerta)
                .Where(a => a.IdTipoAlerta == idTipoAlerta)
                .ToListAsync();
        }
    }
}
