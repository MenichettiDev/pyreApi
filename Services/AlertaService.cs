using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.DTOs.Alerta;
using pyreApi.DTOs.Common;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class AlertaService : GenericService<Alerta>
    {
        private readonly AlertaRepository _alertaRepository;
        private readonly MovimientoHerramientaRepository _movimientoRepository;
        private readonly ApplicationDbContext _context;

        public AlertaService(
            AlertaRepository repository,
            MovimientoHerramientaRepository movimientoRepository,
            ApplicationDbContext context
        )
            : base(repository)
        {
            _alertaRepository = repository;
            _movimientoRepository = movimientoRepository;
            _context = context;
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAllAlertasAsync()
        {
            try
            {
                var alertas = await _repository.GetAllAsync();
                var alertaDtos = alertas
                    .Where(a => a.Activo)
                    .Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<AlertaDto>> GetAlertaByIdAsync(int id)
        {
            try
            {
                var alerta = await _repository.GetByIdAsync(id);
                if (alerta == null)
                {
                    return new BaseResponseDto<AlertaDto>
                    {
                        Success = false,
                        Message = "Alerta no encontrada",
                    };
                }

                return new BaseResponseDto<AlertaDto>
                {
                    Success = true,
                    Data = MapToDto(alerta),
                    Message = "Alerta encontrada",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al buscar la alerta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<AlertaDto>> CreateAlertaAsync(CreateAlertaDto createDto)
        {
            try
            {
                var alerta = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(alerta);

                return new BaseResponseDto<AlertaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Alerta creada correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al crear la alerta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<AlertaDto>> UpdateAlertaAsync(UpdateAlertaDto updateDto)
        {
            try
            {
                var existingAlerta = await _repository.GetByIdAsync(updateDto.IdAlerta);
                if (existingAlerta == null)
                {
                    return new BaseResponseDto<AlertaDto>
                    {
                        Success = false,
                        Message = "Alerta no encontrada",
                    };
                }

                MapFromUpdateDto(updateDto, existingAlerta);
                await _repository.UpdateAsync(existingAlerta);

                return new BaseResponseDto<AlertaDto>
                {
                    Success = true,
                    Data = MapToDto(existingAlerta),
                    Message = "Alerta actualizada correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al actualizar la alerta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAlertasByHerramientaAsync(
            int idHerramienta
        )
        {
            try
            {
                var alertas = await _alertaRepository.GetByMovimientoHerramientaAsync(
                    idHerramienta
                );
                var alertaDtos = alertas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas de la herramienta",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetUnreadAlertasAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetUnreadAsync();
                var alertaDtos = alertas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas no leídas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas no leídas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<bool>> MarkAlertaAsReadAsync(int id)
        {
            try
            {
                var alerta = await _repository.GetByIdAsync(id);
                if (alerta == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "Alerta no encontrada",
                    };
                }

                alerta.Activo = false; // Mark as read by setting Activo to false
                await _repository.UpdateAsync(alerta);

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Alerta marcada como leída",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al marcar la alerta como leída",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetByTipoAlertaAsync(
            int idTipoAlerta
        )
        {
            try
            {
                var alertas = await _alertaRepository.GetByTipoAlertaAsync(idTipoAlerta);
                var alertaDtos = alertas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas por tipo obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas por tipo",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAlertasPendientesAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                var pendientes = alertas
                    .Where(a => a.IdTipoAlerta == 1 && a.Activo)
                    .Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = pendientes,
                    Message = "Alertas pendientes obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas pendientes",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAlertasVencidasAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                var vencidas = alertas.Where(a => a.IdTipoAlerta == 2 && a.Activo).Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = vencidas,
                    Message = "Alertas vencidas obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas vencidas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetCountAlertasPendientesAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                int count = alertas.Where(a => a.IdTipoAlerta == 1 && a.Activo).Count();

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Cantidad de alertas pendientes obtenida correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener la cantidad de alertas pendientes",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetCountAlertasVencidasAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                int count = alertas.Where(a => a.IdTipoAlerta == 2 && a.Activo).Count();

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Cantidad de alertas vencidas obtenida correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener la cantidad de alertas vencidas",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        private AlertaDto MapToDto(Alerta alerta)
        {
            // Cargar el usuario responsable si no está cargado
            var movimiento = alerta.MovimientoHerramienta;
            if (movimiento?.UsuarioResponsable == null && movimiento?.IdUsuarioResponsable > 0)
            {
                // Intentar cargar desde contexto
                var usuario = _context
                    .Set<Usuario>()
                    .FirstOrDefault(u => u.Id == movimiento.IdUsuarioResponsable);
                if (usuario != null && movimiento != null)
                {
                    movimiento.UsuarioResponsable = usuario;
                }
            }

            // Construir responsableNombre concatenando Nombre + Apellido
            string responsableNombre = string.Empty;
            if (
                movimiento?.IdUsuarioResponsable.HasValue == true
                && movimiento?.UsuarioResponsable != null
            )
            {
                responsableNombre =
                    $"{movimiento.UsuarioResponsable.Nombre ?? string.Empty} {movimiento.UsuarioResponsable.Apellido ?? string.Empty}".Trim();
            }
            else if (movimiento?.Proveedor != null)
            {
                responsableNombre = movimiento.Proveedor.NombreProveedor;
            }

            return new AlertaDto
            {
                IdAlerta = alerta.IdAlerta,
                IdMovimiento = alerta.IdMovimiento,
                NombreHerramienta =
                    alerta.MovimientoHerramienta?.Herramienta?.NombreHerramienta ?? string.Empty,
                IdTipoAlerta = alerta.IdTipoAlerta,
                NombreTipoAlerta = alerta.TipoAlerta?.NombreTipoAlerta ?? string.Empty,
                FechaGeneracion = alerta.FechaGeneracion,
                FechaVencimiento =
                    alerta.MovimientoHerramienta?.FechaEstimadaDevolucion ?? DateTime.MinValue,
                Comentario = alerta.Comentario,
                IdModifica = alerta.IdModifica ?? 0,
                Activo = alerta.Activo,
                HerramientaNombre = alerta.MovimientoHerramienta?.Herramienta?.NombreHerramienta,
                HerramientaCodigo = alerta.MovimientoHerramienta?.Herramienta?.Codigo,
                ResponsableNombre = responsableNombre,
                TipoMovimiento =
                    alerta.MovimientoHerramienta?.IdTipoMovimiento == 1
                        ? "Préstamo"
                        : "Mantenimiento",
            };
        }

        private Alerta MapFromCreateDto(CreateAlertaDto createDto)
        {
            return new Alerta
            {
                IdMovimiento = createDto.IdMovimiento,
                IdTipoAlerta = createDto.IdTipoAlerta,
                Comentario = createDto.Comentario,
                FechaGeneracion = DateTime.UtcNow,
                Activo = true,
            };
        }

        private void MapFromUpdateDto(UpdateAlertaDto updateDto, Alerta alerta)
        {
            alerta.IdMovimiento = updateDto.IdMovimiento;
            alerta.IdTipoAlerta = updateDto.IdTipoAlerta;
            alerta.Comentario = updateDto.Comentario;
            alerta.IdModifica = updateDto.IdModifica;
            alerta.Activo = updateDto.Activo;
        }

        public async Task<BaseResponseDto<AlertaDto>> UpdateAlertaAndMovimientoAsync(
            UpdateAlertaMovimientoDto updateDto
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Obtener la alerta
                var alerta = await _alertaRepository.GetByIdAsync(updateDto.IdAlerta);
                if (alerta == null)
                {
                    return new BaseResponseDto<AlertaDto>
                    {
                        Success = false,
                        Message = "Alerta no encontrada",
                    };
                }

                // Obtener el movimiento relacionado
                var movimiento = await _movimientoRepository.GetByIdAsync(alerta.IdMovimiento);
                if (movimiento == null)
                {
                    return new BaseResponseDto<AlertaDto>
                    {
                        Success = false,
                        Message = "Movimiento relacionado no encontrado",
                    };
                }

                // Actualizar el movimiento
                movimiento.FechaEstimadaDevolucion = updateDto.FechaEstimadaDevolucion;
                await _movimientoRepository.UpdateAsync(movimiento);

                // Actualizar la alerta
                alerta.Comentario = updateDto.Comentario;
                alerta.IdModifica = updateDto.IdModifica;
                alerta.Activo = false; // Marcar la alerta como leída
                await _alertaRepository.UpdateAsync(alerta);

                await transaction.CommitAsync();

                return new BaseResponseDto<AlertaDto>
                {
                    Success = true,
                    Data = MapToDto(alerta),
                    Message = "Alerta y movimiento actualizados correctamente",
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al actualizar la alerta y movimiento",
                    Errors = new List<string> { ex.Message },
                };
            }
        }
    }
}
