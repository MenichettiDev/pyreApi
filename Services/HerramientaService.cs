using pyreApi.DTOs.Common;
using pyreApi.DTOs.Herramienta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class HerramientaService : GenericService<Herramienta>
    {
        private readonly HerramientaRepository _herramientaRepository;

        public HerramientaService(HerramientaRepository herramientaRepository) : base(herramientaRepository)
        {
            _herramientaRepository = herramientaRepository;
        }

        public async Task<BaseResponseDto<Herramienta>> CreateHerramientaAsync(CreateHerramientaDto createDto)
        {
            try
            {
                // Validar si el código ya existe
                var existingTool = await _herramientaRepository.GetByCodigoAsync(createDto.Codigo);
                if (existingTool != null)
                {
                    return new BaseResponseDto<Herramienta>
                    {
                        Success = false,
                        Message = "Ya existe una herramienta con este código"
                    };
                }

                var herramienta = new Herramienta
                {
                    Codigo = createDto.Codigo,
                    NombreHerramienta = createDto.NombreHerramienta,
                    IdFamilia = createDto.IdFamilia,
                    Tipo = createDto.Tipo,
                    Marca = createDto.Marca,
                    Serie = createDto.Serie,
                    CostoDolares = createDto.CostoDolares,
                    UbicacionFisica = createDto.UbicacionFisica,
                    IdEstadoActual = createDto.IdEstadoActual,
                    IdPlanta = createDto.IdPlanta,
                    Ubicacion = createDto.Ubicacion,
                    FechaDeIngreso = DateTime.UtcNow,
                    Activo = true,
                    EnReparacion = false
                };

                var result = await _herramientaRepository.AddAsync(herramienta);
                return new BaseResponseDto<Herramienta>
                {
                    Success = true,
                    Data = result,
                    Message = "Herramienta creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Herramienta>
                {
                    Success = false,
                    Message = "Error al crear la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Herramienta>>> GetAvailableToolsAsync()
        {
            try
            {
                var tools = await _herramientaRepository.GetAvailableToolsAsync();
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = true,
                    Data = tools,
                    Message = "Herramientas disponibles obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = false,
                    Message = "Error al obtener herramientas disponibles",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Herramienta>>> GetByFamiliaAsync(int familiaId)
        {
            try
            {
                var tools = await _herramientaRepository.GetByFamiliaAsync(familiaId);
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = true,
                    Data = tools,
                    Message = "Herramientas por familia obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = false,
                    Message = "Error al obtener herramientas por familia",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto> ChangeStatusAsync(HerramientaStatusDto statusDto)
        {
            try
            {
                var herramienta = await _herramientaRepository.GetByIdAsync(statusDto.IdHerramienta);
                if (herramienta == null)
                {
                    return new BaseResponseDto
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                herramienta.Activo = statusDto.Activo;
                await _herramientaRepository.UpdateAsync(herramienta);

                return new BaseResponseDto
                {
                    Success = true,
                    Message = statusDto.Activo ? "Herramienta activada correctamente" : "Herramienta desactivada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto
                {
                    Success = false,
                    Message = "Error al cambiar el estado de la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
