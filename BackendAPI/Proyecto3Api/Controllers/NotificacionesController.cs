using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificacionesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var rows = await _db.Notificaciones.AsNoTracking()
            .Where(n => n.UsuarioId == uid.Value)
            .OrderByDescending(n => n.FechaCreacion)
            .Take(100)
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpPost("{id:int}/marcar-leida")]
    public async Task<IActionResult> MarcarLeida(int id, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var n = await _db.Notificaciones.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (n is null)
        {
            return NotFound();
        }

        if (n.UsuarioId != uid.Value)
        {
            return Forbid();
        }

        n.Leida = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
