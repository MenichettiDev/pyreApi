using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pyreApi.DTOs.Alerta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    /// <summary>
    /// Servicio en segundo plano que se ejecuta diariamente para generar alertas de herramientas vencidas
    /// basadas en herramientas en estado Prestada (2) o Mantenimiento (3).
    /// </summary>
    public class HerramientaAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HerramientaAlertBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromDays(1); // Ejecutar diariamente

        public HerramientaAlertBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<HerramientaAlertBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HerramientaAlertBackgroundService iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Iniciando proceso de verificación de alertas de herramientas");
                    await ProcessHerramientaAlertsAsync();
                    _logger.LogInformation("Proceso de verificación de alertas completado");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar alertas de herramientas");
                }

                _logger.LogInformation("Esperando {Delay} antes del próximo ciclo", _period);
                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("HerramientaAlertBackgroundService detenido");
        }

        /// <summary>
        /// Procesa todas las herramientas en estado Prestada (2) o Mantenimiento (3)
        /// para verificar si tienen préstamos o reparaciones vencidos.
        /// </summary>
        private async Task ProcessHerramientaAlertsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var herramientaRepository = scope.ServiceProvider.GetRequiredService<GenericRepository<Herramienta>>();
            var movimientoRepository = scope.ServiceProvider.GetRequiredService<GenericRepository<MovimientoHerramienta>>();
            var alertaService = scope.ServiceProvider.GetRequiredService<AlertaService>();
            var alertaRepository = scope.ServiceProvider.GetRequiredService<GenericRepository<Alerta>>();

            // Obtener herramientas activas que están en estado Prestada (2) o Mantenimiento (3)
            var herramientasActivas = await herramientaRepository.FindAsync(h =>
                h.Activo && (h.IdDisponibilidad == 2 || h.IdDisponibilidad == 3));

            _logger.LogInformation("Encontradas {CantidadHerramientas} herramientas en estado Prestada o Mantenimiento para evaluar",
                herramientasActivas.Count());

            foreach (var herramienta in herramientasActivas)
            {
                try
                {
                    await ProcessHerramientaAlert(herramienta, movimientoRepository, alertaService, alertaRepository);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar alerta para herramienta {HerramientaId}", herramienta.IdHerramienta);
                }
            }
        }

        /// <summary>
        /// Evalúa una herramienta específica para determinar si tiene movimientos vencidos.
        /// Busca el último movimiento tipo Prestamo (1) o Envio Reparacion (3).
        /// </summary>
        private async Task ProcessHerramientaAlert(
            Herramienta herramienta,
            GenericRepository<MovimientoHerramienta> movimientoRepository,
            AlertaService alertaService,
            GenericRepository<Alerta> alertaRepository)
        {
            _logger.LogDebug("Evaluando herramienta {HerramientaId} ({Codigo}) en estado {Estado}",
                herramienta.IdHerramienta, herramienta.Codigo, herramienta.IdDisponibilidad);

            // Obtener el último movimiento de tipo Prestamo (1) o Envio Reparacion (3)
            // que tenga fecha estimada de devolución
            var ultimoMovimiento = (await movimientoRepository.FindAsync(m =>
                m.IdHerramienta == herramienta.IdHerramienta &&
                (m.IdTipoMovimiento == 1 || m.IdTipoMovimiento == 3) &&
                m.FechaEstimadaDevolucion.HasValue))
                .OrderByDescending(m => m.Fecha)
                .FirstOrDefault();

            if (ultimoMovimiento?.FechaEstimadaDevolucion == null)
            {
                _logger.LogDebug("No se encontró movimiento con fecha estimada para herramienta {HerramientaId}",
                    herramienta.IdHerramienta);
                return;
            }

            _logger.LogDebug("Último movimiento para herramienta {HerramientaId}: MovimientoId {MovimientoId}, Tipo {TipoMovimiento}, Fecha estimada {FechaEstimada}",
                herramienta.IdHerramienta, ultimoMovimiento.IdMovimiento, ultimoMovimiento.IdTipoMovimiento, ultimoMovimiento.FechaEstimadaDevolucion);

            // Verificar si el movimiento está vencido
            var fechaVencimiento = ultimoMovimiento.FechaEstimadaDevolucion.Value;
            var fechaActual = DateTime.Now;

            if (fechaActual > fechaVencimiento)
            {
                _logger.LogInformation("Herramienta {HerramientaId} tiene movimiento vencido. Fecha límite: {FechaLimite}, Fecha actual: {FechaActual}",
                    herramienta.IdHerramienta, fechaVencimiento, fechaActual);

                // Verificar si ya existe una alerta activa para este movimiento
                var alertaExistente = await alertaRepository.FindAsync(a =>
                    a.IdMovimiento == ultimoMovimiento.IdMovimiento &&
                    a.IdTipoAlerta == 2 &&
                    a.Activo);

                if (!alertaExistente.Any())
                {
                    _logger.LogInformation("Creando nueva alerta de vencimiento para herramienta {HerramientaId}, movimiento {MovimientoId}",
                        herramienta.IdHerramienta, ultimoMovimiento.IdMovimiento);

                    // Crear alerta de vencimiento
                    var createAlertaDto = new CreateAlertaDto
                    {
                        IdMovimiento = ultimoMovimiento.IdMovimiento,
                        IdTipoAlerta = 2, // Vencido
                        Comentario = null
                    };

                    var result = await alertaService.CreateAlertaAsync(createAlertaDto);

                    if (result.Success)
                    {
                        _logger.LogInformation("Alerta de vencimiento creada exitosamente para herramienta {HerramientaId}, movimiento {MovimientoId}",
                            herramienta.IdHerramienta, ultimoMovimiento.IdMovimiento);
                    }
                    else
                    {
                        _logger.LogWarning("Error al crear alerta para herramienta {HerramientaId}: {Error}",
                            herramienta.IdHerramienta, result.Message);
                    }
                }
                else
                {
                    _logger.LogDebug("Ya existe alerta activa para movimiento {MovimientoId} de herramienta {HerramientaId}",
                        ultimoMovimiento.IdMovimiento, herramienta.IdHerramienta);
                }
            }
            else
            {
                _logger.LogDebug("Herramienta {HerramientaId} no está vencida. Fecha límite: {FechaLimite}",
                    herramienta.IdHerramienta, fechaVencimiento);
            }
        }
    }
}
