using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FavoritosController : ControllerBase
{
    private readonly AppDbContext _db;

    public FavoritosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Favorito>>> Get(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        return Ok(await _db.Favoritos.AsNoTracking()
            .Where(f => f.UsuarioId == uid.Value)
            .Include(f => f.Libro).ThenInclude(l => l.Autor)
            .Include(f => f.Libro).ThenInclude(l => l.Categoria)
            .OrderByDescending(f => f.FechaAgregado)
            .ToListAsync(ct));
    }

    [HttpPost("{libroId:int}")]
    public async Task<ActionResult<Favorito>> Agregar(int libroId, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        if (!await _db.Libros.AnyAsync(l => l.Id == libroId, ct))
        {
            return NotFound();
        }

        if (await _db.Favoritos.AnyAsync(f => f.UsuarioId == uid.Value && f.LibroId == libroId, ct))
        {
            return Conflict(new { mensaje = "Ya está en favoritos" });
        }

        var f = new Favorito
        {
            UsuarioId = uid.Value,
            LibroId = libroId,
            FechaAgregado = DateTime.UtcNow
        };
        _db.Favoritos.Add(f);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(f).Reference(x => x.Libro).LoadAsync(ct);
        return Ok(f);
    }

    [HttpDelete("{libroId:int}")]
    public async Task<IActionResult> Quitar(int libroId, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var f = await _db.Favoritos.FirstOrDefaultAsync(x => x.UsuarioId == uid.Value && x.LibroId == libroId, ct);
        if (f is null)
        {
            return NotFound();
        }

        _db.Favoritos.Remove(f);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
