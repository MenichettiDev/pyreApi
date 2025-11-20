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
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessHerramientaAlertsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar alertas de herramientas");
                }

                await Task.Delay(_period, stoppingToken);
            }
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

            foreach (var herramienta in herramientasActivas)
            {
                await ProcessHerramientaAlert(herramienta, movimientoRepository, alertaService, alertaRepository);
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
                return;
            }

            // Verificar si el movimiento está vencido
            var fechaVencimiento = ultimoMovimiento.FechaEstimadaDevolucion.Value;
            var fechaActual = DateTime.UtcNow;

            if (fechaActual > fechaVencimiento)
            {
                // Verificar si ya existe una alerta activa para este movimiento
                var alertaExistente = await alertaRepository.FindAsync(a =>
                    a.IdMovimiento == ultimoMovimiento.IdMovimiento &&
                    a.IdTipoAlerta == 2 &&
                    a.Activo);

                if (!alertaExistente.Any())
                {
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
                        _logger.LogInformation("Alerta de vencimiento creada para herramienta {HerramientaId}, movimiento {MovimientoId}",
                            herramienta.IdHerramienta, ultimoMovimiento.IdMovimiento);
                    }
                    else
                    {
                        _logger.LogWarning("Error al crear alerta para herramienta {HerramientaId}: {Error}",
                            herramienta.IdHerramienta, result.Message);
                    }
                }
            }
        }
    }
}
