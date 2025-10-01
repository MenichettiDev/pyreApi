using pyreApi.DTOs.Common;
using pyreApi.Repositories;
using System.Linq.Expressions;

namespace pyreApi.Services
{
    public class GenericService<T> where T : class
    {
        protected readonly GenericRepository<T> _repository;

        public GenericService(GenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<BaseResponseDto<IEnumerable<T>>> GetAllAsync()
        {
            try
            {
                var data = await _repository.GetAllAsync();
                return new BaseResponseDto<IEnumerable<T>>
                {
                    Success = true,
                    Data = data,
                    Message = "Datos obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<T>>
                {
                    Success = false,
                    Message = "Error al obtener los datos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<T>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new BaseResponseDto<T>
                    {
                        Success = false,
                        Message = "Registro no encontrado"
                    };
                }

                return new BaseResponseDto<T>
                {
                    Success = true,
                    Data = entity,
                    Message = "Registro encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<T>
                {
                    Success = false,
                    Message = "Error al buscar el registro",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<T>> AddAsync(T entity)
        {
            try
            {
                var result = await _repository.AddAsync(entity);
                return new BaseResponseDto<T>
                {
                    Success = true,
                    Data = result,
                    Message = "Registro creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<T>
                {
                    Success = false,
                    Message = "Error al crear el registro",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto> UpdateAsync(T entity)
        {
            try
            {
                await _repository.UpdateAsync(entity);
                return new BaseResponseDto
                {
                    Success = true,
                    Message = "Registro actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto
                {
                    Success = false,
                    Message = "Error al actualizar el registro",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto> DeleteAsync(int id)
        {
            try
            {
                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                {
                    return new BaseResponseDto
                    {
                        Success = false,
                        Message = "Registro no encontrado"
                    };
                }

                await _repository.DeleteAsync(id);
                return new BaseResponseDto
                {
                    Success = true,
                    Message = "Registro eliminado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto
                {
                    Success = false,
                    Message = "Error al eliminar el registro",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<T>>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (data, totalRecords) = await _repository.GetPagedAsync(page, pageSize);
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<T>
                {
                    Data = data,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<T>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Datos obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<T>>
                {
                    Success = false,
                    Message = "Error al obtener los datos paginados",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
