using Microsoft.EntityFrameworkCore; // <-- agregado
using pyreApi.Data;
using pyreApi.DTOs.Common;
using pyreApi.DTOs.MovimientoHerramienta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class MovimientoHerramientaService : GenericService<MovimientoHerramienta>
    {
        private readonly MovimientoHerramientaRepository _movimientoRepository;
        private readonly HerramientaRepository _herramientaRepository;
        private readonly ApplicationDbContext _context;
        private readonly AlertaService _alertaService;

        public MovimientoHerramientaService(
            MovimientoHerramientaRepository movimientoRepository,
            HerramientaRepository herramientaRepository,
            ApplicationDbContext context,
            AlertaService alertaService
        )
            : base(movimientoRepository)
        {
            _movimientoRepository = movimientoRepository;
            _herramientaRepository = herramientaRepository;
            _context = context;
            _alertaService = alertaService;
        }

        public async Task<
            BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
        > GetAllMovimientosAsync()
        {
            try
            {
                var movimientos = await _movimientoRepository.GetAllAsync();
                var movimientosList = movimientos.ToList();
                foreach (var m in movimientosList)
                    await EnsureUsuarioResponsableLoaded(m);
                var movimientoDtos = movimientosList.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Movimientos obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener los movimientos",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<MovimientoHerramientaDto>> GetMovimientoByIdAsync(int id)
        {
            try
            {
                var movimiento = await _movimientoRepository.GetByIdAsync(id);
                if (movimiento == null)
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Movimiento no encontrado",
                    };
                }

                await EnsureUsuarioResponsableLoaded(movimiento);

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(movimiento),
                    Message = "Movimiento encontrado",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al buscar el movimiento",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<MovimientoHerramientaDto>> CreateMovimientoAsync(
            CreateMovimientoDto createDto
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar que la herramienta existe
                var herramienta = await _herramientaRepository.GetByIdAsync(
                    createDto.IdHerramienta
                );
                if (herramienta == null)
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada",
                    };
                }

                // Capturar el estado inicial de la herramienta ANTES de actualizarlo
                var estadoInicialHerramienta = herramienta.IdDisponibilidad;

                // Validar que la transición de estado es válida
                if (
                    !IsValidStateTransition(
                        herramienta.IdDisponibilidad,
                        createDto.IdTipoMovimiento
                    )
                )
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = GetStateTransitionErrorMessage(
                            herramienta.IdDisponibilidad,
                            createDto.IdTipoMovimiento
                        ),
                    };
                }

                var movimiento = MapFromCreateDto(createDto);
                var added = await _movimientoRepository.AddAsync(movimiento);

                // Determinar y actualizar el nuevo estado de disponibilidad
                var nuevoEstadoDisponibilidad = DetermineNewAvailabilityStatus(
                    createDto.IdTipoMovimiento
                );
                herramienta.IdDisponibilidad = nuevoEstadoDisponibilidad;
                await _herramientaRepository.UpdateAsync(herramienta);

                //Alertas - Usar el estado inicial para determinar si resolver alertas
                // Si es una devolución Y la herramienta estaba prestada, resolver alertas automáticamente
                if (createDto.IdTipoMovimiento == 2 && estadoInicialHerramienta == 2) // Devolución desde Prestada
                {
                    // Buscar el último movimiento de préstamo de esta herramienta
                    var ultimoPrestamo = await _context.Set<MovimientoHerramienta>()
                        .Where(m => m.IdHerramienta == createDto.IdHerramienta
                                && m.IdTipoMovimiento == 1) // Préstamo
                        .OrderByDescending(m => m.Fecha)
                        .FirstOrDefaultAsync();

                    if (ultimoPrestamo != null)
                    {
                        await _alertaService.ResolveAlertasByMovimientoAsync(ultimoPrestamo.IdMovimiento);
                    }
                }
                // Si es una devolución Y la herramienta estaba en mantenimiento, resolver alertas de reparación
                else if (createDto.IdTipoMovimiento == 2 && estadoInicialHerramienta == 3) // Devolución desde Mantenimiento
                {
                    // Buscar el último movimiento de envío a reparación de esta herramienta
                    var ultimaReparacion = await _context.Set<MovimientoHerramienta>()
                        .Where(m => m.IdHerramienta == createDto.IdHerramienta
                                && m.IdTipoMovimiento == 3) // Envío Reparación
                        .OrderByDescending(m => m.Fecha)
                        .FirstOrDefaultAsync();

                    if (ultimaReparacion != null)
                    {
                        await _alertaService.ResolveAlertasByMovimientoAsync(ultimaReparacion.IdMovimiento);
                    }
                }

                // Confirmar la transacción
                await transaction.CommitAsync();

                // Recargar el movimiento con las relaciones para garantizar que venga el apellido del responsable
                var movimientoConRel = await _context
                    .Set<MovimientoHerramienta>()
                    .Include(m => m.UsuarioResponsable)
                    .Include(m => m.UsuarioGenera)
                    .Include(m => m.Herramienta)
                    .Include(m => m.TipoMovimiento)
                    .Include(m => m.Obra)
                    .Include(m => m.EstadoDevolucion)
                    .Include(m => m.Proveedor)
                    .FirstOrDefaultAsync(m => m.IdMovimiento == added.IdMovimiento);

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(movimientoConRel ?? added),
                    Message = "Movimiento registrado correctamente",
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al registrar el movimiento",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<MovimientoHerramientaDto>> UpdateMovimientoAsync(
            UpdateMovimientoHerramientaDto updateDto
        )
        {
            try
            {
                var existingMovimiento = await _movimientoRepository.GetByIdAsync(
                    updateDto.IdMovimiento
                );
                if (existingMovimiento == null)
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Movimiento no encontrado",
                    };
                }

                MapFromUpdateDto(updateDto, existingMovimiento);
                await _movimientoRepository.UpdateAsync(existingMovimiento);

                // Recargar con relaciones para asegurarnos de incluir apellido del responsable
                var movimientoConRel = await _context
                    .Set<MovimientoHerramienta>()
                    .Include(m => m.UsuarioResponsable)
                    .Include(m => m.UsuarioGenera)
                    .Include(m => m.Herramienta)
                    .Include(m => m.TipoMovimiento)
                    .Include(m => m.Obra)
                    .Include(m => m.EstadoDevolucion)
                    .Include(m => m.Proveedor)
                    .FirstOrDefaultAsync(m => m.IdMovimiento == existingMovimiento.IdMovimiento);

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(movimientoConRel ?? existingMovimiento),
                    Message = "Movimiento actualizado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el movimiento",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
        > GetByHerramientaAsync(int herramientaId)
        {
            try
            {
                var movimientos = await _movimientoRepository.GetByHerramientaAsync(herramientaId);
                var movimientosList = movimientos.ToList();
                foreach (var m in movimientosList)
                    await EnsureUsuarioResponsableLoaded(m);
                var movimientoDtos = movimientosList.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Historial de movimientos obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener el historial de movimientos",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
        > GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var movimientos = await _movimientoRepository.GetMovimientosByDateRangeAsync(
                    startDate,
                    endDate
                );
                var movimientosList = movimientos.ToList();
                foreach (var m in movimientosList)
                    await EnsureUsuarioResponsableLoaded(m);
                var movimientoDtos = movimientosList.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Movimientos por rango de fechas obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener movimientos por rango de fechas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<PaginatedResponseDto<MovimientoHerramientaDto>>
        > GetAllMovimientosPaginatedAsync(
            int page,
            int pageSize,
            string? nombreHerramienta = null,
            int? idFamiliaHerramienta = null,
            int? idUsuarioGenera = null,
            int? idUsuarioResponsable = null,
            int? idTipoMovimiento = null,
            int? idObra = null,
            int? idProveedor = null,
            int? idEstadoFisico = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (pageSize <= 0)
                    pageSize = 10;

                // Obtener movimientos con filtros aplicados directamente en la base de datos
                var movimientos = await _movimientoRepository.GetFilteredMovimientosAsync(
                    nombreHerramienta,
                    idFamiliaHerramienta,
                    idUsuarioGenera,
                    idUsuarioResponsable,
                    idTipoMovimiento,
                    idObra,
                    idProveedor,
                    idEstadoFisico,
                    fechaDesde,
                    fechaHasta
                );

                // Ordenar por fecha descendente antes de aplicar la paginación
                var movimientosOrdenados = movimientos.OrderByDescending(m => m.Fecha);

                var totalRecords = movimientosOrdenados.Count();

                var movimientosPage = movimientosOrdenados
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Asegurar carga de apellido de responsable en la página
                foreach (var m in movimientosPage)
                    await EnsureUsuarioResponsableLoaded(m);

                var movimientosDto = movimientosPage.Select(MapToDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<MovimientoHerramientaDto>
                {
                    Data = movimientosDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                };

                return new BaseResponseDto<PaginatedResponseDto<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Movimientos obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message =
                        "No se pudieron cargar los movimientos. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud: " + ex.Message,
                    },
                };
            }
        }

        public async Task<
            BaseResponseDto<MovimientoHerramientaDto>
        > GetLatestMovimientoByHerramientaAsync(int herramientaId)
        {
            try
            {
                var movimiento = await _movimientoRepository.GetLatestMovimientoByHerramientaAsync(
                    herramientaId
                );
                if (movimiento == null)
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "No se encontraron movimientos para esta herramienta",
                    };
                }

                await EnsureUsuarioResponsableLoaded(movimiento);

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(movimiento),
                    Message = "Último movimiento obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al obtener el último movimiento",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
        > GetLatest5BorrowedToolsAsync()
        {
            try
            {
                var movimientos = await _movimientoRepository.GetLatest5BorrowedToolsAsync();
                var movimientosList = movimientos.ToList();
                foreach (var m in movimientosList)
                    await EnsureUsuarioResponsableLoaded(m);
                var movimientoDtos = movimientosList.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Últimas 5 herramientas prestadas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las últimas herramientas prestadas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<IEnumerable<HerramientaRankingDto>>
        > GetMostBorrowedToolsLast30DaysAsync()
        {
            try
            {
                var rankingData = await _movimientoRepository.GetMostBorrowedToolsLast30DaysAsync();
                var rankingDtos = rankingData.Select(item =>
                {
                    var itemType = item.GetType();
                    return new HerramientaRankingDto
                    {
                        IdHerramienta = (int)itemType.GetProperty("IdHerramienta")?.GetValue(item)!,
                        CodigoHerramienta = (string?)
                            itemType.GetProperty("CodigoHerramienta")?.GetValue(item),
                        NombreHerramienta = (string?)
                            itemType.GetProperty("NombreHerramienta")?.GetValue(item),
                        FamiliaHerramienta = (string?)
                            itemType.GetProperty("FamiliaHerramienta")?.GetValue(item),
                        TotalPrestamos = (int)
                            itemType.GetProperty("TotalPrestamos")?.GetValue(item)!,
                        UltimoPrestamo = (DateTime?)
                            itemType.GetProperty("UltimoPrestamo")?.GetValue(item),
                    };
                });

                return new BaseResponseDto<IEnumerable<HerramientaRankingDto>>
                {
                    Success = true,
                    Data = rankingDtos,
                    Message = "Ranking de herramientas más prestadas obtenido correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaRankingDto>>
                {
                    Success = false,
                    Message = "Error al obtener el ranking de herramientas más prestadas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        private int DetermineNewAvailabilityStatus(int tipoMovimientoId)
        {
            return tipoMovimientoId switch
            {
                1 => 2, // Préstamo -> Prestada
                2 => 1, // Devolución -> Disponible
                3 => 3, // Envío Reparación -> Mantenimiento
                4 => 4, // Baja -> Extraviada (o podrías crear otro estado)
                5 => 1, // Alta -> Disponible
                _ => throw new ArgumentException(
                    $"Tipo de movimiento no válido: {tipoMovimientoId}"
                ),
            };
        }

        private bool IsValidStateTransition(int estadoActual, int tipoMovimiento)
        {
            return (estadoActual, tipoMovimiento) switch
            {
                // Desde Disponible (1)
                (1, 1) => true, // Disponible -> Préstamo
                (1, 3) => true, // Disponible -> Envío Reparación
                (1, 4) => true, // Disponible -> Baja

                // Desde Prestada (2)
                (2, 2) => true, // Prestada -> Devolución
                (2, 4) => true, // Prestada -> Baja (en caso de extravío)

                // Desde Mantenimiento (3)
                (3, 2) => true, // Mantenimiento -> Devolución (finalizar reparación)
                // (3, 4) => true,  // Mantenimiento -> Baja (no se puede reparar)

                // Desde Extraviada (4)
                (4, 5) => true, // Extraviada -> Alta (si se recupera)

                _ => false,
            };
        }

        private string GetStateTransitionErrorMessage(int estadoActual, int tipoMovimiento)
        {
            var estadoNombre = GetEstadoName(estadoActual);
            var movimientoNombre = GetTipoMovimientoName(tipoMovimiento);

            return $"No se puede realizar el movimiento '{movimientoNombre}' cuando la herramienta está en estado '{estadoNombre}'";
        }

        private string GetEstadoName(int estadoId)
        {
            return estadoId switch
            {
                1 => "Disponible",
                2 => "Prestada",
                3 => "Mantenimiento",
                4 => "Extraviada",
                _ => "Desconocido",
            };
        }

        private string GetTipoMovimientoName(int tipoId)
        {
            return tipoId switch
            {
                1 => "Préstamo",
                2 => "Devolución",
                3 => "Envío Reparación",
                4 => "Baja",
                5 => "Alta",
                _ => "Desconocido",
            };
        }

        private MovimientoHerramientaDto MapToDto(MovimientoHerramienta movimiento)
        {
            return new MovimientoHerramientaDto
            {
                IdMovimiento = movimiento.IdMovimiento,
                IdHerramienta = movimiento.IdHerramienta,
                IdUsuarioGenera = movimiento.IdUsuarioGenera,
                IdUsuarioResponsable = movimiento.IdUsuarioResponsable,
                IdTipoMovimiento = movimiento.IdTipoMovimiento,
                Fecha = movimiento.Fecha,
                IdObra = movimiento.IdObra,
                IdProveedor = movimiento.IdProveedor,
                FechaEstimadaDevolucion = movimiento.FechaEstimadaDevolucion,
                EstadoHerramientaAlDevolver = movimiento.EstadoHerramientaAlDevolver,
                Observaciones = movimiento.Observaciones,
                CodigoHerramienta = movimiento.Herramienta?.Codigo,
                NombreHerramienta = movimiento.Herramienta?.NombreHerramienta,
                NombreUsuarioGenera = movimiento.UsuarioGenera?.Nombre,
                // Se ajusta para enviar Nombre + Apellido del responsable siempre que exista
                NombreUsuarioResponsable =
                    movimiento.UsuarioResponsable != null
                        ? $"{movimiento.UsuarioResponsable?.Nombre ?? string.Empty} {movimiento.UsuarioResponsable?.Apellido ?? string.Empty}".Trim()
                        : null,
                TipoMovimiento = movimiento.TipoMovimiento?.NombreTipoMovimiento,
                NombreObra = movimiento.Obra?.NombreObra,
                EstadoDevolucion = movimiento.EstadoDevolucion?.Descripcion,
                NombreProveedor = movimiento.Proveedor?.NombreProveedor,
            };
        }

        private MovimientoHerramienta MapFromCreateDto(CreateMovimientoDto createDto)
        {
            return new MovimientoHerramienta
            {
                IdHerramienta = createDto.IdHerramienta,
                IdUsuarioGenera = createDto.IdUsuarioGenera,
                IdUsuarioResponsable = createDto.IdUsuarioResponsable,
                IdTipoMovimiento = createDto.IdTipoMovimiento,
                IdObra = createDto.IdObra,
                IdProveedor = createDto.IdProveedor,
                FechaEstimadaDevolucion = createDto.FechaEstimadaDevolucion,
                // Corregido: usar la propiedad correcta presente en los DTOs
                EstadoHerramientaAlDevolver = createDto.EstadoHerramientaAlDevolver,
                Observaciones = createDto.Observaciones,
                Fecha = DateTime.Now,
            };
        }

        private void MapFromUpdateDto(
            UpdateMovimientoHerramientaDto updateDto,
            MovimientoHerramienta movimiento
        )
        {
            movimiento.IdHerramienta = updateDto.IdHerramienta;
            movimiento.IdUsuarioGenera = updateDto.IdUsuarioGenera;
            movimiento.IdUsuarioResponsable = updateDto.IdUsuarioResponsable;
            movimiento.IdTipoMovimiento = updateDto.IdTipoMovimiento;
            movimiento.Fecha = updateDto.Fecha;
            movimiento.IdObra = updateDto.IdObra;
            movimiento.IdProveedor = updateDto.IdProveedor;
            movimiento.FechaEstimadaDevolucion = updateDto.FechaEstimadaDevolucion;
            // Corregido: usar la propiedad correcta presente en los DTOs
            movimiento.EstadoHerramientaAlDevolver = updateDto.EstadoHerramientaAlDevolver;
            movimiento.Observaciones = updateDto.Observaciones;
        }

        // Nuevo helper para cargar el UsuarioResponsable si falta
        private async Task EnsureUsuarioResponsableLoaded(MovimientoHerramienta movimiento)
        {
            if (movimiento == null)
                return;

            if (movimiento.UsuarioResponsable == null && movimiento.IdUsuarioResponsable > 0)
            {
                // Intentar cargar desde el contexto para obtener Apellido
                var usuario = await _context
                    .Set<Usuario>()
                    .FirstOrDefaultAsync(u => u.Id == movimiento.IdUsuarioResponsable);
                if (usuario != null)
                    movimiento.UsuarioResponsable = usuario;
            }
        }
    }
}
