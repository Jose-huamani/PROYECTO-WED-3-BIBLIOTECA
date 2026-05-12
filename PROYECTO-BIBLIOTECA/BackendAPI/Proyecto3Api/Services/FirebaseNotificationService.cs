using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Proyecto3Api.Services;

public interface IFirebaseNotificationService
{
    Task<(bool Success, string Message)> SendNotificationAsync(int usuarioId, string titulo, string mensaje, string? tipo = null);
    Task<(bool Success, string Message)> SendNotificationToTokenAsync(string fcmToken, string titulo, string mensaje, string? tipo = null);
}

public class FirebaseNotificationService : IFirebaseNotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly bool _firebaseEnabled;

    public FirebaseNotificationService(AppDbContext db, ILogger<FirebaseNotificationService> logger)
    {
        _db = db;
        _logger = logger;
        
        // Verificamos si la app de Firebase fue inicializada en Program.cs
        _firebaseEnabled = FirebaseApp.DefaultInstance != null;
    }

    public async Task<(bool Success, string Message)> SendNotificationAsync(int usuarioId, string titulo, string mensaje, string? tipo = null)
    {
        try
        {
            // Siempre guardamos la notificación en BD aunque Firebase no esté activo
            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo ?? "General",
                FechaCreacion = DateTime.UtcNow,
                Leida = false
            };
            
            _db.Notificaciones.Add(notificacion);
            await _db.SaveChangesAsync();

            if (!_firebaseEnabled)
            {
                _logger.LogWarning("Firebase no está configurado. Notificación guardada en BD, pero push omitido.");
                return (true, "Notificación guardada (Push omitido por falta de credenciales de Firebase).");
            }

            var tokens = await _db.DeviceTokens
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => t.FcmToken)
                .ToListAsync();

            if (!tokens.Any())
            {
                return (true, "Notificación guardada en BD. Usuario no tiene tokens de dispositivo registrados.");
            }

            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },
                Data = new Dictionary<string, string>
                {
                    { "tipo", tipo ?? "General" },
                    { "notificacionId", notificacion.Id.ToString() }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            
            _logger.LogInformation($"Notificaciones FCM enviadas: {response.SuccessCount} exitosas, {response.FailureCount} fallidas.");
            
            return (true, "Notificación enviada por FCM y guardada en BD.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación push.");
            return (false, $"Error interno: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> SendNotificationToTokenAsync(string fcmToken, string titulo, string mensaje, string? tipo = null)
    {
        if (!_firebaseEnabled)
        {
            return (false, "Firebase no está configurado en el servidor.");
        }

        try
        {
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },
                Data = new Dictionary<string, string>
                {
                    { "tipo", tipo ?? "General" }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return (true, $"Enviado con ID: {response}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar push a token específico.");
            return (false, $"Error: {ex.Message}");
        }
    }
}
