using pyreApi.DTOs.Common;
using pyreApi.DTOs.Alerta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class AlertaService : GenericService<Alerta>
    {
        private readonly GenericRepository<Alerta> _alertaRepository;

        public AlertaService(GenericRepository<Alerta> alertaRepository) : base(alertaRepository)
        {
            _alertaRepository = alertaRepository;
        }

        public async Task<BaseResponseDto<Alerta>> CreateAlertaAsync(CreateAlertaDto createDto)
        {
            try
            {
                var alerta = new Alerta
                {
                    IdTipoAlerta = createDto.IdTipoAlerta,
                    IdHerramienta = createDto.IdHerramienta ?? 0,
                    FechaGeneracion = DateTime.UtcNow,
                    Leida = false
                };

                var result = await _alertaRepository.AddAsync(alerta);
                return new BaseResponseDto<Alerta>
                {
                    Success = true,
                    Data = result,
                    Message = "Alerta creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Alerta>
                {
                    Success = false,
                    Message = "Error al crear la alerta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Alerta>>> GetActiveAlertsAsync()
        {
            try
            {
                var alerts = await _alertaRepository.FindAsync(a => !a.Leida);
                return new BaseResponseDto<IEnumerable<Alerta>>
                {
                    Success = true,
                    Data = alerts,
                    Message = "Alertas activas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Alerta>>
                {
                    Success = false,
                    Message = "Error al obtener alertas activas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto> MarkAsReadAsync(int alertaId)
        {
            try
            {
                var alerta = await _alertaRepository.GetByIdAsync(alertaId);
                if (alerta == null)
                {
                    return new BaseResponseDto
                    {
                        Success = false,
                        Message = "Alerta no encontrada"
                    };
                }

                alerta.Leida = true;
                await _alertaRepository.UpdateAsync(alerta);

                return new BaseResponseDto
                {
                    Success = true,
                    Message = "Alerta marcada como leída"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto
                {
                    Success = false,
                    Message = "Error al marcar la alerta como leída",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}