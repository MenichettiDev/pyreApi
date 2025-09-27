using pyreApi.DTOs.Common;
using pyreApi.DTOs.ReparacionHerramienta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class ReparacionHerramientaService : GenericService<ReparacionHerramienta>
    {
        private readonly GenericRepository<ReparacionHerramienta> _reparacionRepository;
        private readonly HerramientaRepository _herramientaRepository;

        public ReparacionHerramientaService(
            GenericRepository<ReparacionHerramienta> reparacionRepository,
            HerramientaRepository herramientaRepository) : base(reparacionRepository)
        {
            _reparacionRepository = reparacionRepository;
            _herramientaRepository = herramientaRepository;
        }

        public async Task<BaseResponseDto<ReparacionHerramienta>> CreateReparacionAsync(CreateReparacionDto createDto)
        {
            try
            {
                // Validar que la herramienta existe
                var herramienta = await _herramientaRepository.GetByIdAsync(createDto.IdHerramienta);
                if (herramienta == null)
                {
                    return new BaseResponseDto<ReparacionHerramienta>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                var reparacion = new ReparacionHerramienta
                {
                    IdHerramienta = createDto.IdHerramienta,
                    IdProveedor = createDto.IdProveedor,
                    IdUsuario = createDto.IdUsuarioResponsable,
                    FechaSalida = DateTime.UtcNow,
                    FechaAcordadaReparacion = createDto.FechaEstimadaFinalizacion,
                    Observaciones = createDto.Observaciones
                };

                var result = await _reparacionRepository.AddAsync(reparacion);

                // Marcar herramienta en reparación
                herramienta.EstadoDisponibilidad.Id = 3;
                await _herramientaRepository.UpdateAsync(herramienta);

                return new BaseResponseDto<ReparacionHerramienta>
                {
                    Success = true,
                    Data = result,
                    Message = "Reparación registrada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ReparacionHerramienta>
                {
                    Success = false,
                    Message = "Error al registrar la reparación",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ReparacionHerramienta>>> GetActiveRepairsAsync()
        {
            try
            {
                // Since ReparacionHerramienta doesn't have Finalizada property, check if it has FechaAcordadaReparacion without return date
                var repairs = await _reparacionRepository.FindAsync(r => r.FechaAcordadaReparacion != null);
                return new BaseResponseDto<IEnumerable<ReparacionHerramienta>>
                {
                    Success = true,
                    Data = repairs,
                    Message = "Reparaciones activas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ReparacionHerramienta>>
                {
                    Success = false,
                    Message = "Error al obtener reparaciones activas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}