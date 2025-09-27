using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;

namespace pyreApi.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>
    {
        public UsuarioRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetByDniAsync(string dni)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Dni == dni);
        }

        public async Task<IEnumerable<Usuario>> GetByRolAsync(int rolId)
        {
            return await _dbSet.Where(u => u.RolId == rolId).ToListAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string dni, string passwordHash)
        {
            var user = await GetByDniAsync(dni);
            return user != null && user.PasswordHash == passwordHash;
        }

        public async Task<IEnumerable<Usuario>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.Activo == true).ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetUsersWithSystemAccessAsync()
        {
            return await _dbSet.Where(u => u.AccedeAlSistema == true && u.Activo == true).ToListAsync();
        }
    }
}
