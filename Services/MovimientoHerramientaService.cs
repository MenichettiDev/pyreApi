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

        public MovimientoHerramientaService(
            MovimientoHerramientaRepository movimientoRepository,
            HerramientaRepository herramientaRepository) : base(movimientoRepository)
        {
            _movimientoRepository = movimientoRepository;
            _herramientaRepository = herramientaRepository;
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

                var movimiento = MapFromCreateDto(createDto);
                var result = await _movimientoRepository.AddAsync(movimiento);

                return new BaseResponseDto<MovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Movimiento registrado correctamente"
                };
            }
            catch (Exception ex)
            {
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

        private MovimientoHerramientaDto MapToDto(MovimientoHerramienta movimiento)
        {
            return new MovimientoHerramientaDto
            {
                IdMovimiento = movimiento.IdMovimiento,
                IdHerramienta = movimiento.IdHerramienta,
                IdUsuario = movimiento.IdUsuario,
                IdTipoMovimiento = movimiento.IdTipoMovimiento,
                Fecha = movimiento.Fecha,
                IdObra = movimiento.IdObra,
                FechaEstimadaDevolucion = movimiento.FechaEstimadaDevolucion,
                EstadoHerramientaAlDevolver = movimiento.EstadoHerramientaAlDevolver,
                Observaciones = movimiento.Observaciones,
                CodigoHerramienta = movimiento.Herramienta?.Codigo,
                NombreHerramienta = movimiento.Herramienta?.NombreHerramienta,
                NombreUsuario = movimiento.Usuario?.Nombre,
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
                IdUsuario = createDto.IdUsuario,
                IdTipoMovimiento = createDto.IdTipoMovimiento,
                IdObra = createDto.IdObra,
                FechaEstimadaDevolucion = createDto.FechaEstimadaDevolucion,
                EstadoHerramientaAlDevolver = createDto.EstadoHerramientaAlDevolver,
                Observaciones = createDto.Observaciones,
                Fecha = DateTime.UtcNow
            };
        }

        private void MapFromUpdateDto(UpdateMovimientoHerramientaDto updateDto, MovimientoHerramienta movimiento)
        {
            movimiento.IdHerramienta = updateDto.IdHerramienta;
            movimiento.IdUsuario = updateDto.IdUsuario;
            movimiento.IdTipoMovimiento = updateDto.IdTipoMovimiento;
            movimiento.Fecha = updateDto.Fecha;
            movimiento.IdObra = updateDto.IdObra;
            movimiento.FechaEstimadaDevolucion = updateDto.FechaEstimadaDevolucion;
            movimiento.EstadoHerramientaAlDevolver = updateDto.EstadoHerramientaAlDevolver;
            movimiento.Observaciones = updateDto.Observaciones;
        }
    }
}
