using pyreApi.DTOs.Common;
using pyreApi.DTOs.Obra;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
#pragma warning disable CS8601 // Posible asignación de referencia nula
    public class ObraService : GenericService<Obra>
    {
        private readonly GenericRepository<Cliente> _clienteRepository;
        private readonly GenericRepository<Obra> _obraRepository;

        public ObraService(
            GenericRepository<Obra> repository,
            GenericRepository<Cliente> clienteRepository
        )
            : base(repository)
        {
            _clienteRepository =
                clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _obraRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<BaseResponseDto<IEnumerable<ObraDto>>> GetAllObrasAsync()
        {
            try
            {
                var obras = (await _repository.GetAllAsync()).ToList();

                // Ordenar de la más nueva a la más vieja por FechaInicio
                obras = obras.OrderByDescending(o => o.FechaInicio).ToList();

                await PopulateClientesAsync(obras);
                var obraDtos = obras.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ObraDto>>
                {
                    Success = true,
                    Data = obraDtos,
                    Message = "Obras obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ObraDto>>
                {
                    Success = false,
                    Message = "Error al obtener las obras",
                    Errors = new List<string> { ex.Message },
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
                        Message = "Obra no encontrada",
                    };
                }

                obra.Cliente = await _clienteRepository.GetByIdAsync(obra.IdCliente);

                return new BaseResponseDto<ObraDto>
                {
                    Success = true,
                    Data = MapToDto(obra),
                    Message = "Obra encontrada",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ObraDto>
                {
                    Success = false,
                    Message = "Error al buscar la obra",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<ObraDto>> CreateObraAsync(CreateObraDto createDto)
        {
            try
            {
                var obra = new Obra
                {
                    IdCliente = createDto.IdCliente,
                    Codigo = createDto.Codigo,
                    NombreObra = createDto.NombreObra,
                    Ubicacion = createDto.Ubicacion,
                    Descripcion = createDto.Descripcion,
                    FechaInicio = createDto.FechaInicio,
                    FechaFin = createDto.FechaFin,
                    Activo = createDto.Activo,
                };
                var result = await _repository.AddAsync(obra);

                return new BaseResponseDto<ObraDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Obra creada correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ObraDto>
                {
                    Success = false,
                    Message = "Error al crear la obra",
                    Errors = new List<string> { ex.Message },
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
                        Message = "Obra no encontrada",
                    };
                }

                existingObra.IdCliente = updateDto.IdCliente;
                existingObra.Codigo = updateDto.Codigo;
                existingObra.NombreObra = updateDto.NombreObra;
                existingObra.Ubicacion = updateDto.Ubicacion;
                existingObra.Descripcion = updateDto.Descripcion;
                existingObra.FechaInicio = updateDto.FechaInicio;
                existingObra.FechaFin = updateDto.FechaFin;
                existingObra.Activo = updateDto.Activo;
                await _repository.UpdateAsync(existingObra);

                return new BaseResponseDto<ObraDto>
                {
                    Success = true,
                    Data = MapToDto(existingObra),
                    Message = "Obra actualizada correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<ObraDto>
                {
                    Success = false,
                    Message = "Error al actualizar la obra",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<ObraDto>>> GetAllObrasPaginatedAsync(
            int page,
            int pageSize,
            string? nombre = null,
            string? ubicacion = null,
            string? codigo = null,
            int? idCliente = null,
            bool? activo = null
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (pageSize <= 0)
                    pageSize = 10;

                var obras = (await _repository.GetAllAsync()).ToList();
                await PopulateClientesAsync(obras);
                IEnumerable<Obra> filtered = obras;

                filtered = filtered.Where(o => o.Eliminado == false);

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var nombreTrim = nombre.Trim().ToLowerInvariant();
                    filtered = filtered.Where(o =>
                        (o.NombreObra ?? string.Empty).ToLowerInvariant().Contains(nombreTrim)
                    );
                }

                if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    var ubicacionTrim = ubicacion.Trim().ToLowerInvariant();
                    filtered = filtered.Where(o =>
                        (o.Ubicacion ?? string.Empty).ToLowerInvariant().Contains(ubicacionTrim)
                    );
                }

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    var codigoTrim = codigo.Trim().ToLowerInvariant();
                    filtered = filtered.Where(o =>
                        (o.Codigo ?? string.Empty).ToLowerInvariant().Contains(codigoTrim)
                    );
                }

                if (idCliente.HasValue)
                {
                    filtered = filtered.Where(o => o.IdCliente == idCliente.Value);
                }

                // Ordenar de la más nueva a la más vieja por FechaInicio antes de paginar
                filtered = filtered.OrderByDescending(o => o.FechaInicio);

                var totalRecords = filtered.Count();
                var obrasPage = filtered
                    .OrderByDescending(o => o.FechaInicio)
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
                    HasPreviousPage = page > 1,
                };

                return new BaseResponseDto<PaginatedResponseDto<ObraDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Obras obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<ObraDto>>
                {
                    Success = false,
                    Message = "Error al obtener las obras",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<ObraDto>>> GetAllComboAsync(
            int? idCliente = null,
            string? search = null
        )
        {
            try
            {
                var obras = await _repository.GetAllAsync();
                var filtered = obras.Where(o => o.Activo && !o.Eliminado).AsEnumerable();

                if (idCliente.HasValue)
                {
                    filtered = filtered.Where(o => o.IdCliente == idCliente.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    filtered = filtered.Where(o =>
                        (
                            !string.IsNullOrWhiteSpace(o.NombreObra)
                            && o.NombreObra.Contains(s, StringComparison.OrdinalIgnoreCase)
                        )
                        || (
                            !string.IsNullOrWhiteSpace(o.Codigo)
                            && o.Codigo.Contains(s, StringComparison.OrdinalIgnoreCase)
                        )
                    );
                }

                // Ordenar de la más nueva a la más vieja por FechaInicio y luego limitar a 5
                var resultList = filtered
                    .OrderByDescending(o => o.FechaInicio)
                    .Take(5) // limitar a 5
                    .ToList();

                var obraDtos = resultList.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<ObraDto>>
                {
                    Success = true,
                    Data = obraDtos,
                    Message = "Obras obtenidas correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<ObraDto>>
                {
                    Success = false,
                    Message = "Error al obtener las obras",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsyncLogico(int id)
        {
            try
            {
                var obra = await _repository.GetByIdAsync(id);
                if (obra == null || obra.Eliminado)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Obra no encontrada",
                    };
                }

                // Eliminación lógica
                obra.Eliminado = true;
                await _repository.UpdateAsync(obra);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Obra eliminada correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar la obra",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        private async Task PopulateClientesAsync(List<Obra> obras)
        {
            var clientes = (await _clienteRepository.GetAllAsync()).ToDictionary(c => c.IdCliente);

            foreach (var obra in obras)
            {
                clientes.TryGetValue(obra.IdCliente, out var cliente);
                obra.Cliente = cliente;
            }
        }

        private ObraDto MapToDto(Obra obra)
        {
            return new ObraDto
            {
                IdObra = obra.IdObra,
                IdCliente = obra.IdCliente,
                ClienteNombre = obra.Cliente?.Nombre,
                Codigo = obra.Codigo,
                NombreObra = obra.NombreObra,
                Descripcion = obra.Descripcion,
                Ubicacion = obra.Ubicacion,
                FechaInicio = obra.FechaInicio,
                FechaFin = obra.FechaFin,
                Activo = obra.Activo,
            };
        }

        private Obra MapFromCreateDto(CreateObraDto createDto)
        {
            return new Obra
            {
                IdCliente = createDto.IdCliente,
                Codigo = createDto.Codigo,
                NombreObra = createDto.NombreObra,
                Ubicacion = createDto.Ubicacion,
                Descripcion = createDto.Descripcion,
                FechaInicio = createDto.FechaInicio,
                FechaFin = createDto.FechaFin,
                Activo = createDto.Activo,
            };
        }

        private void MapFromUpdateDto(UpdateObraDto updateDto, Obra obra)
        {
            obra.IdCliente = updateDto.IdCliente;
            obra.Codigo = updateDto.Codigo;
            obra.NombreObra = updateDto.NombreObra;
            obra.Ubicacion = updateDto.Ubicacion;
            obra.Descripcion = updateDto.Descripcion;
            obra.FechaInicio = updateDto.FechaInicio;
            obra.FechaFin = updateDto.FechaFin;
            obra.Activo = updateDto.Activo;
        }
    }
#pragma warning restore CS8601
}
