using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriasController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> Get(CancellationToken ct) =>
        Ok(await _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(ct));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> GetById(int id, CancellationToken ct)
    {
        var c = await _db.Categorias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost]
    public async Task<ActionResult<Categoria>> Post(Categoria categoria, CancellationToken ct)
    {
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Categoria categoria, CancellationToken ct)
    {
        if (id != categoria.Id)
        {
            return BadRequest();
        }

        _db.Categorias.Update(categoria);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize(Roles = AppRoles.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var c = await _db.Categorias.FindAsync(new object?[] { id }, ct);
        if (c is null)
        {
            return NotFound();
        }

        if (await _db.Libros.AnyAsync(l => l.CategoriaId == id, ct))
        {
            return Conflict(new { mensaje = "No se puede eliminar: hay libros en esta categoría" });
        }

        _db.Categorias.Remove(c);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
