using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MultasController : ControllerBase
{
    private readonly AppDbContext _db;

    public MultasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Multa>>> Get(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        if (User.IsInRole(AppRoles.Administrador) || User.IsInRole(AppRoles.Bibliotecario))
        {
            return Ok(await _db.Multas.AsNoTracking()
                .Include(m => m.Prestamo)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.FechaGeneracion)
                .ToListAsync(ct));
        }

        return Ok(await _db.Multas.AsNoTracking()
            .Where(m => m.UsuarioId == uid)
            .Include(m => m.Prestamo)
            .OrderByDescending(m => m.FechaGeneracion)
            .ToListAsync(ct));
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost("{id:int}/pagar")]
    public async Task<IActionResult> MarcarPagada(int id, CancellationToken ct)
    {
        var m = await _db.Multas.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null)
        {
            return NotFound();
        }

        m.Pagada = true;
        m.FechaPago = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
