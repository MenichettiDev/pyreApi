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