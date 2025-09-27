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
                    IdEstadoFisico = createDto.IdEstadoFisico,
                    IdPlanta = createDto.IdPlanta,
                    Ubicacion = createDto.Ubicacion,
                    FechaDeIngreso = DateTime.UtcNow,
                    Activo = true,
                    IdDisponibilidad = createDto.IdDisponibilidad
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

        public async Task<BaseResponseDto<Herramienta>> UpdateHerramientaAsync(UpdateHerramientaDto updateDto)
        {
            try
            {
                var herramienta = await _herramientaRepository.GetByIdAsync(updateDto.IdHerramienta);
                if (herramienta == null)
                {
                    return new BaseResponseDto<Herramienta>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                // Update only provided properties
                if (!string.IsNullOrEmpty(updateDto.NombreHerramienta))
                    herramienta.NombreHerramienta = updateDto.NombreHerramienta;

                if (updateDto.IdFamilia.HasValue)
                    herramienta.IdFamilia = updateDto.IdFamilia.Value;

                if (!string.IsNullOrEmpty(updateDto.Tipo))
                    herramienta.Tipo = updateDto.Tipo;

                if (!string.IsNullOrEmpty(updateDto.Marca))
                    herramienta.Marca = updateDto.Marca;

                if (!string.IsNullOrEmpty(updateDto.Serie))
                    herramienta.Serie = updateDto.Serie;

                if (updateDto.CostoDolares.HasValue)
                    herramienta.CostoDolares = updateDto.CostoDolares;

                if (!string.IsNullOrEmpty(updateDto.UbicacionFisica))
                    herramienta.UbicacionFisica = updateDto.UbicacionFisica;

                if (updateDto.IdEstadoFisico.HasValue)
                    herramienta.IdEstadoFisico = updateDto.IdEstadoFisico.Value;

                if (updateDto.IdDisponibilidad.HasValue)
                    herramienta.IdDisponibilidad = updateDto.IdDisponibilidad.Value;

                if (updateDto.IdPlanta.HasValue)
                    herramienta.IdPlanta = updateDto.IdPlanta.Value;

                if (!string.IsNullOrEmpty(updateDto.Ubicacion))
                    herramienta.Ubicacion = updateDto.Ubicacion;

                await _herramientaRepository.UpdateAsync(herramienta);

                return new BaseResponseDto<Herramienta>
                {
                    Success = true,
                    Data = herramienta,
                    Message = "Herramienta actualizada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Herramienta>
                {
                    Success = false,
                    Message = "Error al actualizar la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Herramienta>>> GetByEstadoFisicoAsync(int estadoFisicoId)
        {
            try
            {
                var tools = await _herramientaRepository.GetByEstadoAsync(estadoFisicoId);
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = true,
                    Data = tools,
                    Message = "Herramientas por estado físico obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = false,
                    Message = "Error al obtener herramientas por estado físico",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Herramienta>>> GetInRepairAsync()
        {
            try
            {
                var tools = await _herramientaRepository.GetInRepairAsync();
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = true,
                    Data = tools,
                    Message = "Herramientas en reparación obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Herramienta>>
                {
                    Success = false,
                    Message = "Error al obtener herramientas en reparación",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
