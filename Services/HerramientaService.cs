using System.Reflection;
using System.Security.Claims; // <-- agregado para ClaimTypes
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http; // <-- agregado
using Microsoft.EntityFrameworkCore.Storage; // <-- agregado para transacciones
using Microsoft.Extensions.DependencyInjection; // <-- agregado
using Microsoft.Extensions.Logging; // <-- agregado
using pyreApi.Data; // <-- agregado para acceso al contexto
using pyreApi.DTOs.Common;
using pyreApi.DTOs.Herramienta;
using pyreApi.Extensions; // <-- agregado para ClaimsPrincipalExtensions
using pyreApi.Models;
using pyreApi.Repositories;
using pyreApi.Services;

namespace pyreApi.Services
{
    // Utilidad para comparar dos objetos y obtener solo los campos modificados

    public class HerramientaService : GenericService<Herramienta>
    {
        private readonly HerramientaRepository _herramientaRepository;
        private readonly ILogger<HerramientaService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context; // <-- agregado

        public HerramientaService(
            HerramientaRepository repository,
            ILogger<HerramientaService> logger,
            IServiceProvider serviceProvider,
            ApplicationDbContext context // <-- agregado
        )
            : base(repository)
        {
            _herramientaRepository = repository;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _context = context; // <-- agregado
        }

        // Nota: los métodos de "lista" devuelven herramientas en cualquier estado de disponibilidad (1..5)
        // pero siempre excluyen herramientas inactivas (Activo == false). Los endpoints
        // específicos como "available" siguen devolviendo solo disponibilidad==1 además de Activo==true.

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetAllHerramientasAsync()
        {
            try
            {
                var herramientas = (await _repository.GetAllAsync())
                    .Where(h => h.Activo && !h.Eliminado)
                    .ToList();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message =
                        "Herramientas obtenidas correctamente (todas las disponibilidades; excluye inactivas)",
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
                if (herramienta == null || !herramienta.Activo)
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
            using var transaction = await _context.Database.BeginTransactionAsync();
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

                // Confirmar transacción
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "CreateHerramientaAsync - Transaction committed successfully. IdHerramienta={id}",
                    result.IdHerramienta
                );

                // Mapear los valores adicionales para la respuesta (fuera de la transacción)
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
                await transaction.RollbackAsync();
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
            using var transaction = await _context.Database.BeginTransactionAsync();
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
                    await transaction.RollbackAsync();
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

                // Nuevo: bloquear cualquier actualización si la herramienta está en estado 'Bloqueada' (5)
                // pero permitirla si el usuario actual es SuperAdmin o Administrador
                if (herramienta.IdDisponibilidad == 5 && !IsCurrentUserSuperAdminOrAdmin())
                {
                    await transaction.RollbackAsync();
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message =
                            "La herramienta está bloqueada. Para editarla, primero debe desbloquearla y luego realizar los cambios.",
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

                _logger.LogDebug(
                    "UpdateHerramientaAsync - Original state (shallow): {original}",
                    JsonSerializer.Serialize(ShallowTool(original))
                );

                bool familiaActualizada = false;

                // Map other fields from el DTO primero
                MapFromUpdateDto(updateDto, herramienta);

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

                // Guardar cambios después del registro de auditoría
                await _repository.UpdateAsync(herramienta);

                // Confirmar transacción
                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();
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
                var herramientas = (await _herramientaRepository.GetAvailableToolsAsync())
                    .Where(h => h.Activo && !h.Eliminado)
                    .ToList();
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
                var herramientas = (await _herramientaRepository.GetByFamiliaAsync(familiaId))
                    .Where(h => h.Activo)
                    .ToList();
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
            bool? estado = null,
            int? idDisponibilidad = null
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (pageSize <= 0)
                    pageSize = 10;

                var herramientas = (
                    await _herramientaRepository.GetFilteredHerramientasAsync(
                        codigo,
                        nombre,
                        marca,
                        estado,
                        idDisponibilidad
                    )
                )
                    .Where(h => h.Activo)
                    .Where(h => h.Eliminado == false);

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
                    Message =
                        "Herramientas paginadas obtenidas correctamente (todas las disponibilidades; excluye inactivas)",
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

                // Nuevo: impedir cambio de estado si está bloqueada, salvo SuperAdmin o Administrador
                if (existingHerramienta.IdDisponibilidad == 5 && !IsCurrentUserSuperAdminOrAdmin())
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message =
                            "La herramienta está bloqueada. Para cambiar su estado, primero debe desbloquearla.",
                    };
                }

                existingHerramienta.Activo = updateStatusDto.Activo;
                await _repository.UpdateAsync(existingHerramienta);

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingHerramienta),
                    Message = "Herramietna eliminada correctamente",
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
                var herramientas = (await _herramientaRepository.GetByEstadoAsync(estadoFisicoId))
                    .Where(h => h.Activo && h.Eliminado == false)
                    .ToList();
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
                var herramientas = (await _herramientaRepository.GetInRepairAsync())
                    .Where(h => h.Activo && !h.Eliminado)
                    .ToList();
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
                int total = herramientas.Count(h => h.Activo && !h.Eliminado); // SOLO activas y no eliminadas

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

        public async Task<BaseResponseDto<int>> GetTotalHerramientasByEstadoFisicoAsync(
            int estadoFisicoId
        )
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                int totalPorEstado = herramientas.Count(h =>
                    h.IdEstadoFisico == estadoFisicoId && h.Activo && !h.Eliminado
                );

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalPorEstado,
                    Message = "Total de herramientas por estado físico obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas por estado físico",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasByDisponibilidadAsync(
            int disponibilidadId
        )
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                int totalPorDisponibilidad = herramientas.Count(h =>
                    h.IdDisponibilidad == disponibilidadId && h.Activo && !h.Eliminado
                );

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalPorDisponibilidad,
                    Message = "Total de herramientas por disponibilidad obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas por disponibilidad",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasDisponiblesAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                int totalDisponibles = herramientas.Count(h =>
                    h.IdDisponibilidad == 1 && h.Activo && !h.Eliminado
                );

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
                int totalEnPrestamo = herramientas.Count(h => h.IdDisponibilidad == 2 && h.Activo);

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
                int totalEnReparacion = herramientas.Count(h =>
                    h.IdDisponibilidad == 3 && h.Activo
                );

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
                var herramientas = (
                    await _herramientaRepository.GetByDisponibilidadAsync(disponibilidadId)
                )
                    .Where(h => h.Activo)
                    .Where(h => h.Eliminado == false)
                    .ToList();
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

                // Nuevo: impedir cualquier cambio de disponibilidad si la herramienta está bloqueada, salvo SuperAdmin o Administrador
                if (existingHerramienta.IdDisponibilidad == 5 && !IsCurrentUserSuperAdminOrAdmin())
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message =
                            "La herramienta está bloqueada. Para cambiar su disponibilidad, primero debe desbloquearla.",
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

                var herramientas = (
                    await _herramientaRepository.GetByMultipleDisponibilidadAsync(disponibilidadIds)
                )
                    .Where(h => h.Activo && !h.Eliminado)
                    .ToList();
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
                var herramientas = (
                    await _herramientaRepository.GetByMultipleDisponibilidadAsync(
                        disponibilidadIds,
                        searchText
                    )
                )
                    .Where(h => h.Activo)
                    .Where(h => h.Eliminado == false)
                    .ToList();
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
                    Errors = new List<string> { ex.Message },
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

        public async Task<
            BaseResponseDto<IEnumerable<HerramientaDto>>
        > GetHerramientasEnPrestamoByUsuarioAsync(int idUsuarioResponsable)
        {
            try
            {
                var herramientas = (
                    await _herramientaRepository.GetHerramientasEnPrestamoByUsuarioAsync(
                        idUsuarioResponsable
                    )
                )
                    .Where(h => h.Activo)
                    .ToList();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas en préstamo del usuario obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas en préstamo del usuario",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<HerramientaDto>>
        > GetHerramientasEnReparacionByProveedorAsync(int idProveedor)
        {
            try
            {
                var herramientas = (
                    await _herramientaRepository.GetHerramientasEnReparacionByProveedorAsync(
                        idProveedor
                    )
                )
                    .Where(h => h.Activo)
                    .ToList();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas en reparación del proveedor obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas en reparación del proveedor",
                    Errors = new List<string> { ex.Message },
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

        public async Task<BaseResponseDto<object>> ToggleBloqueoAsync(int id)
        {
            try
            {
                // Buscar la herramienta por ID
                var herramienta = await _herramientaRepository.GetByIdAsync(id);

                if (herramienta == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "La herramienta no existe",
                    };
                }

                // Verificar el estado actual de disponibilidad
                var estadoActual = herramienta.IdDisponibilidad;
                string accion;
                string estadoAnterior;
                string estadoNuevo;

                switch (estadoActual)
                {
                    case 1: // Disponible - cambiar a Bloqueada
                        herramienta.IdDisponibilidad = 5;
                        accion = "bloqueada";
                        estadoAnterior = "Disponible";
                        estadoNuevo = "Bloqueada";
                        break;
                    case 5: // Bloqueada - cambiar a Disponible
                        herramienta.IdDisponibilidad = 1;
                        accion = "desbloqueada";
                        estadoAnterior = "Bloqueada";
                        estadoNuevo = "Disponible";
                        break;
                    case 2:
                        return new BaseResponseDto<object>
                        {
                            Success = false,
                            Message =
                                "No se puede cambiar el estado porque la herramienta está Prestada",
                        };
                    case 3:
                        return new BaseResponseDto<object>
                        {
                            Success = false,
                            Message =
                                "No se puede cambiar el estado porque la herramienta está en Mantenimiento",
                        };
                    case 4:
                        return new BaseResponseDto<object>
                        {
                            Success = false,
                            Message =
                                "No se puede cambiar el estado porque la herramienta está Extraviada",
                        };
                    default:
                        return new BaseResponseDto<object>
                        {
                            Success = false,
                            Message =
                                "No se puede cambiar el estado de la herramienta debido a su estado actual",
                        };
                }

                // Guardar cambios
                await _repository.UpdateAsync(herramienta);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = $"Herramienta {accion} exitosamente",
                    Data = new
                    {
                        idHerramienta = id,
                        estadoAnterior = estadoAnterior,
                        estadoActual = estadoNuevo,
                    },
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al cambiar el estado de la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        // helper para saber si el usuario actual es SuperAdmin o Administrador
        private bool IsCurrentUserSuperAdminOrAdmin()
        {
            try
            {
                // Intentamos resolver IHttpContextAccessor de manera opcional.
                var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
                var user = httpContextAccessor?.HttpContext?.User;
                if (user == null)
                {
                    _logger.LogWarning("IsCurrentUserSuperAdminOrAdmin - HttpContext.User es null");
                    return false;
                }

                // Obtener el rol usando la extensión personalizada
                var userRole = user.GetUserRole();
                _logger.LogInformation(
                    "IsCurrentUserSuperAdminOrAdmin - Rol del usuario: {userRole}",
                    userRole ?? "null"
                );

                // Verificar usando la extensión personalizada primero
                if (userRole == "SuperAdmin" || userRole == "Administrador")
                {
                    _logger.LogInformation(
                        "IsCurrentUserSuperAdminOrAdmin - Usuario autorizado con rol: {userRole}",
                        userRole
                    );
                    return true;
                }

                // Verificar también con los métodos estándar como fallback
                bool isSuperAdmin = user.IsInRole("SuperAdmin");
                bool isAdministrador = user.IsInRole("Administrador");

                _logger.LogInformation(
                    "IsCurrentUserSuperAdminOrAdmin - IsInRole SuperAdmin: {isSuperAdmin}, Administrador: {isAdministrador}",
                    isSuperAdmin,
                    isAdministrador
                );

                if (isSuperAdmin || isAdministrador)
                    return true;

                // Verificar claims directamente
                var claims = user.Claims.ToList();
                _logger.LogInformation(
                    "IsCurrentUserSuperAdminOrAdmin - Total claims: {count}",
                    claims.Count
                );

                foreach (var claim in claims)
                {
                    _logger.LogInformation(
                        "IsCurrentUserSuperAdminOrAdmin - Claim Type: {type}, Value: {value}",
                        claim.Type,
                        claim.Value
                    );

                    if (
                        (
                            claim.Type == "role"
                            || claim.Type.EndsWith("/role")
                            || claim.Type == ClaimTypes.Role
                        ) && (claim.Value == "SuperAdmin" || claim.Value == "Administrador")
                    )
                    {
                        _logger.LogInformation(
                            "IsCurrentUserSuperAdminOrAdmin - Usuario autorizado por claim directo: {value}",
                            claim.Value
                        );
                        return true;
                    }
                }

                _logger.LogWarning("IsCurrentUserSuperAdminOrAdmin - Usuario NO autorizado");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "IsCurrentUserSuperAdminOrAdmin - Error al verificar permisos"
                );
                return false;
            }
        }

        // helper para saber si el usuario actual es SuperAdmin (mantener para compatibilidad si se usa en otros lugares)
        private bool IsCurrentUserSuperAdmin()
        {
            try
            {
                // Intentamos resolver IHttpContextAccessor de manera opcional.
                var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
                var user = httpContextAccessor?.HttpContext?.User;
                if (user == null)
                    return false;
                // Comprueba rol por IsInRole y por claim "role" en caso de que se use claim directo
                return user.IsInRole("SuperAdmin")
                    || user.Claims.Any(c =>
                        (c.Type == "role" || c.Type.EndsWith("/role")) && c.Value == "SuperAdmin"
                    )
                    || user.Claims.Any(c => c.Type == "roles" && c.Value == "SuperAdmin");
            }
            catch
            {
                return false;
            }
        }

        public async Task<BaseResponseDto<byte[]>> ReporteHerramientasAsync(
            bool includeValorizado = true
        )
        {
            try
            {
                var herramientas = (await _repository.GetAllAsync())
                    .Where(h => !h.Eliminado)
                    .ToList();

                // mapeos fijos solicitados
                var disponibilidadMap = new Dictionary<int, string>
                {
                    { 1, "Disponible" },
                    { 2, "Prestada" },
                    { 3, "Mantenimiento" },
                    { 4, "Extraviada" },
                    { 5, "Bloqueada" },
                };

                var estadoFisicoMap = new Dictionary<int, string>
                {
                    { 4, "Dañada" },
                    { 3, "Desgastada" },
                    { 1, "Excelente" },
                    { 5, "No Apta" },
                    { 2, "Usada" },
                };

                var familiaMap = new Dictionary<int, string>
                {
                    { 1, "Eléctrica" },
                    { 4, "Ferretería" },
                    { 6, "Hidráulica" },
                    { 2, "Mecánica" },
                    { 3, "Medición" },
                    { 7, "Neumática" },
                    { 5, "Seguridad" },
                };

                // reflection helpers para extraer propiedades comunes (si existen)
                decimal totalCosto = 0m;
                string GetStringProp(object obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        var p = obj.GetType()
                            .GetProperty(
                                n,
                                BindingFlags.Public
                                    | BindingFlags.Instance
                                    | BindingFlags.IgnoreCase
                            );
                        if (p != null)
                        {
                            var v = p.GetValue(obj);
                            if (v != null)
                                return v.ToString()!;
                        }
                    }
                    return string.Empty;
                }

                int? GetIntProp(object obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        var p = obj.GetType()
                            .GetProperty(
                                n,
                                BindingFlags.Public
                                    | BindingFlags.Instance
                                    | BindingFlags.IgnoreCase
                            );
                        if (p != null)
                        {
                            var v = p.GetValue(obj);
                            if (v == null)
                                continue;
                            if (v is int i)
                                return i;
                            if (int.TryParse(v.ToString(), out var parsed))
                                return parsed;
                        }
                    }
                    return null;
                }

                decimal? GetDecimalProp(object obj, params string[] names)
                {
                    foreach (var n in names)
                    {
                        var p = obj.GetType()
                            .GetProperty(
                                n,
                                BindingFlags.Public
                                    | BindingFlags.Instance
                                    | BindingFlags.IgnoreCase
                            );
                        if (p != null)
                        {
                            var v = p.GetValue(obj);
                            if (v == null)
                                continue;
                            if (v is decimal d)
                                return d;
                            if (v is double db)
                                return (decimal)db;
                            if (v is float f)
                                return (decimal)f;
                            if (decimal.TryParse(v.ToString(), out var parsed))
                                return parsed;
                        }
                    }
                    return null;
                }

                // calculos
                var disponiblesCount = 0;
                var prestadasCount = 0;
                var reparacionCount = 0;
                var activasCount = 0;

                var estadoFisicoCounts = new Dictionary<string, int>();
                foreach (var kv in estadoFisicoMap)
                    estadoFisicoCounts[kv.Value] = 0;

                foreach (var h in herramientas)
                {
                    var dispId = GetIntProp(h, "IdDisponibilidad");
                    var estadoId = GetIntProp(h, "IdEstadoFisico");
                    var activoVal =
                        GetIntProp(h, "Activo")
                        ?? (h.GetType().GetProperty("Activo")?.GetValue(h) is bool b && b ? 1 : 0);

                    if (includeValorizado)
                    {
                        var costo = GetDecimalProp(h, "CostoDolares");
                        if (costo.HasValue)
                            totalCosto += costo.Value;
                    }

                    if (dispId == 1)
                        disponiblesCount++;
                    if (dispId == 2)
                        prestadasCount++;
                    if (dispId == 3)
                        reparacionCount++;
                    if (activoVal == 1)
                        activasCount++;

                    if (
                        estadoId.HasValue
                        && estadoFisicoMap.TryGetValue(estadoId.Value, out var nombreEstado)
                    )
                    {
                        estadoFisicoCounts[nombreEstado] =
                            estadoFisicoCounts.GetValueOrDefault(nombreEstado) + 1;
                    }
                }

                // generar excel con ClosedXML
                using var wb = new XLWorkbook();
                var wsList = wb.Worksheets.Add("Herramientas");

                // encabezados: intento mapear propiedades comunes
                var headers = new List<string>
                {
                    "Id",
                    "Nombre",
                    "Codigo",
                    "Familia",
                    "Disponibilidad",
                    "EstadoFisico",
                    "Costo",
                    "Activo",
                };
                for (int c = 0; c < headers.Count; c++)
                    wsList.Cell(1, c + 1).Value = headers[c];

                var row = 2;
                foreach (var h in herramientas)
                {
                    var id = GetIntProp(h, "IdHerramienta")?.ToString();
                    var nombre = GetStringProp(h, "NombreHerramienta", "Descripcion");
                    var codigo = GetStringProp(h, "Codigo");
                    var familiaId = GetIntProp(h, "IdFamilia");
                    var familiaNombre =
                        familiaId.HasValue && familiaMap.TryGetValue(familiaId.Value, out var fName)
                            ? fName
                            : (familiaId?.ToString() ?? string.Empty);
                    var dispId = GetIntProp(h, "IdDisponibilidad");
                    var dispNombre =
                        dispId.HasValue
                        && disponibilidadMap.TryGetValue(dispId.Value, out var dName)
                            ? dName
                            : (dispId?.ToString() ?? string.Empty);
                    var estadoId = GetIntProp(h, "IdEstadoFisico");
                    var estadoNombre =
                        estadoId.HasValue
                        && estadoFisicoMap.TryGetValue(estadoId.Value, out var eName)
                            ? eName
                            : (estadoId?.ToString() ?? string.Empty);
                    string costo = string.Empty;
                    if (includeValorizado)
                    {
                        costo = GetDecimalProp(h, "CostoDolares")?.ToString("F2") ?? "";
                    }
                    var activo = GetStringProp(h, "Activo");

                    wsList.Cell(row, 1).Value = id;
                    wsList.Cell(row, 2).Value = nombre;
                    wsList.Cell(row, 3).Value = codigo;
                    wsList.Cell(row, 4).Value = familiaNombre;
                    wsList.Cell(row, 5).Value = dispNombre;
                    wsList.Cell(row, 6).Value = estadoNombre;
                    wsList.Cell(row, 7).Value = costo;
                    wsList.Cell(row, 8).Value = activo;
                    row++;
                }

                wsList.Columns().AdjustToContents();

                // hoja resumen
                var ws = wb.Worksheets.Add("Resumen");
                int r = 1;
                ws.Cell(r++, 1).Value = "Reporte de Herramientas";
                ws.Cell(r++, 1).Value = $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (UTC)";
                r++;

                if (includeValorizado)
                {
                    ws.Cell(r, 1).Value = "Total Costo";
                    ws.Cell(r, 2).Value = totalCosto;
                    r++;
                }

                ws.Cell(r, 1).Value = "Cantidad Disponibles";
                ws.Cell(r, 2).Value = disponiblesCount;
                r++;

                ws.Cell(r, 1).Value = "Cantidad Prestadas";
                ws.Cell(r, 2).Value = prestadasCount;
                r++;

                ws.Cell(r, 1).Value = "Cantidad en Mantenimiento";
                ws.Cell(r, 2).Value = reparacionCount;
                r++;

                ws.Cell(r, 1).Value = "Cantidad Activas";
                ws.Cell(r, 2).Value = activasCount;
                r += 2;

                ws.Cell(r++, 1).Value = "Cantidades por Estado Físico";
                foreach (var kv in estadoFisicoCounts)
                {
                    ws.Cell(r, 1).Value = kv.Key;
                    ws.Cell(r, 2).Value = kv.Value;
                    r++;
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                var bytes = ms.ToArray();

                return new BaseResponseDto<byte[]>
                {
                    Success = true,
                    Data = bytes,
                    Message = "Reporte generado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<byte[]>
                {
                    Success = false,
                    Message = "Error al generar el reporte de herramientas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsyncLogico(int id)
        {
            try
            {
                var herramienta = await _repository.GetByIdAsync(id);
                if (herramienta == null || herramienta.Eliminado)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada",
                    };
                }

                // Eliminación lógica
                herramienta.Eliminado = true;
                await _repository.UpdateAsync(herramienta);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Herramienta eliminada correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetHerramientasByUsuarioAsync(int usuarioId)
        {
            try
            {
                var result = await _herramientaRepository.GetHerramientasByUsuarioAsync(usuarioId);

                // Obtener las propiedades del resultado usando reflection
                var usuariosProperty = result.GetType().GetProperty("UsuariosConHerramientas");
                var proveedoresProperty = result.GetType().GetProperty("ProveedoresConHerramientas");
                var resumenProperty = result.GetType().GetProperty("Resumen");

                var usuariosConHerramientas = usuariosProperty?.GetValue(result) as IEnumerable<object> ?? new List<object>();
                var proveedoresConHerramientas = proveedoresProperty?.GetValue(result) as IEnumerable<object> ?? new List<object>();
                var resumen = resumenProperty?.GetValue(result);

                var response = new
                {
                    UsuariosConHerramientas = usuariosConHerramientas.Select(item => new
                    {
                        Usuario = item.GetType().GetProperty("Usuario")?.GetValue(item),
                        CantidadHerramientas = item.GetType().GetProperty("CantidadHerramientas")?.GetValue(item),
                        Herramientas = item.GetType().GetProperty("Herramientas")?.GetValue(item)
                    }),
                    ProveedoresConHerramientas = proveedoresConHerramientas.Select(item => new
                    {
                        Proveedor = item.GetType().GetProperty("Proveedor")?.GetValue(item),
                        CantidadHerramientas = item.GetType().GetProperty("CantidadHerramientas")?.GetValue(item),
                        Herramientas = item.GetType().GetProperty("Herramientas")?.GetValue(item)
                    }),
                    Resumen = resumen
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = response,
                    Message = "Reporte de usuarios y proveedores con herramientas obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de usuarios y proveedores con herramientas");
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al obtener el reporte de usuarios y proveedores con herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<byte[]>> ReporteUsuariosProveedoresAsync(int? usuarioId = null, int? proveedorId = null)
        {
            try
            {
                var result = await _herramientaRepository.GetHerramientasByUsuarioAsync(0);

                // Obtener las propiedades del resultado usando reflection
                var usuariosProperty = result.GetType().GetProperty("UsuariosConHerramientas");
                var proveedoresProperty = result.GetType().GetProperty("ProveedoresConHerramientas");
                var resumenProperty = result.GetType().GetProperty("Resumen");

                var usuariosConHerramientas = usuariosProperty?.GetValue(result) as IEnumerable<object> ?? new List<object>();
                var proveedoresConHerramientas = proveedoresProperty?.GetValue(result) as IEnumerable<object> ?? new List<object>();
                var resumen = resumenProperty?.GetValue(result);

                // Filtrar por usuario específico si se proporciona
                if (usuarioId.HasValue)
                {
                    usuariosConHerramientas = usuariosConHerramientas.Where(item =>
                    {
                        var usuario = item.GetType().GetProperty("Usuario")?.GetValue(item);
                        var idProp = usuario?.GetType().GetProperty("Id")?.GetValue(usuario);
                        return idProp?.Equals(usuarioId.Value) == true;
                    });
                }

                // Filtrar por proveedor específico si se proporciona
                if (proveedorId.HasValue)
                {
                    proveedoresConHerramientas = proveedoresConHerramientas.Where(item =>
                    {
                        var proveedor = item.GetType().GetProperty("Proveedor")?.GetValue(item);
                        var idProp = proveedor?.GetType().GetProperty("Id")?.GetValue(proveedor);
                        return idProp?.Equals(proveedorId.Value) == true;
                    });
                }

                using var workbook = new XLWorkbook();

                // Hoja de Usuarios con Herramientas Prestadas
                if (usuariosConHerramientas.Any())
                {
                    var wsUsuarios = workbook.Worksheets.Add("Usuarios con Herramientas");

                    // Encabezados para usuarios
                    var headerUsuarios = new[] { "ID Usuario", "Nombre Completo", "Legajo", "Cantidad Herramientas", "Código Herramienta", "Nombre Herramienta", "Familia", "Fecha Préstamo" };
                    for (int i = 0; i < headerUsuarios.Length; i++)
                    {
                        wsUsuarios.Cell(1, i + 1).Value = headerUsuarios[i];
                        wsUsuarios.Cell(1, i + 1).Style.Font.Bold = true;
                        wsUsuarios.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    int rowUsuarios = 2;
                    foreach (var item in usuariosConHerramientas)
                    {
                        var usuario = item.GetType().GetProperty("Usuario")?.GetValue(item);
                        var cantidadHerramientas = item.GetType().GetProperty("CantidadHerramientas")?.GetValue(item);
                        var herramientas = item.GetType().GetProperty("Herramientas")?.GetValue(item) as IEnumerable<object> ?? new List<object>();

                        var idUsuario = usuario?.GetType().GetProperty("Id")?.GetValue(usuario)?.ToString();
                        var nombreCompleto = usuario?.GetType().GetProperty("NombreCompleto")?.GetValue(usuario)?.ToString();
                        var legajo = usuario?.GetType().GetProperty("Legajo")?.GetValue(usuario)?.ToString();

                        if (herramientas.Any())
                        {
                            foreach (var herramienta in herramientas)
                            {
                                wsUsuarios.Cell(rowUsuarios, 1).Value = idUsuario;
                                wsUsuarios.Cell(rowUsuarios, 2).Value = nombreCompleto;
                                wsUsuarios.Cell(rowUsuarios, 3).Value = legajo;
                                wsUsuarios.Cell(rowUsuarios, 4).Value = cantidadHerramientas?.ToString();
                                wsUsuarios.Cell(rowUsuarios, 5).Value = herramienta.GetType().GetProperty("Codigo")?.GetValue(herramienta)?.ToString();
                                wsUsuarios.Cell(rowUsuarios, 6).Value = herramienta.GetType().GetProperty("NombreHerramienta")?.GetValue(herramienta)?.ToString();
                                wsUsuarios.Cell(rowUsuarios, 7).Value = herramienta.GetType().GetProperty("Familia")?.GetValue(herramienta)?.ToString();

                                var fechaPrestamo = herramienta.GetType().GetProperty("FechaPrestamo")?.GetValue(herramienta);
                                if (fechaPrestamo is DateTime fecha)
                                {
                                    wsUsuarios.Cell(rowUsuarios, 8).Value = fecha;
                                    wsUsuarios.Cell(rowUsuarios, 8).Style.DateFormat.Format = "dd/mm/yyyy";
                                }
                                else
                                {
                                    wsUsuarios.Cell(rowUsuarios, 8).Value = fechaPrestamo?.ToString();
                                }

                                rowUsuarios++;
                            }
                        }
                        else
                        {
                            // Usuario sin herramientas
                            wsUsuarios.Cell(rowUsuarios, 1).Value = idUsuario;
                            wsUsuarios.Cell(rowUsuarios, 2).Value = nombreCompleto;
                            wsUsuarios.Cell(rowUsuarios, 3).Value = legajo;
                            wsUsuarios.Cell(rowUsuarios, 4).Value = cantidadHerramientas?.ToString();
                            wsUsuarios.Cell(rowUsuarios, 5).Value = "Sin herramientas";
                            rowUsuarios++;
                        }
                    }

                    wsUsuarios.Columns().AdjustToContents();
                }

                // Hoja de Proveedores con Herramientas en Mantenimiento
                if (proveedoresConHerramientas.Any())
                {
                    var wsProveedores = workbook.Worksheets.Add("Proveedores con Herramientas");

                    // Encabezados para proveedores
                    var headerProveedores = new[] { "ID Proveedor", "Nombre Proveedor", "Contacto", "Teléfono", "Cantidad Herramientas", "Código Herramienta", "Nombre Herramienta", "Familia", "Fecha Mantenimiento", "Observaciones" };
                    for (int i = 0; i < headerProveedores.Length; i++)
                    {
                        wsProveedores.Cell(1, i + 1).Value = headerProveedores[i];
                        wsProveedores.Cell(1, i + 1).Style.Font.Bold = true;
                        wsProveedores.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    int rowProveedores = 2;
                    foreach (var item in proveedoresConHerramientas)
                    {
                        var proveedor = item.GetType().GetProperty("Proveedor")?.GetValue(item);
                        var cantidadHerramientas = item.GetType().GetProperty("CantidadHerramientas")?.GetValue(item);
                        var herramientas = item.GetType().GetProperty("Herramientas")?.GetValue(item) as IEnumerable<object> ?? new List<object>();

                        var idProveedor = proveedor?.GetType().GetProperty("Id")?.GetValue(proveedor)?.ToString();
                        var nombreProveedor = proveedor?.GetType().GetProperty("NombreProveedor")?.GetValue(proveedor)?.ToString();
                        var contacto = proveedor?.GetType().GetProperty("Contacto")?.GetValue(proveedor)?.ToString();
                        var telefono = proveedor?.GetType().GetProperty("Telefono")?.GetValue(proveedor)?.ToString();

                        if (herramientas.Any())
                        {
                            foreach (var herramienta in herramientas)
                            {
                                wsProveedores.Cell(rowProveedores, 1).Value = idProveedor;
                                wsProveedores.Cell(rowProveedores, 2).Value = nombreProveedor;
                                wsProveedores.Cell(rowProveedores, 3).Value = contacto;
                                wsProveedores.Cell(rowProveedores, 4).Value = telefono;
                                wsProveedores.Cell(rowProveedores, 5).Value = cantidadHerramientas?.ToString();
                                wsProveedores.Cell(rowProveedores, 6).Value = herramienta.GetType().GetProperty("Codigo")?.GetValue(herramienta)?.ToString();
                                wsProveedores.Cell(rowProveedores, 7).Value = herramienta.GetType().GetProperty("NombreHerramienta")?.GetValue(herramienta)?.ToString();
                                wsProveedores.Cell(rowProveedores, 8).Value = herramienta.GetType().GetProperty("Familia")?.GetValue(herramienta)?.ToString();

                                var fechaMantenimiento = herramienta.GetType().GetProperty("FechaMantenimiento")?.GetValue(herramienta);
                                if (fechaMantenimiento is DateTime fecha)
                                {
                                    wsProveedores.Cell(rowProveedores, 9).Value = fecha;
                                    wsProveedores.Cell(rowProveedores, 9).Style.DateFormat.Format = "dd/mm/yyyy";
                                }
                                else
                                {
                                    wsProveedores.Cell(rowProveedores, 9).Value = fechaMantenimiento?.ToString();
                                }

                                wsProveedores.Cell(rowProveedores, 10).Value = herramienta.GetType().GetProperty("Observaciones")?.GetValue(herramienta)?.ToString();
                                rowProveedores++;
                            }
                        }
                        else
                        {
                            // Proveedor sin herramientas
                            wsProveedores.Cell(rowProveedores, 1).Value = idProveedor;
                            wsProveedores.Cell(rowProveedores, 2).Value = nombreProveedor;
                            wsProveedores.Cell(rowProveedores, 3).Value = contacto;
                            wsProveedores.Cell(rowProveedores, 4).Value = telefono;
                            wsProveedores.Cell(rowProveedores, 5).Value = cantidadHerramientas?.ToString();
                            wsProveedores.Cell(rowProveedores, 6).Value = "Sin herramientas";
                            rowProveedores++;
                        }
                    }

                    wsProveedores.Columns().AdjustToContents();
                }

                // Hoja de Resumen
                var wsResumen = workbook.Worksheets.Add("Resumen");
                int r = 1;

                var tipoReporte = usuarioId.HasValue ? "Individual - Usuario" :
                                proveedorId.HasValue ? "Individual - Proveedor" : "General";

                wsResumen.Cell(r++, 1).Value = $"Reporte de Usuarios y Proveedores - {tipoReporte}";
                wsResumen.Cell(r++, 1).Value = $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                r++;

                if (resumen != null)
                {
                    var totalUsuarios = resumen.GetType().GetProperty("TotalUsuariosConPrestamos")?.GetValue(resumen);
                    var totalProveedores = resumen.GetType().GetProperty("TotalProveedoresConMantenimiento")?.GetValue(resumen);
                    var totalPrestadas = resumen.GetType().GetProperty("TotalHerramientasPrestadas")?.GetValue(resumen);
                    var totalMantenimiento = resumen.GetType().GetProperty("TotalHerramientasEnMantenimiento")?.GetValue(resumen);

                    wsResumen.Cell(r, 1).Value = "Total Usuarios con Préstamos";
                    wsResumen.Cell(r++, 2).Value = totalUsuarios?.ToString();

                    wsResumen.Cell(r, 1).Value = "Total Proveedores con Mantenimiento";
                    wsResumen.Cell(r++, 2).Value = totalProveedores?.ToString();

                    wsResumen.Cell(r, 1).Value = "Total Herramientas Prestadas";
                    wsResumen.Cell(r++, 2).Value = totalPrestadas?.ToString();

                    wsResumen.Cell(r, 1).Value = "Total Herramientas en Mantenimiento";
                    wsResumen.Cell(r++, 2).Value = totalMantenimiento?.ToString();
                }

                wsResumen.Columns().AdjustToContents();

                // Si no hay hojas con datos, crear una hoja informativa
                if (!workbook.Worksheets.Any(ws => ws.Name != "Resumen"))
                {
                    var wsInfo = workbook.Worksheets.Add("Información");
                    wsInfo.Cell(1, 1).Value = "No se encontraron datos para los criterios especificados";
                    wsInfo.Cell(2, 1).Value = $"Fecha de consulta: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var bytes = stream.ToArray();

                var messageType = usuarioId.HasValue ? "usuario específico" :
                                proveedorId.HasValue ? "proveedor específico" : "general";

                return new BaseResponseDto<byte[]>
                {
                    Success = true,
                    Data = bytes,
                    Message = $"Reporte de usuarios y proveedores ({messageType}) generado correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el reporte Excel de usuarios y proveedores");
                return new BaseResponseDto<byte[]>
                {
                    Success = false,
                    Message = "Error al generar el reporte Excel de usuarios y proveedores",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
