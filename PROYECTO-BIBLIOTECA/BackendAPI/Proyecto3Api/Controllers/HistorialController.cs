using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HistorialController : ControllerBase
{
    private readonly AppDbContext _db;

    public HistorialController(AppDbContext db) => _db = db;

    [HttpGet("mi-historial")]
    public async Task<IActionResult> MiHistorial(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var rows = await _db.HistorialActividades.AsNoTracking()
            .Where(h => h.UsuarioId == uid.Value)
            .OrderByDescending(h => h.Fecha)
            .Take(200)
            .ToListAsync(ct);
        return Ok(rows);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpGet("todos")]
    public async Task<IActionResult> Todos(CancellationToken ct)
    {
        var rows = await _db.HistorialActividades.AsNoTracking()
            .OrderByDescending(h => h.Fecha)
            .Take(500)
            .ToListAsync(ct);
        return Ok(rows);
    }
}
