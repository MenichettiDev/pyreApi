using pyreApi.DTOs.Common;
using pyreApi.DTOs.Herramienta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class HerramientaService : GenericService<Herramienta>
    {
        private readonly HerramientaRepository _herramientaRepository;

        public HerramientaService(HerramientaRepository repository) : base(repository)
        {
            _herramientaRepository = repository;
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetAllHerramientasAsync()
        {
            try
            {
                var herramientas = await _repository.GetAllAsync();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> GetHerramientaByIdAsync(int id)
        {
            try
            {
                var herramienta = await _repository.GetByIdAsync(id);
                if (herramienta == null)
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(herramienta),
                    Message = "Herramienta encontrada"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al buscar la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> CreateHerramientaAsync(CreateHerramientaDto createDto)
        {
            try
            {
                var herramienta = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(herramienta);

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Herramienta creada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al crear la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> UpdateHerramientaAsync(UpdateHerramientaDto updateDto)
        {
            try
            {
                var existingHerramienta = await _repository.GetByIdAsync(updateDto.IdHerramienta);
                if (existingHerramienta == null)
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                MapFromUpdateDto(updateDto, existingHerramienta);
                await _repository.UpdateAsync(existingHerramienta);

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingHerramienta),
                    Message = "Herramienta actualizada correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetAvailableToolsAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAvailableToolsAsync();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas disponibles obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas disponibles",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetByFamiliaAsync(int familiaId)
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByFamiliaAsync(familiaId);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por familia obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por familia",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PagedResponseDto<HerramientaDto>>> GetPagedAsync(PagedRequestDto request)
        {
            try
            {
                var allHerramientas = await _herramientaRepository.GetAllAsync();

                if (!string.IsNullOrEmpty(request.Search))
                {
                    allHerramientas = allHerramientas.Where(h =>
                        h.NombreHerramienta.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                        h.Codigo.Contains(request.Search, StringComparison.OrdinalIgnoreCase));
                }

                var totalRecords = allHerramientas.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                var pagedHerramientas = allHerramientas
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);

                var herramientaDtos = pagedHerramientas.Select(MapToDto);

                var pagedResponse = new PagedResponseDto<HerramientaDto>
                {
                    Data = herramientaDtos,
                    TotalRecords = totalRecords,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = request.Page < totalPages,
                    HasPreviousPage = request.Page > 1
                };

                return new BaseResponseDto<PagedResponseDto<HerramientaDto>>
                {
                    Success = true,
                    Data = pagedResponse,
                    Message = "Herramientas paginadas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PagedResponseDto<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas paginadas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<HerramientaDto>> UpdateStatusAsync(UpdateStatusDto updateStatusDto)
        {
            try
            {
                var existingHerramienta = await _repository.GetByIdAsync(updateStatusDto.IdHerramienta);
                if (existingHerramienta == null)
                {
                    return new BaseResponseDto<HerramientaDto>
                    {
                        Success = false,
                        Message = "Herramienta no encontrada"
                    };
                }

                existingHerramienta.Activo = updateStatusDto.Activo;
                await _repository.UpdateAsync(existingHerramienta);

                return new BaseResponseDto<HerramientaDto>
                {
                    Success = true,
                    Data = MapToDto(existingHerramienta),
                    Message = "Estado de la herramienta actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<HerramientaDto>
                {
                    Success = false,
                    Message = "Error al actualizar el estado de la herramienta",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetByEstadoFisicoAsync(int estadoFisicoId)
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByEstadoAsync(estadoFisicoId);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por estado físico obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por estado físico",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetInRepairAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetInRepairAsync();
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas en reparación obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas en reparación",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                int total = herramientas.Count();

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = total,
                    Message = "Total de herramientas obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasDisponiblesAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                // Ajusta el valor de IdDisponibilidad según tu lógica de disponibilidad
                int totalDisponibles = herramientas.Count(h => h.IdDisponibilidad == 1);

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalDisponibles,
                    Message = "Total de herramientas disponibles obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas disponibles",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasEnPrestamoAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                // Ajusta el valor de IdDisponibilidad según tu lógica de préstamo
                int totalEnPrestamo = herramientas.Count(h => h.IdDisponibilidad == 2);

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalEnPrestamo,
                    Message = "Total de herramientas en préstamo obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas en préstamo",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetTotalHerramientasEnReparacionAsync()
        {
            try
            {
                var herramientas = await _herramientaRepository.GetAllAsync();
                // Ajusta el valor de IdDisponibilidad según tu lógica de reparación
                int totalEnReparacion = herramientas.Count(h => h.IdDisponibilidad == 3);

                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = totalEnReparacion,
                    Message = "Total de herramientas en reparación obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error al obtener el total de herramientas en reparación",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<HerramientaDto>>> GetByDisponibilidadAsync(int disponibilidadId)
        {
            try
            {
                var herramientas = await _herramientaRepository.GetByDisponibilidadAsync(disponibilidadId);
                var herramientaDtos = herramientas.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = true,
                    Data = herramientaDtos,
                    Message = "Herramientas por estado de disponibilidad obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<HerramientaDto>>
                {
                    Success = false,
                    Message = "Error al obtener las herramientas por estado de disponibilidad",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private HerramientaDto MapToDto(Herramienta herramienta)
        {
            return new HerramientaDto
            {
                IdHerramienta = herramienta.IdHerramienta,
                Codigo = herramienta.Codigo,
                NombreHerramienta = herramienta.NombreHerramienta,
                IdFamilia = herramienta.IdFamilia,
                Tipo = herramienta.Tipo,
                Marca = herramienta.Marca,
                Serie = herramienta.Serie,
                FechaDeIngreso = herramienta.FechaDeIngreso,
                CostoDolares = herramienta.CostoDolares,
                UbicacionFisica = herramienta.UbicacionFisica,
                IdEstadoFisico = herramienta.IdEstadoFisico,
                IdPlanta = herramienta.IdPlanta,
                Ubicacion = herramienta.Ubicacion,
                Activo = herramienta.Activo,
                IdDisponibilidad = herramienta.IdDisponibilidad,
                NombreFamilia = herramienta.Familia?.NombreFamilia,
                EstadoFisico = herramienta.EstadoFisico?.Descripcion,
                EstadoDisponibilidad = herramienta.EstadoDisponibilidad?.Descripcion,
                NombrePlanta = herramienta.Planta?.NombrePlanta
            };
        }

        private Herramienta MapFromCreateDto(CreateHerramientaDto createDto)
        {
            return new Herramienta
            {
                Codigo = createDto.Codigo,
                NombreHerramienta = createDto.NombreHerramienta,
                IdFamilia = createDto.IdFamilia,
                Tipo = createDto.Tipo,
                Marca = createDto.Marca,
                Serie = createDto.Serie,
                FechaDeIngreso = createDto.FechaDeIngreso,
                CostoDolares = createDto.CostoDolares,
                UbicacionFisica = createDto.UbicacionFisica,
                IdEstadoFisico = createDto.IdEstadoFisico,
                IdPlanta = createDto.IdPlanta,
                Ubicacion = createDto.Ubicacion,
                Activo = createDto.Activo,
                IdDisponibilidad = createDto.IdDisponibilidad
            };
        }

        private void MapFromUpdateDto(UpdateHerramientaDto updateDto, Herramienta herramienta)
        {
            herramienta.Codigo = updateDto.Codigo;
            herramienta.NombreHerramienta = updateDto.NombreHerramienta;
            herramienta.IdFamilia = updateDto.IdFamilia;
            herramienta.Tipo = updateDto.Tipo;
            herramienta.Marca = updateDto.Marca;
            herramienta.Serie = updateDto.Serie;
            herramienta.FechaDeIngreso = updateDto.FechaDeIngreso;
            herramienta.CostoDolares = updateDto.CostoDolares;
            herramienta.UbicacionFisica = updateDto.UbicacionFisica;
            herramienta.IdEstadoFisico = updateDto.IdEstadoFisico;
            herramienta.IdPlanta = updateDto.IdPlanta;
            herramienta.Ubicacion = updateDto.Ubicacion;
            herramienta.Activo = updateDto.Activo;
            herramienta.IdDisponibilidad = updateDto.IdDisponibilidad;
        }
    }
}