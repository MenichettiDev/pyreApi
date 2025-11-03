using System.Text.Json;
using Microsoft.Extensions.Logging; // <-- agregado
using pyreApi.DTOs.Common;
using pyreApi.DTOs.Herramienta;
using pyreApi.Models;
using pyreApi.Repositories;
using pyreApi.Services;

namespace pyreApi.Services
{
    // Utilidad para comparar dos objetos y obtener solo los campos modificados
    public static class AuditHelper
    {
        public static Dictionary<string, object?> GetChangedFields<T>(T before, T after)
        {
            var result = new Dictionary<string, object?>();
            if (before == null || after == null)
                return result;
            var type = typeof(T);
            foreach (var prop in type.GetProperties())
            {
                // Solo campos simples (evitar navegación y colecciones)
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    continue;
                if (
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)
                    && prop.PropertyType != typeof(string)
                )
                    continue;
                var beforeValue = prop.GetValue(before);
                var afterValue = prop.GetValue(after);
                if (!Equals(beforeValue, afterValue))
                {
                    result[prop.Name] = afterValue;
                }
            }
            return result;
        }

        public static Dictionary<string, object?> GetOriginalFields<T>(T before, T after)
        {
            var result = new Dictionary<string, object?>();
            if (before == null || after == null)
                return result;
            var type = typeof(T);
            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    continue;
                if (
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)
                    && prop.PropertyType != typeof(string)
                )
                    continue;
                var beforeValue = prop.GetValue(before);
                var afterValue = prop.GetValue(after);
                if (!Equals(beforeValue, afterValue))
                {
                    result[prop.Name] = beforeValue;
                }
            }
            return result;
        }
    }

    public class HerramientaService : GenericService<Herramienta>
    {
        private readonly HerramientaRepository _herramientaRepository;
        private readonly AuditorGeneralService _auditorGeneralService;
        private readonly ILogger<HerramientaService> _logger; // <-- agregado

        public HerramientaService(
            HerramientaRepository repository,
            AuditorGeneralService auditorGeneralService,
            ILogger<HerramientaService> logger // <-- agregado
        )
            : base(repository)
        {
            _herramientaRepository = repository;
            _auditorGeneralService = auditorGeneralService;
            _logger = logger; // <-- agregado
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetAllHerramientasAsync()
        {
            try
            {
                var herramientas = await _repository.GetAllAsync();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> GetHerramientaByIdAsync(int id)
        {
            try
            {
                var herramienta = await _repository.GetByIdAsync(id);
                if (herramienta == null)
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada",
                    };
                }

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(herramienta),
                    Message = "Herramienta encontrada",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al buscar la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> CreateHerramientaAsync(
            CreateHerramientaDto createDto
        )
        {
            try
            {
                _logger.LogInformation(
                    "CreateHerramientaAsync - Start. DTO: {dto}",
                    JsonSerializer.Serialize(createDto)
                );

                // Mapear el DTO a la entidad
                var herramienta = MapFromCreateDto(createDto);

                // Guardar la herramienta en la base de datos para obtener el IdHerramienta
                var result = await _repository.AddAsync(herramienta);

                // Generar el código basado en la familia y el IdHerramienta
                result.Codigo = GenerateCodigo(result.IdFamilia, result.IdHerramienta);

                // Actualizar la herramienta con el código generado
                await _repository.UpdateAsync(result);

                _logger.LogInformation(
                    "CreateHerramientaAsync - Saved entity Id={id}, FamiliaId={fam}, Codigo={cod}",
                    result.IdHerramienta,
                    result.IdFamilia,
                    result.Codigo
                );

                // Registrar auditoría de INSERT
                // Serializar todos los campos simples relevantes para INSERT
                var insertData = new
                {
                    result.IdHerramienta,
                    result.NombreHerramienta,
                    result.Codigo,
                    result.CostoDolares,
                    result.IdFamilia,
                    result.IdEstadoFisico,
                    result.IdDisponibilidad,
                    result.IdPlanta,
                    result.Tipo,
                    result.Marca,
                    result.Serie,
                    result.FechaDeIngreso,
                    result.UbicacionFisica,
                    result.Ubicacion,
                    result.Activo,
                    result.DiasAlerta,
                };

                var auditInsertPayload = new AuditorGeneral
                {
                    IdUsuario = 1, // Reemplazar con el ID del usuario actual
                    Entidad = nameof(Herramienta),
                    IdEntidad = result.IdHerramienta,
                    Accion = AccionAuditoria.INSERT,
                    ValorAnterior = null,
                    ValorNuevo = JsonSerializer.Serialize(insertData),
                    Observaciones = "Herramienta creada correctamente",
                };

                _logger.LogDebug(
                    "CreateHerramientaAsync - Enviando auditoría INSERT: {audit}",
                    JsonSerializer.Serialize(auditInsertPayload)
                );

                await _auditorGeneralService.RegisterAuditAsync(auditInsertPayload);

                // Mapear los valores adicionales para la respuesta
                var herramientaDto = MapToDto(result);
                herramientaDto.NombreFamilia = await _herramientaRepository.GetFamiliaNombre(
                    result.IdFamilia
                );
                herramientaDto.EstadoFisico = await _herramientaRepository.GetEstadoFisicoNombre(
                    result.IdEstadoFisico
                );
                herramientaDto.EstadoDisponibilidad =
                    await _herramientaRepository.GetEstadoDisponibilidadNombre(
                        result.IdDisponibilidad
                    );
                herramientaDto.NombrePlanta = await _herramientaRepository.GetPlantaNombre(
                    result.IdPlanta
                );

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = herramientaDto,
                    Message = $"Herramienta creada correctamente con código {result.Codigo}",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CreateHerramientaAsync - Error al crear herramienta. DTO: {dto}",
                    JsonSerializer.Serialize(createDto)
                );
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al crear la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> UpdateHerramientaAsync(
            UpdateHerramientaDto updateDto
        )
        {
            try
            {
                _logger.LogInformation(
                    "UpdateHerramientaAsync - Start. IdHerramienta={id}, DTO={dto}",
                    updateDto.IdHerramienta,
                    JsonSerializer.Serialize(updateDto)
                );

                var herramienta = await _repository.GetByIdAsync(updateDto.IdHerramienta);
                if (herramienta == null)
                {
                    _logger.LogWarning(
                        "UpdateHerramientaAsync - Herramienta no encontrada. Id={id}",
                        updateDto.IdHerramienta
                    );
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada",
                    };
                }

                // Copia del estado original antes de modificar
                var original = new Herramienta
                {
                    IdHerramienta = herramienta.IdHerramienta,
                    NombreHerramienta = herramienta.NombreHerramienta,
                    Codigo = herramienta.Codigo,
                    CostoDolares = herramienta.CostoDolares,
                    IdFamilia = herramienta.IdFamilia,
                    IdEstadoFisico = herramienta.IdEstadoFisico,
                    IdDisponibilidad = herramienta.IdDisponibilidad,
                    IdPlanta = herramienta.IdPlanta,
                    Tipo = herramienta.Tipo,
                    Marca = herramienta.Marca,
                    Serie = herramienta.Serie,
                    FechaDeIngreso = herramienta.FechaDeIngreso,
                    UbicacionFisica = herramienta.UbicacionFisica,
                    Ubicacion = herramienta.Ubicacion,
                    Activo = herramienta.Activo,
                    DiasAlerta = herramienta.DiasAlerta,
                };

                // Loguear solo campos simples para evitar ciclos de serialización
                _logger.LogDebug(
                    "UpdateHerramientaAsync - Original state (shallow): {original}",
                    JsonSerializer.Serialize(ShallowTool(original))
                );

                bool familiaActualizada = false;

                // Map other fields from el DTO primero
                MapFromUpdateDto(updateDto, herramienta);

                // Loguear solo campos simples para evitar ciclos de serialización
                _logger.LogDebug(
                    "UpdateHerramientaAsync - State after mapping DTO (shallow): {after}",
                    JsonSerializer.Serialize(ShallowTool(herramienta))
                );

                // Si la familia cambió, actualiza el código
                if (herramienta.IdFamilia != original.IdFamilia)
                {
                    var oldCodigo = herramienta.Codigo;
                    herramienta.Codigo = GenerateCodigo(
                        herramienta.IdFamilia,
                        herramienta.IdHerramienta
                    );
                    familiaActualizada = true;
                    _logger.LogInformation(
                        "UpdateHerramientaAsync - Familia cambiada. IdHerramienta={id}, OldCodigo={old}, NewCodigo={new}",
                        herramienta.IdHerramienta,
                        oldCodigo,
                        herramienta.Codigo
                    );
                }

                // Obtener los cambios antes de guardar
                var cambiosAntes = AuditHelper.GetOriginalFields(original, herramienta);
                var cambiosDespues = AuditHelper.GetChangedFields(original, herramienta);

                _logger.LogDebug(
                    "UpdateHerramientaAsync - Cambios antes (original values): {cambiosAntes}",
                    JsonSerializer.Serialize(cambiosAntes)
                );
                _logger.LogDebug(
                    "UpdateHerramientaAsync - Cambios despues (new values): {cambiosDespues}",
                    JsonSerializer.Serialize(cambiosDespues)
                );

                // Solo registrar auditoría si hay cambios
                if (cambiosAntes.Count > 0 && cambiosDespues.Count > 0)
                {
                    var auditPayload = new AuditorGeneral
                    {
                        IdUsuario = 1, // Reemplazar con el ID del usuario actual
                        Entidad = nameof(Herramienta),
                        IdEntidad = herramienta.IdHerramienta,
                        Accion = AccionAuditoria.UPDATE,
                        ValorAnterior = JsonSerializer.Serialize(cambiosAntes),
                        ValorNuevo = JsonSerializer.Serialize(cambiosDespues),
                        Observaciones = "Herramienta actualizada correctamente",
                    };

                    // Log adicional: número y nombre de la acción + valores exactos que se enviarán
                    _logger.LogDebug(
                        "UpdateHerramientaAsync - Audit payload details: AccionNumber={num}, AccionString={str}, ValorAnterior={va}, ValorNuevo={vn}",
                        (int)auditPayload.Accion,
                        auditPayload.Accion.ToString(),
                        auditPayload.ValorAnterior,
                        auditPayload.ValorNuevo
                    );

                    _logger.LogInformation(
                        "UpdateHerramientaAsync - Enviando auditoría UPDATE: {audit}",
                        JsonSerializer.Serialize(auditPayload)
                    );

                    await _auditorGeneralService.RegisterAuditAsync(auditPayload);
                }
                else
                {
                    _logger.LogInformation(
                        "UpdateHerramientaAsync - No se registró auditoría porque no se detectaron cambios. IdHerramienta={id}",
                        herramienta.IdHerramienta
                    );
                }

                // Guardar cambios después del registro de auditoría
                await _repository.UpdateAsync(herramienta);

                _logger.LogInformation(
                    "UpdateHerramientaAsync - Actualización persistida. IdHerramienta={id}",
                    herramienta.IdHerramienta
                );

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Message = familiaActualizada
                        ? $"Herramienta actualizada correctamente. Nuevo código: {herramienta.Codigo}"
                        : "Herramienta actualizada correctamente.",
                    Data = MapToDto(herramienta),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateHerramientaAsync - Error al actualizar herramienta. DTO: {dto}",
                    JsonSerializer.Serialize(updateDto)
                );
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetAvailableToolsAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAvailableToolsAsync();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas disponibles obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas disponibles",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetByFamiliaAsync(
            int familiaId
        )
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByFamiliaAsync(familiaId);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por familia obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por familia",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<PagedResponseDto<HerramientaDto>>> GetPagedAsync(
            int page,
            int pageSize,
            string? codigo = null,
            string? nombre = null,
            string? marca = null,
            bool? estado = null
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (pageSize <= 0)
                    pageSize = 10;

                var herramientas = await _herramientaRepository.GetFilteredHerramientasAsync(
                    codigo,
                    nombre,
                    marca,
                    estado
                );

                // Ordenar por IdHerramienta en orden descendente
                var herramientasOrdenadas = herramientas.OrderByDescending(h => h.IdHerramienta);

                var totalRecords = herramientasOrdenadas.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var pagedHerramientas = herramientasOrdenadas
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var herramientaDtos = pagedHerramientas.Select(MapToDto).ToList();

                var pagedResponse = new PagedResponseDto<HerramientaDto>
                {
                    Data = herramientaDtos,
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                };

                return new BaseResponseDto<PagedResponseDto<HerramientaDto>>
                {
                    Success = true,
                    Data = pagedResponse,
                    Message = "Herramientas paginadas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PagedResponseDto<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas paginadas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> UpdateStatusAsync(
            UpdateStatusDto updateStatusDto
        )
        {
            try
            {
                var existingHerramienta = await _repository.GetByIdAsync(
                    updateStatusDto.IdHerramienta
                );
                if (existingHerramienta == null)
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada",
                    };
                }

                existingHerramienta.Activo = updateStatusDto.Activo;
                await _repository.UpdateAsync(existingHerramienta);

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingHerramienta),
                    Message = "Estado de la herramienta actualizado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el estado de la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetByEstadoFisicoAsync(
            int estadoFisicoId
        )
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByEstadoAsync(estadoFisicoId);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por estado físico obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por estado físico",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetInRepairAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetInRepairAsync();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas en reparación obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas en reparación",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                int total = herramientas.Count();

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = total,
                    Message = "Total de herramientas obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasDisponiblesAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                // Ajusta el valor de IdDisponibilidad según tu lógica de disponibilidad
                int totalDisponibles = herramientas.Count(h => h.IdDisponibilidad == 1);

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalDisponibles,
                    Message = "Total de herramientas disponibles obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas disponibles",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasEnPrestamoAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                // Ajusta el valor de IdDisponibilidad según tu lógica de préstamo
                int totalEnPrestamo = herramientas.Count(h => h.IdDisponibilidad == 2);

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalEnPrestamo,
                    Message = "Total de herramientas en préstamo obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas en préstamo",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasEnReparacionAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                // Ajusta el valor de IdDisponibilidad según tu lógica de reparación
                int totalEnReparacion = herramientas.Count(h => h.IdDisponibilidad == 3);

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalEnReparacion,
                    Message = "Total de herramientas en reparación obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas en reparación",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetByDisponibilidadAsync(
            int disponibilidadId
        )
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByDisponibilidadAsync(
                    disponibilidadId
                );
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por estado de disponibilidad obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por estado de disponibilidad",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        //Lo usamos al generar el movimiento de una herramienta para actualizar su disponibilidad
        public async Task<BaseResponseDto<HerramientaDto>> UpdateDisponibilidadAsync(
            int herramientaId,
            int nuevaDisponibilidad
        )
        {
            try
            {
                var existingHerramienta = await _repository.GetByIdAsync(herramientaId);
                if (existingHerramienta == null)
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada",
                    };
                }

                existingHerramienta.IdDisponibilidad = nuevaDisponibilidad;
                await _repository.UpdateAsync(existingHerramienta);

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingHerramienta),
                    Message = "Estado de disponibilidad actualizado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el estado de disponibilidad",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<HerramientaDto>>
        > GetByMultipleDisponibilidadAsync(IEnumerable<int> disponibilidadIds)
        {
            try
            {
                if (disponibilidadIds == null || !disponibilidadIds.Any())
                {
                    return new BaseResponseDto<IEnumerable<HerramientaDto>>
                    {
                        Success = false,
                        Message = "Se requiere al menos un ID de disponibilidad",
                    };
                }

                var herramientas = await _herramientaRepository.GetByMultipleDisponibilidadAsync(
                    disponibilidadIds
                );
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por estados de disponibilidad obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por estados de disponibilidad",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<HerramientaDto>>
        > GetByMultipleDisponibilidadAsync(List<int> disponibilidadIds, string? searchText = null)
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByMultipleDisponibilidadAsync(
                    disponibilidadIds,
                    searchText
                );
                var herramientasDto = herramientas.Select(MapToDto);

                var message = string.IsNullOrWhiteSpace(searchText)
                    ? "Herramientas por disponibilidad obtenidas correctamente"
                    : $"Herramientas filtradas por disponibilidad y búsqueda '{searchText}' obtenidas correctamente";

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientasDto,
                    Message = message,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener herramientas por disponibilidad",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private string GenerateCodigo(int idFamilia, int idHerramienta)
        {
            string idFormatted = idHerramienta.ToString("D3"); // Formatea el ID con tres dígitos
            return idFamilia switch
            {
                1 => $"ELE-{idFormatted}",
                2 => $"MEC-{idFormatted}",
                3 => $"MED-{idFormatted}",
                4 => $"FER-{idFormatted}",
                5 => $"SEG-{idFormatted}",
                6 => $"HID-{idFormatted}",
                7 => $"NEU-{idFormatted}",
                _ => $"UNK-{idFormatted}" // Código por defecto para familias desconocidas
            };
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetHerramientasEnPrestamoByUsuarioAsync(int idUsuarioResponsable)
        {
            try
            {
                var herramientas = await _herramientaRepository.GetHerramientasEnPrestamoByUsuarioAsync(idUsuarioResponsable);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas en préstamo del usuario obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas en préstamo del usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetHerramientasEnReparacionByProveedorAsync(int idProveedor)
        {
            try
            {
                var herramientas = await _herramientaRepository.GetHerramientasEnReparacionByProveedorAsync(idProveedor);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas en reparación del proveedor obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas en reparación del proveedor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private HerramientaDto MapToDto(Herramienta herramienta)
        {
            return new HerramientaDto
            {
                IdHerramienta = herramienta.IdHerramienta,
                Codigo = herramienta.Codigo,
                NombreHerramienta = herramienta.NombreHerramienta,
                IdFamilia = herramienta.IdFamilia,
                Tipo = herramienta.Tipo,
                Marca = herramienta.Marca,
                Serie = herramienta.Serie,
                FechaDeIngreso = herramienta.FechaDeIngreso,
                CostoDolares = herramienta.CostoDolares,
                UbicacionFisica = herramienta.UbicacionFisica,
                IdEstadoFisico = herramienta.IdEstadoFisico,
                IdPlanta = herramienta.IdPlanta,
                Activo = herramienta.Activo,
                IdDisponibilidad = herramienta.IdDisponibilidad,
                NombreFamilia = herramienta.Familia?.NombreFamilia,
                EstadoFisico = herramienta.EstadoFisico?.Descripcion,
                EstadoDisponibilidad = herramienta.EstadoDisponibilidad?.Descripcion,
                NombrePlanta = herramienta.Planta?.NombrePlanta,
            };
        }

        private Herramienta MapFromCreateDto(CreateHerramientaDto createDto)
        {
            return new Herramienta
            {
                NombreHerramienta = createDto.NombreHerramienta,
                IdFamilia = createDto.IdFamilia,
                Tipo = createDto.Tipo,
                Marca = createDto.Marca,
                Serie = createDto.Serie,
                CostoDolares = createDto.CostoDolares,
                UbicacionFisica = createDto.UbicacionFisica,
                IdPlanta = createDto.IdPlanta,
                Activo = createDto.Activo,
                IdDisponibilidad = createDto.IdDisponibilidad,
                DiasAlerta = createDto.DiasAlerta,
                FechaDeIngreso = DateTime.Now, // Asignar la fecha actual
                IdEstadoFisico = 1, // Asignar el estado físico como "Excelente"
            };
        }

        private void MapFromUpdateDto(UpdateHerramientaDto updateDto, Herramienta herramienta)
        {
            herramienta.Codigo = updateDto.Codigo;
            herramienta.NombreHerramienta = updateDto.NombreHerramienta;
            herramienta.IdFamilia = updateDto.IdFamilia;
            herramienta.Tipo = updateDto.Tipo;
            herramienta.Marca = updateDto.Marca;
            herramienta.Serie = updateDto.Serie;
            herramienta.CostoDolares = updateDto.CostoDolares;
            herramienta.UbicacionFisica = updateDto.UbicacionFisica;
            herramienta.IdEstadoFisico = updateDto.IdEstadoFisico;
            herramienta.IdPlanta = updateDto.IdPlanta;
            herramienta.Activo = updateDto.Activo;
            herramienta.IdDisponibilidad = updateDto.IdDisponibilidad;
        }

        // Agregar método helper privado (colócalo dentro de la clase HerramientaService, por ejemplo antes de GenerateCodigo)
        private object ShallowTool(Herramienta? h)
        {
            if (h == null)
                return new { };
            return new
            {
                h.IdHerramienta,
                h.Codigo,
                h.NombreHerramienta,
                h.IdFamilia,
                h.CostoDolares,
                h.IdEstadoFisico,
                h.IdDisponibilidad,
                h.IdPlanta,
                h.Activo,
                h.DiasAlerta,
                h.Tipo,
                h.Marca,
                h.Serie,
            };
        }
    }
}
