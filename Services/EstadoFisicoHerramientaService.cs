using pyreApi.DTOs.Common;
using pyreApi.DTOs.EstadoFisicoHerramienta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class EstadoFisicoHerramientaService : GenericService<EstadoFisicoHerramienta>
    {
        public EstadoFisicoHerramientaService(GenericRepository<EstadoFisicoHerramienta> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<EstadoFisicoHerramientaDto>>> GetAllEstadosFisicosAsync()
        {
            try
            {
                var estados = await _repository.GetAllAsync();
                var estadoDtos = estados.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<EstadoFisicoHerramientaDto>>
                {
                    Success = true,
                    Data = estadoDtos,
                    Message = "Estados físicos obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<EstadoFisicoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener los estados físicos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<EstadoFisicoHerramientaDto>> GetEstadoFisicoByIdAsync(int id)
        {
            try
            {
                var estado = await _repository.GetByIdAsync(id);
                if (estado == null)
                {
                    return new BaseResponseDto<EstadoFisicoHerramientaDto>
                    {
                        Success = false,
                        Message = "Estado físico no encontrado"
                    };
                }

                return new BaseResponseDto<EstadoFisicoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(estado),
                    Message = "Estado físico encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<EstadoFisicoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al buscar el estado físico",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<EstadoFisicoHerramientaDto>> CreateEstadoFisicoAsync(CreateEstadoFisicoHerramientaDto createDto)
        {
            try
            {
                var estado = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(estado);

                return new BaseResponseDto<EstadoFisicoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Estado físico creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<EstadoFisicoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al crear el estado físico",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<EstadoFisicoHerramientaDto>> UpdateEstadoFisicoAsync(UpdateEstadoFisicoHerramientaDto updateDto)
        {
            try
            {
                var existingEstado = await _repository.GetByIdAsync(updateDto.IdEstadoFisico);
                if (existingEstado == null)
                {
                    return new BaseResponseDto<EstadoFisicoHerramientaDto>
                    {
                        Success = false,
                        Message = "Estado físico no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingEstado);
                await _repository.UpdateAsync(existingEstado);

                return new BaseResponseDto<EstadoFisicoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingEstado),
                    Message = "Estado físico actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<EstadoFisicoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el estado físico",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private EstadoFisicoHerramientaDto MapToDto(EstadoFisicoHerramienta estado)
        {
            return new EstadoFisicoHerramientaDto
            {
                IdEstadoFisico = estado.Id,
                DescripcionEstado = estado.Descripcion
            };
        }

        private EstadoFisicoHerramienta MapFromCreateDto(CreateEstadoFisicoHerramientaDto createDto)
        {
            return new EstadoFisicoHerramienta
            {
                Descripcion = createDto.DescripcionEstado
            };
        }

        private void MapFromUpdateDto(UpdateEstadoFisicoHerramientaDto updateDto, EstadoFisicoHerramienta estado)
        {
            estado.Descripcion = updateDto.DescripcionEstado;
        }
    }
}
