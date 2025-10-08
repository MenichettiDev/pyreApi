using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pyreApi.DTOs.Alerta;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    /// <summary>
    /// Servicio en segundo plano que se ejecuta diariamente para generar alertas de herramientas
    /// basadas en su fecha de vencimiento y estado de disponibilidad.
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

        /// <summary>
        /// Método principal que ejecuta el proceso de verificación de alertas de manera continua.
        /// Se ejecuta cada 24 horas hasta que se cancele el servicio.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessHerramientaAlertsAsync();
                    // _logger.LogInformation("Proceso de alertas de herramientas completado a las {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar alertas de herramientas");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        /// <summary>
        /// Procesa todas las herramientas activas que están en estado Prestada (2) o Mantenimiento (3)
        /// para verificar si requieren alertas por vencimiento.
        /// </summary>
        private async Task ProcessHerramientaAlertsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var herramientaRepository = scope.ServiceProvider.GetRequiredService<GenericRepository<Herramienta>>();
            var movimientoRepository = scope.ServiceProvider.GetRequiredService<GenericRepository<MovimientoHerramienta>>();
            var alertaService = scope.ServiceProvider.GetRequiredService<AlertaService>();
            var alertaRepository = scope.ServiceProvider.GetRequiredService<GenericRepository<Alerta>>();

            // Obtener herramientas activas que están en estado Prestada (2) o Mantenimiento (3)
            // Solo estas necesitan seguimiento de vencimiento
            var herramientasActivas = await herramientaRepository.FindAsync(h =>
                h.Activo && (h.IdDisponibilidad == 2 || h.IdDisponibilidad == 3));

            // _logger.LogInformation("Procesando {Count} herramientas activas en estado Prestada o Mantenimiento", herramientasActivas.Count());

            foreach (var herramienta in herramientasActivas)
            {
                await ProcessHerramientaAlert(herramienta, movimientoRepository, alertaService, alertaRepository);
            }
        }

        /// <summary>
        /// Evalúa una herramienta específica para determinar si necesita generar alertas.
        /// Usa la fecha de devolución probable del último movimiento para calcular el vencimiento.
        /// </summary>
        /// <param name="herramienta">La herramienta a evaluar</param>
        /// <param name="movimientoRepository">Repositorio para obtener movimientos</param>
        /// <param name="alertaService">Servicio para crear alertas</param>
        /// <param name="alertaRepository">Repositorio para consultar alertas existentes</param>
        private async Task ProcessHerramientaAlert(
            Herramienta herramienta,
            GenericRepository<MovimientoHerramienta> movimientoRepository,
            AlertaService alertaService,
            GenericRepository<Alerta> alertaRepository)
        {
            // Obtener el último movimiento de la herramienta que tenga fecha estimada de devolución
            var ultimoMovimiento = (await movimientoRepository.FindAsync(m =>
                m.IdHerramienta == herramienta.IdHerramienta &&
                m.FechaEstimadaDevolucion.HasValue))
                .OrderByDescending(m => m.Fecha)
                .FirstOrDefault();

            if (ultimoMovimiento?.FechaEstimadaDevolucion == null)
            {
                // _logger.LogInformation("Herramienta {HerramientaId} no tiene movimientos con fecha estimada de devolución",
                // herramienta.IdHerramienta);
                return;
            }

            // La fecha de vencimiento es la fecha estimada de devolución del último movimiento
            var fechaVencimiento = ultimoMovimiento.FechaEstimadaDevolucion.Value;

            // La fecha de warning es la fecha de vencimiento menos los días de alerta configurados
            var fechaWarning = fechaVencimiento.AddDays(-herramienta.DiasAlerta);

            // Calcular días restantes hasta el vencimiento
            var diasRestantesVencimiento = (fechaVencimiento - DateTime.UtcNow).Days;
            var diasRestantesWarning = (fechaWarning - DateTime.UtcNow).Days;

            // _logger.LogInformation("Herramienta {HerramientaId}: Vencimiento={FechaVencimiento}, Warning={FechaWarning}, DíasRestantesVencimiento={DiasVencimiento}",
            //     herramienta.IdHerramienta, fechaVencimiento, fechaWarning, diasRestantesVencimiento);
            // _logger.LogInformation("Herramienta {HerramientaId}: FechaVencimiento={FechaVencimiento}, FechaActual={FechaActual}, DíasRestantes={DiasRestantes}",
            //     herramienta.IdHerramienta, fechaVencimiento, DateTime.UtcNow, diasRestantesVencimiento);


            // Verificar si ya existe una alerta no leída para esta herramienta
            var alertaExistente = await alertaRepository.FindAsync(a =>
                a.IdHerramienta == herramienta.IdHerramienta && !a.Leida);

            // Lógica de generación de alertas basada en fechas
            if (diasRestantesVencimiento <= 0)
            {
                //             _logger.LogInformation("Herramienta {HerramientaId} está VENCIDA - días restantes: {Dias}",
                //    herramienta.IdHerramienta, diasRestantesVencimiento);
                // Caso 1: Herramienta vencida (fecha de devolución ya pasó)
                // Generar alerta tipo 2 (Vencido)
                await CreateAlertIfNotExists(alertaExistente, herramienta.IdHerramienta, 2, alertaService, "vencida");
            }
            else if (diasRestantesWarning <= 0 && diasRestantesVencimiento > 0)
            {
                // Caso 2: Herramienta en período de warning (entre fecha warning y fecha vencimiento)
                // Generar alerta tipo 1 (Próximo a vencer)
                // Solo si el período total es mayor a los días de alerta (para evitar alertas en préstamos muy cortos)
                var diasTotalPrestamo = (fechaVencimiento - ultimoMovimiento.Fecha).Days;
                if (diasTotalPrestamo > herramienta.DiasAlerta)
                {
                    await CreateAlertIfNotExists(alertaExistente, herramienta.IdHerramienta, 1, alertaService, "próxima a vencer");
                }
            }
            // Caso 3: Herramienta aún no en período de warning - No requiere alerta
        }

        /// <summary>
        /// Crea una nueva alerta si no existe ya una del mismo tipo para la herramienta.
        /// Evita duplicar alertas del mismo tipo para la misma herramienta.
        /// </summary>
        /// <param name="alertasExistentes">Alertas no leídas existentes para la herramienta</param>
        /// <param name="idHerramienta">ID de la herramienta</param>
        /// <param name="tipoAlerta">Tipo de alerta a crear (1=Próximo a vencer, 2=Vencido)</param>
        /// <param name="alertaService">Servicio para crear la alerta</param>
        /// <param name="descripcionEstado">Descripción del estado para logging</param>
        private async Task CreateAlertIfNotExists(
            IEnumerable<Alerta> alertasExistentes,
            int idHerramienta,
            int tipoAlerta,
            AlertaService alertaService,
            string descripcionEstado)
        {
            // Verificar si ya existe una alerta del mismo tipo para esta herramienta
            var alertaDelTipoExiste = alertasExistentes.Any(a => a.IdTipoAlerta == tipoAlerta);

            if (!alertaDelTipoExiste)
            {
                var createAlertaDto = new CreateAlertaDto
                {
                    IdHerramienta = idHerramienta,
                    IdTipoAlerta = tipoAlerta
                };

                var result = await alertaService.CreateAlertaAsync(createAlertaDto);

                if (result.Success)
                {
                    // _logger.LogInformation("Alerta creada para herramienta {HerramientaId} ({Estado}), tipo {TipoAlerta}",
                    //     idHerramienta, descripcionEstado, tipoAlerta);
                }
                else
                {
                    _logger.LogWarning("Error al crear alerta para herramienta {HerramientaId}: {Error}",
                        idHerramienta, result.Message);
                }
            }
            else
            {
                _logger.LogInformation("Alerta tipo {TipoAlerta} ya existe para herramienta {HerramientaId}",
                    tipoAlerta, idHerramienta);
            }
        }
    }
}
