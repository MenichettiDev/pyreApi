using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    // Servicio para registrar auditorías con logging detallado
    public class AuditorGeneralService
    {
        private readonly AuditorGeneralRepository _repository;
        private readonly ILogger<AuditorGeneralService> _logger;

        public AuditorGeneralService(
            AuditorGeneralRepository repository,
            ILogger<AuditorGeneralService> logger
        )
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task RegisterAuditAsync(AuditorGeneral audit)
        {
            // Log básico (sin serializar navegación compleja)
            _logger.LogInformation(
                "AuditorGeneralService.RegisterAuditAsync - Received audit payload: IdEntidad={id}, AccionNumber={num}, AccionString={str}",
                audit.IdEntidad,
                (int)audit.Accion,
                audit.Accion.ToString()
            );

            // Log de valores concretos enviados
            _logger.LogDebug(
                "AuditorGeneralService.RegisterAuditAsync - ValorAnterior (raw): {va}",
                audit.ValorAnterior
            );
            _logger.LogDebug(
                "AuditorGeneralService.RegisterAuditAsync - ValorNuevo (raw): {vn}",
                audit.ValorNuevo
            );

            try
            {
                _logger.LogDebug(
                    "AuditorGeneralService.RegisterAuditAsync - Preparing to persist audit (no navigation properties logged)."
                );

                // Persistir usando el repositorio — método devuelve Task (sin resultado)
                await _repository.AddAsync(audit);

                // Log resultado guardado usando el objeto 'audit' (el repo puede haber rellenado IdAuditoria/FechaHora)
                _logger.LogInformation(
                    "AuditorGeneralService.RegisterAuditAsync - Audit saved. IdAuditoria={auditId}, IdEntidad={id}, Accion={accion}",
                    audit.IdAuditoria,
                    audit.IdEntidad,
                    audit.Accion.ToString()
                );

                // Log detalle reducido del objeto guardado para corroborar lo persistido
                _logger.LogDebug(
                    "AuditorGeneralService.RegisterAuditAsync - Saved audit details: {detail}",
                    JsonSerializer.Serialize(
                        new
                        {
                            IdAuditoria = audit.IdAuditoria,
                            FechaHora = audit.FechaHora,
                            audit.IdUsuario,
                            audit.Entidad,
                            audit.IdEntidad,
                            AccionNumber = (int)audit.Accion,
                            AccionString = audit.Accion.ToString(),
                            audit.ValorAnterior,
                            audit.ValorNuevo,
                            audit.Observaciones,
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AuditorGeneralService.RegisterAuditAsync - Error al guardar auditoría. Payload summary: {payload}",
                    JsonSerializer.Serialize(
                        new
                        {
                            audit.IdEntidad,
                            Accion = (int)audit.Accion,
                            audit.ValorAnterior,
                            audit.ValorNuevo,
                        }
                    )
                );
                throw;
            }
        }
    }
}
