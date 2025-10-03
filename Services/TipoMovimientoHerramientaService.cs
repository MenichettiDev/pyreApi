using pyreApi.DTOs.Common;
using pyreApi.DTOs.TipoMovimientoHerramienta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class TipoMovimientoHerramientaService : GenericService<TipoMovimientoHerramienta>
    {
        public TipoMovimientoHerramientaService(GenericRepository<TipoMovimientoHerramienta> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<TipoMovimientoHerramientaDto>>> GetAllTiposMovimientoAsync()
        {
            try
            {
                var tipos = await _repository.GetAllAsync();
                var tipoDtos = tipos.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<TipoMovimientoHerramientaDto>>
                {
                    Success = true,
                    Data = tipoDtos,
                    Message = "Tipos de movimiento obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<TipoMovimientoHerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener los tipos de movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<TipoMovimientoHerramientaDto>> GetTipoMovimientoByIdAsync(int id)
        {
            try
            {
                var tipo = await _repository.GetByIdAsync(id);
                if (tipo == null)
                {
                    return new BaseResponseDto<TipoMovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Tipo de movimiento no encontrado"
                    };
                }

                return new BaseResponseDto<TipoMovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(tipo),
                    Message = "Tipo de movimiento encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<TipoMovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al buscar el tipo de movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<TipoMovimientoHerramientaDto>> CreateTipoMovimientoAsync(CreateTipoMovimientoHerramientaDto createDto)
        {
            try
            {
                var tipo = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(tipo);

                return new BaseResponseDto<TipoMovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Tipo de movimiento creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<TipoMovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al crear el tipo de movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<TipoMovimientoHerramientaDto>> UpdateTipoMovimientoAsync(UpdateTipoMovimientoHerramientaDto updateDto)
        {
            try
            {
                var existingTipo = await _repository.GetByIdAsync(updateDto.IdTipoMovimiento);
                if (existingTipo == null)
                {
                    return new BaseResponseDto<TipoMovimientoHerramientaDto>
                    {
                        Success = false,
                        Message = "Tipo de movimiento no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingTipo);
                await _repository.UpdateAsync(existingTipo);

                return new BaseResponseDto<TipoMovimientoHerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingTipo),
                    Message = "Tipo de movimiento actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<TipoMovimientoHerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el tipo de movimiento",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private TipoMovimientoHerramientaDto MapToDto(TipoMovimientoHerramienta tipo)
        {
            return new TipoMovimientoHerramientaDto
            {
                IdTipoMovimiento = tipo.IdTipoMovimiento,
                NombreTipoMovimiento = tipo.NombreTipoMovimiento
            };
        }

        private TipoMovimientoHerramienta MapFromCreateDto(CreateTipoMovimientoHerramientaDto createDto)
        {
            return new TipoMovimientoHerramienta
            {
                NombreTipoMovimiento = createDto.NombreTipoMovimiento
            };
        }

        private void MapFromUpdateDto(UpdateTipoMovimientoHerramientaDto updateDto, TipoMovimientoHerramienta tipo)
        {
            tipo.NombreTipoMovimiento = updateDto.NombreTipoMovimiento;
        }
    }
}
