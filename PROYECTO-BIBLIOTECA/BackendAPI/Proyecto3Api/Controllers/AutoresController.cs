using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoresController : ControllerBase
{
    private readonly AppDbContext _db;

    public AutoresController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Autor>>> Get(CancellationToken ct) =>
        Ok(await _db.Autores.AsNoTracking().OrderBy(a => a.Nombre).ToListAsync(ct));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Autor>> GetById(int id, CancellationToken ct)
    {
        var a = await _db.Autores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost]
    public async Task<ActionResult<Autor>> Post(Autor autor, CancellationToken ct)
    {
        _db.Autores.Add(autor);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = autor.Id }, autor);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Autor autor, CancellationToken ct)
    {
        if (id != autor.Id)
        {
            return BadRequest();
        }

        _db.Autores.Update(autor);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize(Roles = AppRoles.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var a = await _db.Autores.FindAsync(new object?[] { id }, ct);
        if (a is null)
        {
            return NotFound();
        }

        if (await _db.Libros.AnyAsync(l => l.AutorId == id, ct))
        {
            return Conflict(new { mensaje = "No se puede eliminar: hay libros asociados" });
        }

        _db.Autores.Remove(a);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
