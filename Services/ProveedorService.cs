using pyreApi.DTOs.Common;
using pyreApi.DTOs.Proveedor;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class ProveedorService : GenericService<Proveedor>
    {
        public ProveedorService(GenericRepository<Proveedor> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<ProveedorDto>>> GetAllProveedoresAsync()
        {
            try
            {
                var proveedores = await _repository.GetAllAsync();
                var proveedorDtos = proveedores.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = true,
                    Data = proveedorDtos,
                    Message = "Proveedores obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = false,
                    Message = "Error al obtener los proveedores",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<ProveedorDto>> GetProveedorByIdAsync(int id)
        {
            try
            {
                var proveedor = await _repository.GetByIdAsync(id);
                if (proveedor == null)
                {
                    return new BaseResponseDto<ProveedorDto>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado"
                    };
                }

                return new BaseResponseDto<ProveedorDto>
                {
                    Success = true,
                    Data = MapToDto(proveedor),
                    Message = "Proveedor encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProveedorDto>
                {
                    Success = false,
                    Message = "Error al buscar el proveedor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<ProveedorDto>> CreateProveedorAsync(CreateProveedorDto createDto)
        {
            try
            {
                var proveedor = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(proveedor);

                return new BaseResponseDto<ProveedorDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Proveedor creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProveedorDto>
                {
                    Success = false,
                    Message = "Error al crear el proveedor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<ProveedorDto>> UpdateProveedorAsync(UpdateProveedorDto updateDto)
        {
            try
            {
                var existingProveedor = await _repository.GetByIdAsync(updateDto.IdProveedor);
                if (existingProveedor == null)
                {
                    return new BaseResponseDto<ProveedorDto>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingProveedor);
                await _repository.UpdateAsync(existingProveedor);

                return new BaseResponseDto<ProveedorDto>
                {
                    Success = true,
                    Data = MapToDto(existingProveedor),
                    Message = "Proveedor actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProveedorDto>
                {
                    Success = false,
                    Message = "Error al actualizar el proveedor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<ProveedorDto>>> GetAllProveedoresPaginatedAsync(
            int page,
            int pageSize,
            string? nombre = null,
            string? cuit = null,
            bool? activo = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var proveedores = await _repository.GetAllAsync();
                IEnumerable<Proveedor> filtered = proveedores;

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrim = nombre.Trim().ToLowerInvariant();
                    filtered = filtered.Where(p => (p.NombreProveedor ?? string.Empty).ToLowerInvariant().Contains(nombreTrim));
                }

                if (!string.IsNullOrWhiteSpace(cuit))
                {
                    var cuitTrim = cuit.Trim();
                    filtered = filtered.Where(p => (p.Cuit ?? string.Empty).Contains(cuitTrim));
                }

                if (activo.HasValue)
                {
                    filtered = filtered.Where(p => p.Activo == activo.Value);
                }

                var totalRecords = filtered.Count();
                var proveedoresPage = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var proveedorDtos = proveedoresPage.Select(MapToDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<ProveedorDto>
                {
                    Data = proveedorDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<ProveedorDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Proveedores obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<ProveedorDto>>
                {
                    Success = false,
                    Message = "Error al obtener los proveedores",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ProveedorDto>>> GetAllComboAsync(string? search = null)
        {
            try
            {
                var proveedores = await _repository.GetAllAsync();
                var filteredProveedores = proveedores
                    .Where(p => string.IsNullOrWhiteSpace(search) ||
                        (p.NombreProveedor != null && p.NombreProveedor.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Contacto != null && p.Contacto.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .Take(15)
                    .ToList();

                var proveedorDtos = filteredProveedores.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = true,
                    Data = proveedorDtos,
                    Message = "Proveedores obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = false,
                    Message = "Error al obtener los proveedores",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private ProveedorDto MapToDto(Proveedor proveedor)
        {
            return new ProveedorDto
            {
                IdProveedor = proveedor.IdProveedor,
                NombreProveedor = proveedor.NombreProveedor,
                Cuit = proveedor.Cuit,
                Contacto = proveedor.Contacto,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Direccion = proveedor.Direccion,
                Activo = proveedor.Activo
            };
        }

        private Proveedor MapFromCreateDto(CreateProveedorDto createDto)
        {
            return new Proveedor
            {
                NombreProveedor = createDto.NombreProveedor,
                Cuit = createDto.Cuit,
                Contacto = createDto.Contacto,
                Telefono = createDto.Telefono,
                Email = createDto.Email,
                Direccion = createDto.Direccion,
                Activo = createDto.Activo
            };
        }

        private void MapFromUpdateDto(UpdateProveedorDto updateDto, Proveedor proveedor)
        {
            proveedor.NombreProveedor = updateDto.NombreProveedor;
            proveedor.Cuit = updateDto.Cuit;
            proveedor.Contacto = updateDto.Contacto;
            proveedor.Telefono = updateDto.Telefono;
            proveedor.Email = updateDto.Email;
            proveedor.Direccion = updateDto.Direccion;
            proveedor.Activo = updateDto.Activo;
        }
    }
}
