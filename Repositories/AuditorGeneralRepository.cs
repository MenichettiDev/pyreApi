using System.Data.Common; // <-- agregado
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pyreApi.Data; // <-- Ajusta si tu DbContext está en otro namespace
using pyreApi.Models;

namespace pyreApi.Repositories
{
    // Repositorio mínimo para persistir AuditorGeneral con logging detallado.
    public class AuditorGeneralRepository
    {
        private readonly ApplicationDbContext _context; // <- usar el DbContext concreto
        private readonly ILogger<AuditorGeneralRepository> _logger;

        public AuditorGeneralRepository(
            ApplicationDbContext context,
            ILogger<AuditorGeneralRepository> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        // Agrega la auditoría y registra logs antes/después de persistir.
        public async Task AddAsync(AuditorGeneral audit)
        {
            // Preparar payload reducido para logging (evitar ciclos)
            var payload = new
            {
                audit.IdUsuario,
                audit.Entidad,
                audit.IdEntidad,
                AccionNumber = (int)audit.Accion,
                AccionString = audit.Accion.ToString(),
                audit.ValorAnterior,
                audit.ValorNuevo,
                audit.Observaciones,
            };

            _logger.LogInformation(
                "AuditorGeneralRepository.AddAsync - Adding audit. IdEntidad={id}, Accion={accion}",
                audit.IdEntidad,
                payload.AccionString
            );
            _logger.LogDebug(
                "AuditorGeneralRepository.AddAsync - Audit payload (reduced): {payload}",
                JsonSerializer.Serialize(payload)
            );

            try
            {
                // Añadir a contexto
                _context.Set<AuditorGeneral>().Add(audit);

                _logger.LogDebug("AuditorGeneralRepository.AddAsync - Calling SaveChangesAsync...");
                await _context.SaveChangesAsync();

                // Log del resultado después de guardar (IdAuditoria y Accion tal como quedó en la entidad)
                _logger.LogInformation(
                    "AuditorGeneralRepository.AddAsync - Saved audit. IdAuditoria={auditId}, IdEntidad={id}, AccionNumber={num}, AccionString={str}",
                    audit.IdAuditoria,
                    audit.IdEntidad,
                    (int)audit.Accion,
                    audit.Accion.ToString()
                );

                _logger.LogDebug(
                    "AuditorGeneralRepository.AddAsync - Saved audit details: {detail}",
                    JsonSerializer.Serialize(
                        new
                        {
                            audit.IdAuditoria,
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

                // --- Nuevo: forzar la columna 'Accion' como string para evitar la conversión ambigua de MySQL ENUM ---
                try
                {
                    var conn = _context.Database.GetDbConnection();
                    bool openedHere = false;
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                        openedHere = true;
                    }

                    try
                    {
                        // Forzar escritura del texto de la acción (por ejemplo 'UPDATE') para evitar que MySQL use el índice del ENUM.
                        using var cmdUpdate = conn.CreateCommand();
                        cmdUpdate.CommandText =
                            "UPDATE AuditorGeneral SET Accion = @accion WHERE IdAuditoria = @id";
                        var pAcc = cmdUpdate.CreateParameter();
                        pAcc.ParameterName = "@accion";
                        pAcc.Value = audit.Accion.ToString();
                        cmdUpdate.Parameters.Add(pAcc);
                        var pId = cmdUpdate.CreateParameter();
                        pId.ParameterName = "@id";
                        pId.Value = audit.IdAuditoria;
                        cmdUpdate.Parameters.Add(pId);

                        var rows = await cmdUpdate.ExecuteNonQueryAsync();
                        _logger.LogInformation(
                            "AuditorGeneralRepository.AddAsync - Forced DB Accion update for IdAuditoria={id}. RowsAffected={rows}",
                            audit.IdAuditoria,
                            rows
                        );

                        // Leer la columna Accion para verificar qué quedó realmente
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText =
                            "SELECT Accion FROM AuditorGeneral WHERE IdAuditoria = @id";
                        var param = cmd.CreateParameter();
                        param.ParameterName = "@id";
                        param.Value = audit.IdAuditoria;
                        cmd.Parameters.Add(param);

                        var result = await cmd.ExecuteScalarAsync();
                        var accionPersistida = result?.ToString();

                        _logger.LogInformation(
                            "AuditorGeneralRepository.AddAsync - DB reported Accion column for IdAuditoria={id}: {accionPersistida}",
                            audit.IdAuditoria,
                            accionPersistida
                        );
                    }
                    finally
                    {
                        if (openedHere && conn.State == System.Data.ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
                catch (System.Exception exInner)
                {
                    _logger.LogError(
                        exInner,
                        "AuditorGeneralRepository.AddAsync - Error al forzar/leer la columna Accion directamente desde la BD para IdAuditoria={id}",
                        audit.IdAuditoria
                    );
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AuditorGeneralRepository.AddAsync - Error saving audit. Payload summary: {payload}",
                    JsonSerializer.Serialize(payload)
                );
                throw;
            }
        }
    }
}
