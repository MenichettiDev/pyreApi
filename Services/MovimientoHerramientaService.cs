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

        public async Task<BaseResponseDto<MovimientoHerramienta>> CreateMovimientoAsync(CreateMovimientoDto createDto)
        {
            try
            {
                // Validar que la herramienta existe
                var herramienta = await _herramientaRepository.GetByIdAsync(createDto.IdHerramienta);
                if (herramienta == null)
                {
                    return new BaseResponseDto<MovimientoHerramienta>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                // Validar disponibilidad de la herramienta para egresos
                // TODO: Implementar lógica de validación según tipo de movimiento

                var movimiento = new MovimientoHerramienta
                {
                    IdHerramienta = createDto.IdHerramienta,
                    IdUsuario = createDto.IdUsuario,
                    IdTipoMovimiento = createDto.IdTipoMovimiento,
                    DestinoObra = createDto.DestinoObra,
                    IdObra = createDto.IdObra,
                    FechaEstimadaDevolucion = createDto.FechaEstimadaDevolucion,
                    EstadoHerramientaAlDevolver = createDto.EstadoHerramientaAlDevolver,
                    Observaciones = createDto.Observaciones,
                    Fecha = DateTime.UtcNow
                };

                var result = await _movimientoRepository.AddAsync(movimiento);

                // TODO: Actualizar estado de la herramienta según el tipo de movimiento

                return new BaseResponseDto<MovimientoHerramienta>
                {
                    Success = true,
                    Data = result,
                    Message = "Movimiento registrado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<MovimientoHerramienta>
                {
                    Success = false,
                    Message = "Error al registrar el movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<MovimientoHerramienta>>> GetByHerramientaAsync(int herramientaId)
        {
            try
            {
                var movimientos = await _movimientoRepository.GetByHerramientaAsync(herramientaId);
                return new BaseResponseDto<IEnumerable<MovimientoHerramienta>>
                {
                    Success = true,
                    Data = movimientos,
                    Message = "Historial de movimientos obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramienta>>
                {
                    Success = false,
                    Message = "Error al obtener el historial de movimientos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<MovimientoHerramienta>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var movimientos = await _movimientoRepository.GetMovimientosByDateRangeAsync(startDate, endDate);
                return new BaseResponseDto<IEnumerable<MovimientoHerramienta>>
                {
                    Success = true,
                    Data = movimientos,
                    Message = "Movimientos por rango de fechas obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<MovimientoHerramienta>>
                {
                    Success = false,
                    Message = "Error al obtener movimientos por rango de fechas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
