using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;
using Proyecto3Api.Services;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFirebaseNotificationService _notificationService;

    public NotificationsController(AppDbContext db, IFirebaseNotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public class TokenRequest
    {
        public string FcmToken { get; set; } = string.Empty;
        public string? Device { get; set; }
    }

    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterToken([FromBody] TokenRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.FcmToken)) return BadRequest("Token requerido.");

        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        var existing = await _db.DeviceTokens.FirstOrDefaultAsync(t => t.FcmToken == req.FcmToken, ct);
        
        if (existing == null)
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                UsuarioId = uid.Value,
                FcmToken = req.FcmToken,
                Device = req.Device
            });
        }
        else if (existing.UsuarioId != uid.Value)
        {
            // El token ahora pertenece a otro usuario (cambio de cuenta en el mismo celular)
            existing.UsuarioId = uid.Value;
            existing.Device = req.Device;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Token registrado exitosamente." });
    }

    public class SendNotificationRequest
    {
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? Tipo { get; set; }
    }

    [HttpPost("send")]
    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest req)
    {
        var result = await _notificationService.SendNotificationAsync(req.UsuarioId, req.Titulo, req.Mensaje, req.Tipo);
        if (result.Success)
            return Ok(new { message = result.Message });
        
        return BadRequest(new { error = result.Message });
    }

    [HttpGet("user/{id:int?}")]
    public async Task<IActionResult> GetNotificaciones(int? id, CancellationToken ct)
    {
        int targetUid;
        if (User.IsInRole(AppRoles.Administrador) && id.HasValue)
        {
            targetUid = id.Value;
        }
        else
        {
            var uid = User.GetUserId();
            if (uid == null) return Unauthorized();
            targetUid = uid.Value;
        }

        var notificaciones = await _db.Notificaciones
            .Where(n => n.UsuarioId == targetUid)
            .OrderByDescending(n => n.FechaCreacion)
            .Take(50)
            .ToListAsync(ct);

        return Ok(notificaciones);
    }
}
