using pyreApi.DTOs.Common;
using pyreApi.DTOs.MovimientoHerramienta;
using pyreApi.Models;
using pyreApi.Repositories;
using pyreApi.Data;


namespace pyreApi.Services
{
    public class MovimientoHerramientaService : GenericService<MovimientoHerramienta>
    {
        private readonly MovimientoHerramientaRepository _movimientoRepository;
        private readonly HerramientaRepository _herramientaRepository;
        private readonly ApplicationDbContext _context;

        public MovimientoHerramientaService(
            MovimientoHerramientaRepository movimientoRepository,
            HerramientaRepository herramientaRepository,
            ApplicationDbContext context) : base(movimientoRepository)
        {
            _movimientoRepository = movimientoRepository;
            _herramientaRepository = herramientaRepository;
            _context = context;
        }

        public async Task<BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>> GetAllMovimientosAsync()
        {
            try
            {
                var movimientos = await _movimientoRepository.GetAllAsync();
                var movimientoDtos = movimientos.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Movimientos obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener los movimientos",
                    Errors = new List<string> { ex.Message }
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
                        Message = "Movimiento no encontrado"
                    };
                }

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(movimiento),
                    Message = "Movimiento encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al buscar el movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<MovimientoHerramientaDto>> CreateMovimientoAsync(CreateMovimientoDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar que la herramienta existe
                var herramienta = await _herramientaRepository.GetByIdAsync(createDto.IdHerramienta);
                if (herramienta == null)
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                // Validar que la transición de estado es válida
                if (!IsValidStateTransition(herramienta.IdDisponibilidad, createDto.IdTipoMovimiento))
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = GetStateTransitionErrorMessage(herramienta.IdDisponibilidad, createDto.IdTipoMovimiento)
                    };
                }

                var movimiento = MapFromCreateDto(createDto);
                var result = await _movimientoRepository.AddAsync(movimiento);

                // Determinar y actualizar el nuevo estado de disponibilidad
                var nuevoEstadoDisponibilidad = DetermineNewAvailabilityStatus(createDto.IdTipoMovimiento);
                herramienta.IdDisponibilidad = nuevoEstadoDisponibilidad;
                await _herramientaRepository.UpdateAsync(herramienta);

                // Confirmar la transacción
                await transaction.CommitAsync();

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Movimiento registrado correctamente"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al registrar el movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<MovimientoHerramientaDto>> UpdateMovimientoAsync(UpdateMovimientoHerramientaDto updateDto)
        {
            try
            {
                var existingMovimiento = await _movimientoRepository.GetByIdAsync(updateDto.IdMovimiento);
                if (existingMovimiento == null)
                {
                    return new BaseResponseDto<MovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Movimiento no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingMovimiento);
                await _movimientoRepository.UpdateAsync(existingMovimiento);

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingMovimiento),
                    Message = "Movimiento actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>> GetByHerramientaAsync(int herramientaId)
        {
            try
            {
                var movimientos = await _movimientoRepository.GetByHerramientaAsync(herramientaId);
                var movimientoDtos = movimientos.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Historial de movimientos obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener el historial de movimientos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var movimientos = await _movimientoRepository.GetMovimientosByDateRangeAsync(startDate, endDate);
                var movimientoDtos = movimientos.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = movimientoDtos,
                    Message = "Movimientos por rango de fechas obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener movimientos por rango de fechas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<MovimientoHerramientaDto>>> GetAllMovimientosPaginatedAsync(
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
            DateTime? fechaHasta = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                // Obtener movimientos con filtros aplicados directamente en la base de datos
                var movimientos = await _movimientoRepository.GetFilteredMovimientosAsync(
                    nombreHerramienta, idFamiliaHerramienta, idUsuarioGenera, idUsuarioResponsable,
                    idTipoMovimiento, idObra, idProveedor, idEstadoFisico, fechaDesde, fechaHasta);

                // Ordenar por fecha descendente antes de aplicar la paginación
                var movimientosOrdenados = movimientos.OrderByDescending(m => m.Fecha);

                var totalRecords = movimientosOrdenados.Count();

                var movimientosPage = movimientosOrdenados
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

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
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<MovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Movimientos obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<MovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los movimientos. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor al procesar la solicitud: " + ex.Message }
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
                _ => throw new ArgumentException($"Tipo de movimiento no válido: {tipoMovimientoId}")
            };
        }
        private bool IsValidStateTransition(int estadoActual, int tipoMovimiento)
        {
            return (estadoActual, tipoMovimiento) switch
            {
                // Desde Disponible (1)
                (1, 1) => true,  // Disponible -> Préstamo
                (1, 3) => true,  // Disponible -> Envío Reparación
                (1, 4) => true,  // Disponible -> Baja

                // Desde Prestada (2)
                (2, 2) => true,  // Prestada -> Devolución
                (2, 4) => true,  // Prestada -> Baja (en caso de extravío)

                // Desde Mantenimiento (3)
                (3, 2) => true,  // Mantenimiento -> Devolución (finalizar reparación)
                (3, 4) => true,  // Mantenimiento -> Baja (no se puede reparar)

                // Desde Extraviada (4)
                (4, 5) => true,  // Extraviada -> Alta (si se recupera)

                _ => false
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
                _ => "Desconocido"
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
                _ => "Desconocido"
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
                NombreUsuarioResponsable = movimiento.UsuarioResponsable?.Nombre,
                TipoMovimiento = movimiento.TipoMovimiento?.NombreTipoMovimiento,
                NombreObra = movimiento.Obra?.NombreObra,
                EstadoDevolucion = movimiento.EstadoDevolucion?.Descripcion
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
                EstadoHerramientaAlDevolver = createDto.EstadoHerramientaAlDevolver,
                Observaciones = createDto.Observaciones,
                Fecha = DateTime.UtcNow
            };
        }

        private void MapFromUpdateDto(UpdateMovimientoHerramientaDto updateDto, MovimientoHerramienta movimiento)
        {
            movimiento.IdHerramienta = updateDto.IdHerramienta;
            movimiento.IdUsuarioGenera = updateDto.IdUsuarioGenera;
            movimiento.IdUsuarioResponsable = updateDto.IdUsuarioResponsable;
            movimiento.IdTipoMovimiento = updateDto.IdTipoMovimiento;
            movimiento.Fecha = updateDto.Fecha;
            movimiento.IdObra = updateDto.IdObra;
            movimiento.IdProveedor = updateDto.IdProveedor;
            movimiento.FechaEstimadaDevolucion = updateDto.FechaEstimadaDevolucion;
            movimiento.EstadoHerramientaAlDevolver = updateDto.EstadoHerramientaAlDevolver;
            movimiento.Observaciones = updateDto.Observaciones;
        }
    }
}
