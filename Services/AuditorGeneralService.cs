using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class AuditorGeneralService
    {
        private readonly AuditorGeneralRepository _auditorGeneralRepository;

        public AuditorGeneralService(AuditorGeneralRepository auditorGeneralRepository)
        {
            _auditorGeneralRepository = auditorGeneralRepository;
        }

        public async Task RegisterAuditAsync(AuditorGeneral auditoria)
        {
            await _auditorGeneralRepository.AddAsync(auditoria);
        }
    }
}
