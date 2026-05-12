using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingController : ControllerBase
{
    private readonly AppDbContext _db;

    public RankingController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet("tabla")]
    public async Task<IActionResult> Tabla(CancellationToken ct)
    {
        var rows = await _db.Rankings.AsNoTracking()
            .OrderByDescending(r => r.Puntos)
            .Take(50)
            .Join(_db.Usuarios.AsNoTracking(),
                r => r.UsuarioId,
                u => u.Id,
                (r, u) => new { r.UsuarioId, r.Puntos, Nombre = u.NombreCompleto })
            .ToListAsync(ct);
        return Ok(rows);
    }
}
