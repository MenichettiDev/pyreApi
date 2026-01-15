using pyreApi.DTOs.Common;
using pyreApi.DTOs.Proveedor;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class ProveedorService : GenericService<Proveedor>
    {
        private readonly MovimientoHerramientaRepository _movimientoRepository;

        public ProveedorService(
            GenericRepository<Proveedor> repository,
            MovimientoHerramientaRepository movimientoRepository
        )
            : base(repository)
        {
            _movimientoRepository =
                movimientoRepository
                ?? throw new ArgumentNullException(nameof(movimientoRepository));
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
                    Message = "Proveedores obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = false,
                    Message = "Error al obtener los proveedores",
                    Errors = new List<string> { ex.Message },
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
                        Message = "Proveedor no encontrado",
                    };
                }

                return new BaseResponseDto<ProveedorDto>
                {
                    Success = true,
                    Data = MapToDto(proveedor),
                    Message = "Proveedor encontrado",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProveedorDto>
                {
                    Success = false,
                    Message = "Error al buscar el proveedor",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<ProveedorDto>> CreateProveedorAsync(
            CreateProveedorDto createDto
        )
        {
            try
            {
                var proveedor = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(proveedor);

                return new BaseResponseDto<ProveedorDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Proveedor creado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProveedorDto>
                {
                    Success = false,
                    Message = "Error al crear el proveedor",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<ProveedorDto>> UpdateProveedorAsync(
            UpdateProveedorDto updateDto
        )
        {
            try
            {
                var existingProveedor = await _repository.GetByIdAsync(updateDto.IdProveedor);
                if (existingProveedor == null)
                {
                    return new BaseResponseDto<ProveedorDto>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado",
                    };
                }

                MapFromUpdateDto(updateDto, existingProveedor);
                await _repository.UpdateAsync(existingProveedor);

                return new BaseResponseDto<ProveedorDto>
                {
                    Success = true,
                    Data = MapToDto(existingProveedor),
                    Message = "Proveedor actualizado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ProveedorDto>
                {
                    Success = false,
                    Message = "Error al actualizar el proveedor",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<
            BaseResponseDto<PaginatedResponseDto<ProveedorDto>>
        > GetAllProveedoresPaginatedAsync(
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

                var proveedores = await _repository.GetAllAsync();
                IEnumerable<Proveedor> filtered = proveedores;

                filtered = filtered.Where(p => p.Eliminado == false);

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrim = nombre.Trim().ToLowerInvariant();
                    filtered = filtered.Where(p =>
                        (p.NombreProveedor ?? string.Empty).ToLowerInvariant().Contains(nombreTrim)
                    );
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
                var proveedoresPage = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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
                    HasPreviousPage = page > 1,
                };

                return new BaseResponseDto<PaginatedResponseDto<ProveedorDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Proveedores obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<ProveedorDto>>
                {
                    Success = false,
                    Message = "Error al obtener los proveedores",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ProveedorDto>>> GetAllComboAsync(
            string? search = null
        )
        {
            try
            {
                var proveedores = await _repository.GetAllAsync();

                // Aplicar filtro de texto primero
                var candidateProveedores = proveedores
                    .Where(p =>
                        !p.Eliminado
                        && (
                            string.IsNullOrWhiteSpace(search)
                            || (
                                p.NombreProveedor != null
                                && p.NombreProveedor.Contains(
                                    search,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            || (
                                p.Contacto != null
                                && p.Contacto.Contains(search, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                    )
                    .ToList();

                var resultado = new List<Proveedor>();

                // Incluir proveedores activos directamente; incluir inactivos sólo si tienen herramientas en préstamo/reparación asociadas
                foreach (var p in candidateProveedores)
                {
                    if (p.Activo)
                    {
                        resultado.Add(p);
                        continue;
                    }

                    // proveedor inactivo: comprobar movimientos asociados
                    var movimientos = await _movimientoRepository.GetByProveedorAsync(
                        p.IdProveedor
                    );
                    var tieneHerramientasPendientes = movimientos.Any(m =>
                        m.Herramienta != null
                        && (
                            m.Herramienta.IdDisponibilidad == 2
                            || m.Herramienta.IdDisponibilidad == 3
                        )
                    );

                    if (tieneHerramientasPendientes)
                        resultado.Add(p);
                }

                var proveedorDtos = resultado.Take(15).Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = true,
                    Data = proveedorDtos,
                    Message = "Proveedores obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ProveedorDto>>
                {
                    Success = false,
                    Message = "Error al obtener los proveedores",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsyncLogico(int id)
        {
            try
            {
                var proveedor = await _repository.GetByIdAsync(id);
                if (proveedor == null || proveedor.Eliminado)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Proveedor no encontrado",
                    };
                }

                // Eliminación lógica
                proveedor.Eliminado = true;
                await _repository.UpdateAsync(proveedor);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Proveedor eliminado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar el proveedor",
                    Errors = new List<string> { ex.Message },
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
                Descripcion = proveedor.Descripcion,
                Activo = proveedor.Activo,
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
                Descripcion = createDto.Descripcion,
                Activo = createDto.Activo,
            };
        }

        private void MapFromUpdateDto(UpdateProveedorDto updateDto, Proveedor proveedor)
        {
            // Solo actualizar campos que no están vacíos o nulos, preservando los existentes
            if (!string.IsNullOrWhiteSpace(updateDto.NombreProveedor))
                proveedor.NombreProveedor = updateDto.NombreProveedor;

            if (!string.IsNullOrWhiteSpace(updateDto.Contacto))
                proveedor.Contacto = updateDto.Contacto;

            // Para campos nullable, solo actualizar si se proporciona un valor no vacío
            if (!string.IsNullOrWhiteSpace(updateDto.Cuit))
                proveedor.Cuit = updateDto.Cuit;

            if (!string.IsNullOrWhiteSpace(updateDto.Telefono))
                proveedor.Telefono = updateDto.Telefono;

            if (!string.IsNullOrWhiteSpace(updateDto.Email))
                proveedor.Email = updateDto.Email;

            if (!string.IsNullOrWhiteSpace(updateDto.Direccion))
                proveedor.Direccion = updateDto.Direccion;

            if (!string.IsNullOrWhiteSpace(updateDto.Descripcion))
                proveedor.Descripcion = updateDto.Descripcion;

            // El estado activo siempre se actualiza ya que es un bool
            proveedor.Activo = updateDto.Activo;
        }
    }
}
