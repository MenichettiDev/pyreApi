using pyreApi.DTOs.Common;
using pyreApi.DTOs.TipoAlerta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class TipoAlertaService : GenericService<TipoAlerta>
    {
        public TipoAlertaService(GenericRepository<TipoAlerta> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<TipoAlertaDto>>> GetAllTiposAlertaAsync()
        {
            try
            {
                var tipos = await _repository.GetAllAsync();
                var tipoDtos = tipos.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<TipoAlertaDto>>
                {
                    Success = true,
                    Data = tipoDtos,
                    Message = "Tipos de alerta obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<TipoAlertaDto>>
                {
                    Success = false,
                    Message = "Error al obtener los tipos de alerta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<TipoAlertaDto>> GetTipoAlertaByIdAsync(int id)
        {
            try
            {
                var tipo = await _repository.GetByIdAsync(id);
                if (tipo == null)
                {
                    return new BaseResponseDto<TipoAlertaDto>
                    {
                        Success = false,
                        Message = "Tipo de alerta no encontrado"
                    };
                }

                return new BaseResponseDto<TipoAlertaDto>
                {
                    Success = true,
                    Data = MapToDto(tipo),
                    Message = "Tipo de alerta encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<TipoAlertaDto>
                {
                    Success = false,
                    Message = "Error al buscar el tipo de alerta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<TipoAlertaDto>> CreateTipoAlertaAsync(CreateTipoAlertaDto createDto)
        {
            try
            {
                var tipo = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(tipo);

                return new BaseResponseDto<TipoAlertaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Tipo de alerta creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<TipoAlertaDto>
                {
                    Success = false,
                    Message = "Error al crear el tipo de alerta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<TipoAlertaDto>> UpdateTipoAlertaAsync(UpdateTipoAlertaDto updateDto)
        {
            try
            {
                var existingTipo = await _repository.GetByIdAsync(updateDto.IdTipoAlerta);
                if (existingTipo == null)
                {
                    return new BaseResponseDto<TipoAlertaDto>
                    {
                        Success = false,
                        Message = "Tipo de alerta no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingTipo);
                await _repository.UpdateAsync(existingTipo);

                return new BaseResponseDto<TipoAlertaDto>
                {
                    Success = true,
                    Data = MapToDto(existingTipo),
                    Message = "Tipo de alerta actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<TipoAlertaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el tipo de alerta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private TipoAlertaDto MapToDto(TipoAlerta tipo)
        {
            return new TipoAlertaDto
            {
                IdTipoAlerta = tipo.IdTipoAlerta,
                NombreTipoAlerta = tipo.NombreTipoAlerta
            };
        }

        private TipoAlerta MapFromCreateDto(CreateTipoAlertaDto createDto)
        {
            return new TipoAlerta
            {
                NombreTipoAlerta = createDto.NombreTipoAlerta
            };
        }

        private void MapFromUpdateDto(UpdateTipoAlertaDto updateDto, TipoAlerta tipo)
        {
            tipo.NombreTipoAlerta = updateDto.NombreTipoAlerta;
        }
    }
}
