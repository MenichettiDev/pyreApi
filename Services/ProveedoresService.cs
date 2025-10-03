using pyreApi.DTOs.Common;
using pyreApi.DTOs.Proveedor;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class ProveedoresService : GenericService<Proveedor>
    {
        public ProveedoresService(GenericRepository<Proveedor> repository) : base(repository)
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

        private ProveedorDto MapToDto(Proveedor proveedor)
        {
            return new ProveedorDto
            {
                IdProveedor = proveedor.IdProveedor,
                NombreProveedor = proveedor.NombreProveedor,
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
            proveedor.Contacto = updateDto.Contacto;
            proveedor.Telefono = updateDto.Telefono;
            proveedor.Email = updateDto.Email;
            proveedor.Direccion = updateDto.Direccion;
            proveedor.Activo = updateDto.Activo;
        }
    }
}
