using pyreApi.DTOs.Common;
using pyreApi.DTOs.Alerta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class AlertaService : GenericService<Alerta>
    {
        private readonly AlertaRepository _alertaRepository;

        public AlertaService(AlertaRepository repository) : base(repository)
        {
            _alertaRepository = repository;
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAllAlertasAsync()
        {
            try
            {
                var alertas = await _repository.GetAllAsync();
                var alertaDtos = alertas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas",
                    Errors = new List<string> { ex.Message }
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
                        Message = "Alerta no encontrada"
                    };
                }

                return new BaseResponseDto<AlertaDto>
                {
                    Success = true,
                    Data = MapToDto(alerta),
                    Message = "Alerta encontrada"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al buscar la alerta",
                    Errors = new List<string> { ex.Message }
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
                    Message = "Alerta creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al crear la alerta",
                    Errors = new List<string> { ex.Message }
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
                        Message = "Alerta no encontrada"
                    };
                }

                MapFromUpdateDto(updateDto, existingAlerta);
                await _repository.UpdateAsync(existingAlerta);

                return new BaseResponseDto<AlertaDto>
                {
                    Success = true,
                    Data = MapToDto(existingAlerta),
                    Message = "Alerta actualizada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<AlertaDto>
                {
                    Success = false,
                    Message = "Error al actualizar la alerta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAlertasByHerramientaAsync(int idHerramienta)
        {
            try
            {
                var alertas = await _alertaRepository.GetByHerramientaAsync(idHerramienta);
                var alertaDtos = alertas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas de la herramienta",
                    Errors = new List<string> { ex.Message }
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
                    Message = "Alertas no leídas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas no leídas",
                    Errors = new List<string> { ex.Message }
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
                        Message = "Alerta no encontrada"
                    };
                }

                alerta.Leida = true;
                await _repository.UpdateAsync(alerta);

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Alerta marcada como leída"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al marcar la alerta como leída",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetByTipoAlertaAsync(int idTipoAlerta)
        {
            try
            {
                var alertas = await _alertaRepository.GetByTipoAlertaAsync(idTipoAlerta);
                var alertaDtos = alertas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = alertaDtos,
                    Message = "Alertas por tipo obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas por tipo",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAlertasPendientesAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                var pendientes = alertas
                    .Where(a => a.IdTipoAlerta == 1 && a.Herramienta != null &&
                        a.FechaGeneracion.AddDays(a.Herramienta.DiasAlerta) > DateTime.Now)
                    .Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = pendientes,
                    Message = "Alertas pendientes obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas pendientes",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<AlertaDto>>> GetAlertasVencidasAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                var vencidas = alertas
                    .Where(a => a.IdTipoAlerta == 2 && a.Herramienta != null &&
                        a.FechaGeneracion.AddDays(a.Herramienta.DiasAlerta) <= DateTime.Now)
                    .Select(MapToDto);

                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = true,
                    Data = vencidas,
                    Message = "Alertas vencidas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<AlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las alertas vencidas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetCountAlertasPendientesAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                int count = alertas
                    .Where(a => a.IdTipoAlerta == 1 && a.Herramienta != null &&
                        a.FechaGeneracion.AddDays(a.Herramienta.DiasAlerta) > DateTime.Now)
                    .Count();

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Cantidad de alertas pendientes obtenida correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener la cantidad de alertas pendientes",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetCountAlertasVencidasAsync()
        {
            try
            {
                var alertas = await _alertaRepository.GetAllAsync();
                int count = alertas
                    .Where(a => a.IdTipoAlerta == 2 && a.Herramienta != null &&
                        a.FechaGeneracion.AddDays(a.Herramienta.DiasAlerta) <= DateTime.Now)
                    .Count();

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = count,
                    Message = "Cantidad de alertas vencidas obtenida correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener la cantidad de alertas vencidas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private AlertaDto MapToDto(Alerta alerta)
        {
            return new AlertaDto
            {
                IdAlerta = alerta.IdAlerta,
                IdHerramienta = alerta.IdHerramienta,
                NombreHerramienta = alerta.Herramienta?.NombreHerramienta ?? string.Empty,
                IdTipoAlerta = alerta.IdTipoAlerta,
                NombreTipoAlerta = alerta.TipoAlerta?.NombreTipoAlerta ?? string.Empty,
                FechaGeneracion = alerta.FechaGeneracion,
                Leida = alerta.Leida
            };
        }

        private Alerta MapFromCreateDto(CreateAlertaDto createDto)
        {
            return new Alerta
            {
                IdHerramienta = createDto.IdHerramienta,
                IdTipoAlerta = createDto.IdTipoAlerta
            };
        }

        private void MapFromUpdateDto(UpdateAlertaDto updateDto, Alerta alerta)
        {
            alerta.IdHerramienta = updateDto.IdHerramienta;
            alerta.Leida = updateDto.Leida;
        }
    }
}