using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize(Roles = AppRoles.Administrador)]
[ApiController]
[Route("api/[controller]")]
public class AuditoriasController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditoriasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var rows = await _db.Auditorias.AsNoTracking()
            .OrderByDescending(a => a.Fecha)
            .Take(500)
            .ToListAsync(ct);
        return Ok(rows);
    }
}
