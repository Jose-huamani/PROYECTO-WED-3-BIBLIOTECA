using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Enums;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize(Roles = AppRoles.AdminOBibliotecario)]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen(CancellationToken ct)
    {
        var totalLibros = await _db.Libros.CountAsync(ct);
        var ejemplaresDisponibles = await _db.Libros.SumAsync(l => (int?)l.CantidadDisponible ?? 0, ct);
        var usuariosActivos = await _db.Usuarios.CountAsync(u => u.Activo, ct);
        var prestamosActivos = await _db.Prestamos.CountAsync(p => p.Estado == PrestamoEstado.Activo, ct);
        var multasPendientes = await _db.Multas.CountAsync(m => !m.Pagada, ct);

        var topLibros = await _db.DetallePrestamos.AsNoTracking()
            .GroupBy(d => d.LibroId)
            .Select(g => new { LibroId = g.Key, Veces = g.Sum(x => x.Cantidad) })
            .OrderByDescending(x => x.Veces)
            .Take(5)
            .ToListAsync(ct);

        var titulos = new Dictionary<int, string>();
        foreach (var t in topLibros)
        {
            titulos[t.LibroId] = await _db.Libros.AsNoTracking().Where(l => l.Id == t.LibroId)
                .Select(l => l.Titulo).FirstAsync(ct);
        }

        var ranking = await _db.Rankings.AsNoTracking()
            .OrderByDescending(r => r.Puntos)
            .Take(5)
            .Select(r => new { r.UsuarioId, r.Puntos })
            .ToListAsync(ct);

        return Ok(new
        {
            totalLibros,
            ejemplaresDisponibles,
            usuariosActivos,
            prestamosActivos,
            multasPendientes,
            librosMasPrestados = topLibros.Select(x => new { x.LibroId, Titulo = titulos[x.LibroId], x.Veces }),
            topLectores = ranking
        });
    }
}
