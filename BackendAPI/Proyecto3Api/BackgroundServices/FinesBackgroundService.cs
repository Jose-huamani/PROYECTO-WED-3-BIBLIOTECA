using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proyecto3Api.Configuration;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Domain.Enums;
using Proyecto3Api.Infrastructure.Persistence;
using Proyecto3Api.Services;

namespace Proyecto3Api.BackgroundServices;

public class FinesBackgroundService : BackgroundService
{
    private readonly ILogger<FinesBackgroundService> _logger;
    private readonly IServiceProvider _services;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public FinesBackgroundService(ILogger<FinesBackgroundService> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FinesBackgroundService iniciado. Revisará multas cada 24 horas.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarMultasAutomaticasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar multas automáticas.");
            }

            // Esperar 24 horas para la próxima revisión
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcesarMultasAutomaticasAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<LibrarySettings>>().Value;
        var notificationService = scope.ServiceProvider.GetRequiredService<IFirebaseNotificationService>();

        var hoy = DateTime.UtcNow.Date;

        // Buscar préstamos activos vencidos
        var prestamosRetrasados = await db.Prestamos
            .Include(p => p.Detalles)
            .ThenInclude(d => d.Libro)
            .Where(p => p.Estado == PrestamoEstado.Activo && p.FechaDevolucionEsperada.Date < hoy)
            .ToListAsync(stoppingToken);

        if (!prestamosRetrasados.Any())
        {
            _logger.LogInformation("No hay préstamos con retraso para procesar hoy.");
            return;
        }

        int multasGeneradas = 0;

        foreach (var prestamo in prestamosRetrasados)
        {
            int diasRetraso = (hoy - prestamo.FechaDevolucionEsperada.Date).Days;
            decimal montoMulta = diasRetraso * settings.MultaPorDiaRetraso;

            // Verificar si ya existe una multa para este préstamo hoy (para evitar duplicados si se reinicia el server)
            var existeMulta = await db.Multas.AnyAsync(m => m.PrestamoId == prestamo.Id && m.FechaGeneracion.Date == hoy, stoppingToken);
            
            if (!existeMulta)
            {
                var multa = new Multa
                {
                    PrestamoId = prestamo.Id,
                    UsuarioId = prestamo.UsuarioId,
                    Monto = settings.MultaPorDiaRetraso, // Agregamos la multa diaria
                    Motivo = $"Multa por retraso diario de {diasRetraso} día(s).",
                    FechaGeneracion = DateTime.UtcNow,
                    Pagada = false
                };

                db.Multas.Add(multa);
                multasGeneradas++;

                // Enviar Notificación Push al Usuario
                string libroNombres = string.Join(", ", prestamo.Detalles.Select(d => d.Libro.Titulo));
                string mensaje = $"Tu préstamo de '{libroNombres}' tiene {diasRetraso} días de retraso. Se te ha cobrado {settings.MultaPorDiaRetraso} Bs adicionales de multa el día de hoy.";
                
                await notificationService.SendNotificationAsync(
                    usuarioId: prestamo.UsuarioId, 
                    titulo: "Préstamo Retrasado - Multa Generada", 
                    mensaje: mensaje, 
                    tipo: "Alerta"
                );
            }
        }

        if (multasGeneradas > 0)
        {
            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation($"Se generaron {multasGeneradas} multas nuevas por retraso hoy.");
        }
    }
}
