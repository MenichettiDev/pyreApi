using pyreApi.DTOs.Common;
using pyreApi.DTOs.Planta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class PlantaService : GenericService<Planta>
    {
        public PlantaService(GenericRepository<Planta> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<PlantaDto>>> GetAllPlantasAsync()
        {
            try
            {
                var plantas = await _repository.GetAllAsync();
                var plantaDtos = plantas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<PlantaDto>>
                {
                    Success = true,
                    Data = plantaDtos,
                    Message = "Plantas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<PlantaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las plantas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PlantaDto>> GetPlantaByIdAsync(int id)
        {
            try
            {
                var planta = await _repository.GetByIdAsync(id);
                if (planta == null)
                {
                    return new BaseResponseDto<PlantaDto>
                    {
                        Success = false,
                        Message = "Planta no encontrada"
                    };
                }

                return new BaseResponseDto<PlantaDto>
                {
                    Success = true,
                    Data = MapToDto(planta),
                    Message = "Planta encontrada"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PlantaDto>
                {
                    Success = false,
                    Message = "Error al buscar la planta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PlantaDto>> CreatePlantaAsync(CreatePlantaDto createDto)
        {
            try
            {
                var planta = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(planta);

                return new BaseResponseDto<PlantaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Planta creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PlantaDto>
                {
                    Success = false,
                    Message = "Error al crear la planta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PlantaDto>> UpdatePlantaAsync(UpdatePlantaDto updateDto)
        {
            try
            {
                var existingPlanta = await _repository.GetByIdAsync(updateDto.IdPlanta);
                if (existingPlanta == null)
                {
                    return new BaseResponseDto<PlantaDto>
                    {
                        Success = false,
                        Message = "Planta no encontrada"
                    };
                }

                MapFromUpdateDto(updateDto, existingPlanta);
                await _repository.UpdateAsync(existingPlanta);

                return new BaseResponseDto<PlantaDto>
                {
                    Success = true,
                    Data = MapToDto(existingPlanta),
                    Message = "Planta actualizada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PlantaDto>
                {
                    Success = false,
                    Message = "Error al actualizar la planta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private PlantaDto MapToDto(Planta planta)
        {
            return new PlantaDto
            {
                IdPlanta = planta.IdPlanta,
                NombrePlanta = planta.NombrePlanta,
                Ubicacion = planta.Ubicacion,
                Direccion = planta.Direccion,
                Activa = planta.Activa,
                TotalHerramientas = planta.Herramientas?.Count ?? 0,
                HerramientasActivas = planta.Herramientas?.Count(h => h.Activo) ?? 0
            };
        }

        private Planta MapFromCreateDto(CreatePlantaDto createDto)
        {
            return new Planta
            {
                NombrePlanta = createDto.NombrePlanta,
                Ubicacion = createDto.Ubicacion,
                Direccion = createDto.Direccion,
                Activa = createDto.Activa
            };
        }

        private void MapFromUpdateDto(UpdatePlantaDto updateDto, Planta planta)
        {
            planta.NombrePlanta = updateDto.NombrePlanta;
            planta.Ubicacion = updateDto.Ubicacion;
            planta.Direccion = updateDto.Direccion;
            planta.Activa = updateDto.Activa;
        }
    }
}
