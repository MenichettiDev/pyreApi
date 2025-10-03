using pyreApi.DTOs.Common;
using pyreApi.DTOs.FamiliaHerramientas;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class FamiliaHerramientasService : GenericService<FamiliaHerramientas>
    {
        public FamiliaHerramientasService(GenericRepository<FamiliaHerramientas> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<FamiliaHerramientasDto>>> GetAllFamiliasAsync()
        {
            try
            {
                var familias = await _repository.GetAllAsync();
                var familiaDtos = familias.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<FamiliaHerramientasDto>>
                {
                    Success = true,
                    Data = familiaDtos,
                    Message = "Familias de herramientas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<FamiliaHerramientasDto>>
                {
                    Success = false,
                    Message = "Error al obtener las familias de herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<FamiliaHerramientasDto>> GetFamiliaByIdAsync(int id)
        {
            try
            {
                var familia = await _repository.GetByIdAsync(id);
                if (familia == null)
                {
                    return new BaseResponseDto<FamiliaHerramientasDto>
                    {
                        Success = false,
                        Message = "Familia de herramientas no encontrada"
                    };
                }

                return new BaseResponseDto<FamiliaHerramientasDto>
                {
                    Success = true,
                    Data = MapToDto(familia),
                    Message = "Familia de herramientas encontrada"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<FamiliaHerramientasDto>
                {
                    Success = false,
                    Message = "Error al buscar la familia de herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<FamiliaHerramientasDto>> CreateFamiliaAsync(CreateFamiliaHerramientasDto createDto)
        {
            try
            {
                var familia = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(familia);

                return new BaseResponseDto<FamiliaHerramientasDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Familia de herramientas creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<FamiliaHerramientasDto>
                {
                    Success = false,
                    Message = "Error al crear la familia de herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<FamiliaHerramientasDto>> UpdateFamiliaAsync(UpdateFamiliaHerramientasDto updateDto)
        {
            try
            {
                var existingFamilia = await _repository.GetByIdAsync(updateDto.IdFamilia);
                if (existingFamilia == null)
                {
                    return new BaseResponseDto<FamiliaHerramientasDto>
                    {
                        Success = false,
                        Message = "Familia de herramientas no encontrada"
                    };
                }

                MapFromUpdateDto(updateDto, existingFamilia);
                await _repository.UpdateAsync(existingFamilia);

                return new BaseResponseDto<FamiliaHerramientasDto>
                {
                    Success = true,
                    Data = MapToDto(existingFamilia),
                    Message = "Familia de herramientas actualizada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<FamiliaHerramientasDto>
                {
                    Success = false,
                    Message = "Error al actualizar la familia de herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private FamiliaHerramientasDto MapToDto(FamiliaHerramientas familia)
        {
            return new FamiliaHerramientasDto
            {
                IdFamilia = familia.IdFamilia,
                NombreFamilia = familia.NombreFamilia
            };
        }

        private FamiliaHerramientas MapFromCreateDto(CreateFamiliaHerramientasDto createDto)
        {
            return new FamiliaHerramientas
            {
                NombreFamilia = createDto.NombreFamilia
            };
        }

        private void MapFromUpdateDto(UpdateFamiliaHerramientasDto updateDto, FamiliaHerramientas familia)
        {
            familia.NombreFamilia = updateDto.NombreFamilia;
        }
    }
}
