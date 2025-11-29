using pyreApi.DTOs.Cliente;
using pyreApi.DTOs.Common;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class ClienteService : GenericService<Cliente>
    {
        public ClienteService(GenericRepository<Cliente> repository)
            : base(repository) { }

        public async Task<BaseResponseDto<IEnumerable<ClienteDto>>> GetAllClientesAsync()
        {
            try
            {
                var clientes = await _repository.GetAllAsync();
                var clienteDtos = clientes.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ClienteDto>>
                {
                    Success = true,
                    Data = clienteDtos,
                    Message = "Clientes obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ClienteDto>>
                {
                    Success = false,
                    Message = "Error al obtener los clientes",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<ClienteDto>> GetClienteByIdAsync(int id)
        {
            try
            {
                var cliente = await _repository.GetByIdAsync(id);
                if (cliente == null)
                {
                    return new BaseResponseDto<ClienteDto>
                    {
                        Success = false,
                        Message = "Cliente no encontrado",
                    };
                }

                return new BaseResponseDto<ClienteDto>
                {
                    Success = true,
                    Data = MapToDto(cliente),
                    Message = "Cliente encontrado",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ClienteDto>
                {
                    Success = false,
                    Message = "Error al buscar el cliente",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<ClienteDto>> CreateClienteAsync(
            CreateClienteDto createDto
        )
        {
            try
            {
                var cliente = MapFromCreateDto(createDto);
                cliente.FechaRegistro = DateTime.UtcNow;
                var result = await _repository.AddAsync(cliente);

                return new BaseResponseDto<ClienteDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Cliente creado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ClienteDto>
                {
                    Success = false,
                    Message = "Error al crear el cliente",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<ClienteDto>> UpdateClienteAsync(
            UpdateClienteDto updateDto
        )
        {
            try
            {
                var existingCliente = await _repository.GetByIdAsync(updateDto.IdCliente);
                if (existingCliente == null)
                {
                    return new BaseResponseDto<ClienteDto>
                    {
                        Success = false,
                        Message = "Cliente no encontrado",
                    };
                }

                MapFromUpdateDto(updateDto, existingCliente);
                await _repository.UpdateAsync(existingCliente);

                return new BaseResponseDto<ClienteDto>
                {
                    Success = true,
                    Data = MapToDto(existingCliente),
                    Message = "Cliente actualizado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ClienteDto>
                {
                    Success = false,
                    Message = "Error al actualizar el cliente",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<PaginatedResponseDto<ClienteDto>>
        > GetAllClientesPaginatedAsync(
            int page,
            int pageSize,
            string? nombre = null,
            string? cuit = null,
            bool? activo = null
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (pageSize <= 0)
                    pageSize = 10;

                var clientes = await _repository.GetAllAsync();
                IEnumerable<Cliente> filtered = clientes;

                filtered = filtered.Where(c => c.Eliminado == false);

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrim = nombre.Trim().ToLowerInvariant();
                    filtered = filtered.Where(c =>
                        (c.Nombre ?? string.Empty).ToLowerInvariant().Contains(nombreTrim)
                    );
                }

                if (!string.IsNullOrWhiteSpace(cuit))
                {
                    var cuitTrim = cuit.Trim();
                    filtered = filtered.Where(c => (c.Cuit ?? string.Empty).Contains(cuitTrim));
                }

                if (activo.HasValue)
                {
                    filtered = filtered.Where(c => c.Activo == activo.Value);
                }

                var totalRecords = filtered.Count();
                var clientesPage = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var clienteDtos = clientesPage.Select(MapToDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<ClienteDto>
                {
                    Data = clienteDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                };

                return new BaseResponseDto<PaginatedResponseDto<ClienteDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Clientes obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<ClienteDto>>
                {
                    Success = false,
                    Message = "Error al obtener los clientes",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ClienteDto>>> GetAllComboAsync(
            string? search = null
        )
        {
            try
            {
                var clientes = await _repository.GetAllAsync();
                var filteredClientes = clientes
                    .Where(c =>
                        c.Activo && !c.Eliminado
                        && (
                            string.IsNullOrWhiteSpace(search)
                            || (
                                c.Nombre != null
                                && c.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase)
                            )
                            || (
                                c.Cuit != null
                                && c.Cuit.Contains(search, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                    )
                    .Take(15)
                    .ToList();

                var clienteDtos = filteredClientes.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ClienteDto>>
                {
                    Success = true,
                    Data = clienteDtos,
                    Message = "Clientes obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ClienteDto>>
                {
                    Success = false,
                    Message = "Error al obtener los clientes",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsyncLogico(int id)
        {
            try
            {
                var cliente = await _repository.GetByIdAsync(id);
                if (cliente == null || cliente.Eliminado)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Cliente no encontrado",
                    };
                }

                // Eliminación lógica
                cliente.Eliminado = true;
                await _repository.UpdateAsync(cliente);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Cliente eliminado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar el cliente",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        private ClienteDto MapToDto(Cliente cliente)
        {
            return new ClienteDto
            {
                IdCliente = cliente.IdCliente,
                Cuit = cliente.Cuit,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Activo = cliente.Activo,
                FechaRegistro = cliente.FechaRegistro,
            };
        }

        private Cliente MapFromCreateDto(CreateClienteDto createDto)
        {
            return new Cliente
            {
                Cuit = createDto.Cuit,
                Nombre = createDto.Nombre,
                Telefono = createDto.Telefono,
                Email = createDto.Email,
                Direccion = createDto.Direccion,
                Activo = createDto.Activo,
            };
        }

        private void MapFromUpdateDto(UpdateClienteDto updateDto, Cliente cliente)
        {
            cliente.Cuit = updateDto.Cuit;
            cliente.Nombre = updateDto.Nombre;
            cliente.Telefono = updateDto.Telefono;
            cliente.Email = updateDto.Email;
            cliente.Direccion = updateDto.Direccion;
            cliente.Activo = updateDto.Activo;
        }
    }
}
