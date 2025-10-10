using pyreApi.DTOs.Common;
using pyreApi.DTOs.Obra;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class ObraService : GenericService<Obra>
    {
        public ObraService(GenericRepository<Obra> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<ObraDto>>> GetAllObrasAsync()
        {
            try
            {
                var obras = await _repository.GetAllAsync();
                var obraDtos = obras.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ObraDto>>
                {
                    Success = true,
                    Data = obraDtos,
                    Message = "Obras obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ObraDto>>
                {
                    Success = false,
                    Message = "Error al obtener las obras",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<ObraDto>> GetObraByIdAsync(int id)
        {
            try
            {
                var obra = await _repository.GetByIdAsync(id);
                if (obra == null)
                {
                    return new BaseResponseDto<ObraDto>
                    {
                        Success = false,
                        Message = "Obra no encontrada"
                    };
                }

                return new BaseResponseDto<ObraDto>
                {
                    Success = true,
                    Data = MapToDto(obra),
                    Message = "Obra encontrada"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ObraDto>
                {
                    Success = false,
                    Message = "Error al buscar la obra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<ObraDto>> CreateObraAsync(CreateObraDto createDto)
        {
            try
            {
                var obra = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(obra);

                return new BaseResponseDto<ObraDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Obra creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ObraDto>
                {
                    Success = false,
                    Message = "Error al crear la obra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<ObraDto>> UpdateObraAsync(UpdateObraDto updateDto)
        {
            try
            {
                var existingObra = await _repository.GetByIdAsync(updateDto.IdObra);
                if (existingObra == null)
                {
                    return new BaseResponseDto<ObraDto>
                    {
                        Success = false,
                        Message = "Obra no encontrada"
                    };
                }

                MapFromUpdateDto(updateDto, existingObra);
                await _repository.UpdateAsync(existingObra);

                return new BaseResponseDto<ObraDto>
                {
                    Success = true,
                    Data = MapToDto(existingObra),
                    Message = "Obra actualizada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ObraDto>
                {
                    Success = false,
                    Message = "Error al actualizar la obra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<ObraDto>>> GetAllObrasPaginatedAsync(
            int page,
            int pageSize,
            string? nombre = null,
            string? ubicacion = null,
            string? codigo = null,
            bool? activo = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var obras = await _repository.GetAllAsync();
                IEnumerable<Obra> filtered = obras;

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrim = nombre.Trim().ToLowerInvariant();
                    filtered = filtered.Where(o => (o.NombreObra ?? string.Empty).ToLowerInvariant().Contains(nombreTrim));
                }

                if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    var ubicacionTrim = ubicacion.Trim().ToLowerInvariant();
                    filtered = filtered.Where(o => (o.Ubicacion ?? string.Empty).ToLowerInvariant().Contains(ubicacionTrim));
                }

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    var codigoTrim = codigo.Trim().ToLowerInvariant();
                    filtered = filtered.Where(o => (o.Codigo ?? string.Empty).ToLowerInvariant().Contains(codigoTrim));
                }

                var totalRecords = filtered.Count();
                var obrasPage = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var obraDtos = obrasPage.Select(MapToDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<ObraDto>
                {
                    Data = obraDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<ObraDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Obras obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<ObraDto>>
                {
                    Success = false,
                    Message = "Error al obtener las obras",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private ObraDto MapToDto(Obra obra)
        {
            return new ObraDto
            {
                IdObra = obra.IdObra,
                Codigo = obra.Codigo,
                NombreObra = obra.NombreObra,
                Ubicacion = obra.Ubicacion,
                FechaInicio = obra.FechaInicio,
                FechaFin = obra.FechaFin
            };
        }

        private Obra MapFromCreateDto(CreateObraDto createDto)
        {
            return new Obra
            {
                Codigo = createDto.Codigo,
                NombreObra = createDto.NombreObra,
                Ubicacion = createDto.Ubicacion,
                FechaInicio = createDto.FechaInicio,
                FechaFin = createDto.FechaFin
            };
        }

        private void MapFromUpdateDto(UpdateObraDto updateDto, Obra obra)
        {
            obra.Codigo = updateDto.Codigo;
            obra.NombreObra = updateDto.NombreObra;
            obra.Ubicacion = updateDto.Ubicacion;
            obra.FechaInicio = updateDto.FechaInicio;
            obra.FechaFin = updateDto.FechaFin;
        }
    }
}
