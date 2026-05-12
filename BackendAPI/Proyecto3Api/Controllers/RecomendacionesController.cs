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
public class RecomendacionesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RecomendacionesController(AppDbContext db) => _db = db;

    [HttpGet("libros")]
    public async Task<ActionResult<IEnumerable<Libro>>> Libros(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var favLibroIds = await _db.Favoritos.AsNoTracking()
            .Where(f => f.UsuarioId == uid.Value)
            .Select(f => f.LibroId)
            .ToListAsync(ct);

        var catIds = await _db.Libros.AsNoTracking()
            .Where(l => favLibroIds.Contains(l.Id))
            .Select(l => l.CategoriaId)
            .Distinct()
            .ToListAsync(ct);

        if (catIds.Count == 0)
        {
            catIds = await _db.Libros.AsNoTracking().Select(l => l.CategoriaId).Distinct().Take(3)
                .ToListAsync(ct);
        }

        var sugeridos = await _db.Libros.AsNoTracking()
            .Where(l => catIds.Contains(l.CategoriaId) && l.CantidadDisponible > 0)
            .Include(l => l.Autor)
            .Include(l => l.Categoria)
            .OrderByDescending(l => l.CantidadDisponible)
            .Take(12)
            .ToListAsync(ct);

        return Ok(sugeridos);
    }
}
