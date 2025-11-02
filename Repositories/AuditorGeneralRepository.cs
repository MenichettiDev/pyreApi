using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;

namespace pyreApi.Repositories
{
    public class AuditorGeneralRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditorGeneralRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditorGeneral auditoria)
        {
            _context.AuditorGeneral.Add(auditoria);
            await _context.SaveChangesAsync();
        }
    }
}
