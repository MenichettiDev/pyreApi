using pyreApi.DTOs.Common;
using pyreApi.DTOs.EstadoDisponibilidad;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class EstadoDisponibilidadService : GenericService<EstadoDisponibilidadHerramienta>
    {
        public EstadoDisponibilidadService(GenericRepository<EstadoDisponibilidadHerramienta> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<EstadoDisponibilidadDto>>> GetAllEstadosDisponibilidadAsync()
        {
            try
            {
                var estados = await _repository.GetAllAsync();
                var estadoDtos = estados.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<EstadoDisponibilidadDto>>
                {
                    Success = true,
                    Data = estadoDtos,
                    Message = "Estados de disponibilidad obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<EstadoDisponibilidadDto>>
                {
                    Success = false,
                    Message = "Error al obtener los estados de disponibilidad",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<EstadoDisponibilidadDto>> GetEstadoDisponibilidadByIdAsync(int id)
        {
            try
            {
                var estado = await _repository.GetByIdAsync(id);
                if (estado == null)
                {
                    return new BaseResponseDto<EstadoDisponibilidadDto>
                    {
                        Success = false,
                        Message = "Estado de disponibilidad no encontrado"
                    };
                }

                return new BaseResponseDto<EstadoDisponibilidadDto>
                {
                    Success = true,
                    Data = MapToDto(estado),
                    Message = "Estado de disponibilidad encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<EstadoDisponibilidadDto>
                {
                    Success = false,
                    Message = "Error al buscar el estado de disponibilidad",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<EstadoDisponibilidadDto>> CreateEstadoDisponibilidadAsync(CreateEstadoDisponibilidadDto createDto)
        {
            try
            {
                var estado = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(estado);

                return new BaseResponseDto<EstadoDisponibilidadDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Estado de disponibilidad creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<EstadoDisponibilidadDto>
                {
                    Success = false,
                    Message = "Error al crear el estado de disponibilidad",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<EstadoDisponibilidadDto>> UpdateEstadoDisponibilidadAsync(UpdateEstadoDisponibilidadDto updateDto)
        {
            try
            {
                var existingEstado = await _repository.GetByIdAsync(updateDto.IdEstadoDisponibilidad);
                if (existingEstado == null)
                {
                    return new BaseResponseDto<EstadoDisponibilidadDto>
                    {
                        Success = false,
                        Message = "Estado de disponibilidad no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingEstado);
                await _repository.UpdateAsync(existingEstado);

                return new BaseResponseDto<EstadoDisponibilidadDto>
                {
                    Success = true,
                    Data = MapToDto(existingEstado),
                    Message = "Estado de disponibilidad actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<EstadoDisponibilidadDto>
                {
                    Success = false,
                    Message = "Error al actualizar el estado de disponibilidad",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private EstadoDisponibilidadDto MapToDto(EstadoDisponibilidadHerramienta estado)
        {
            return new EstadoDisponibilidadDto
            {
                IdEstadoDisponibilidad = estado.Id,
                DescripcionEstado = estado.Descripcion
            };
        }

        private EstadoDisponibilidadHerramienta MapFromCreateDto(CreateEstadoDisponibilidadDto createDto)
        {
            return new EstadoDisponibilidadHerramienta
            {
                Descripcion = createDto.DescripcionEstado
            };
        }

        private void MapFromUpdateDto(UpdateEstadoDisponibilidadDto updateDto, EstadoDisponibilidadHerramienta estado)
        {
            estado.Descripcion = updateDto.DescripcionEstado;
        }
    }
}
