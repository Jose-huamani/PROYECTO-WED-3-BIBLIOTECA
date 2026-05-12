using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.DTOs;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibrosController : ControllerBase
{
    private readonly AppDbContext _db;

    public LibrosController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Libro>>> GetLibros(CancellationToken ct)
    {
        var data = await AsQueryableWithNavs().AsNoTracking().ToListAsync(ct);
        return Ok(data);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Libro>> GetLibro(int id, CancellationToken ct)
    {
        var libro = await AsQueryableWithNavs().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);
        return libro is null ? NotFound(new { mensaje = "El libro no existe" }) : Ok(libro);
    }

    [AllowAnonymous]
    [HttpGet("qr/{codigo}")]
    public async Task<ActionResult<Libro>> GetByQR(string codigo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return BadRequest(new { mensaje = "El código QR es obligatorio" });
        }

        var libro = await AsQueryableWithNavs().AsNoTracking()
            .FirstOrDefaultAsync(l => l.CodigoQR == codigo, ct);
        return libro is null ? NotFound(new { mensaje = "Código QR no reconocido" }) : Ok(libro);
    }

    [AllowAnonymous]
    [HttpGet("codigo-barras/{codigo}")]
    public async Task<ActionResult<Libro>> GetByCodigoBarras(string codigo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return BadRequest(new { mensaje = "El código de barras es obligatorio" });
        }

        var libro = await AsQueryableWithNavs().AsNoTracking()
            .FirstOrDefaultAsync(l => l.CodigoBarras == codigo, ct);
        return libro is null ? NotFound(new { mensaje = "Código de barras no reconocido" }) : Ok(libro);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost]
    public async Task<ActionResult<Libro>> PostLibro(LibroCreateRequest dto, CancellationToken ct)
    {
        var titulo = dto.Titulo.Trim();
        var isbn = dto.Isbn.Trim();
        var codigoQr = dto.CodigoQR.Trim();
        var codigoBarras = string.IsNullOrWhiteSpace(dto.CodigoBarras) ? null : dto.CodigoBarras.Trim();

        if (!await _db.Autores.AnyAsync(a => a.Id == dto.AutorId, ct))
        {
            return BadRequest(new { mensaje = "El autor seleccionado no existe" });
        }

        if (!await _db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId, ct))
        {
            return BadRequest(new { mensaje = "La categoria seleccionada no existe" });
        }

        if (dto.CantidadDisponible > dto.CantidadTotal)
        {
            return BadRequest(new { mensaje = "La cantidad disponible no puede superar la cantidad total" });
        }

        if (await _db.Libros.AnyAsync(l => l.CodigoQR == codigoQr, ct))
        {
            return Conflict(new { mensaje = "Ya existe un libro con ese Código QR" });
        }

        if (!string.IsNullOrEmpty(codigoBarras) &&
            await _db.Libros.AnyAsync(l => l.CodigoBarras == codigoBarras, ct))
        {
            return Conflict(new { mensaje = "Ya existe un libro con ese código de barras" });
        }

        var libro = new Libro
        {
            Titulo = titulo,
            Isbn = isbn,
            AutorId = dto.AutorId,
            CategoriaId = dto.CategoriaId,
            CodigoQR = codigoQr,
            CodigoBarras = codigoBarras,
            CantidadTotal = dto.CantidadTotal,
            CantidadDisponible = dto.CantidadDisponible,
            Precio = dto.Precio,
            ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim(),
            Introduccion = string.IsNullOrWhiteSpace(dto.Introduccion) ? null : dto.Introduccion.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Editorial = string.IsNullOrWhiteSpace(dto.Editorial) ? null : dto.Editorial.Trim(),
            NumeroPaginas = dto.NumeroPaginas,
            Idioma = string.IsNullOrWhiteSpace(dto.Idioma) ? null : dto.Idioma.Trim(),
            EstadoLibro = string.IsNullOrWhiteSpace(dto.EstadoLibro) ? null : dto.EstadoLibro.Trim()
        };

        _db.Libros.Add(libro);
        await _db.SaveChangesAsync(ct);

        var auditUserId = User.GetUserId();
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = auditUserId,
            Accion = "CrearLibro",
            TipoEntidad = nameof(Libro),
            EntidadId = libro.Id.ToString(),
            Fecha = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync(ct);

        libro = await AsQueryableWithNavs().AsNoTracking().FirstAsync(l => l.Id == libro.Id, ct);
        return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, libro);
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutLibro(int id, LibroCreateRequest dto, CancellationToken ct)
    {
        var libro = await _db.Libros.FindAsync(new object?[] { id }, ct);
        if (libro is null)
        {
            return NotFound(new { mensaje = "El libro no existe" });
        }

        if (!await _db.Autores.AnyAsync(a => a.Id == dto.AutorId, ct))
        {
            return BadRequest(new { mensaje = "El autor seleccionado no existe" });
        }

        if (!await _db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId, ct))
        {
            return BadRequest(new { mensaje = "La categoria seleccionada no existe" });
        }

        if (await _db.Libros.AnyAsync(l => l.CodigoQR == dto.CodigoQR && l.Id != id, ct))
        {
            return Conflict(new { mensaje = "Ya existe otro libro con ese Código QR" });
        }

        if (!string.IsNullOrEmpty(dto.CodigoBarras) &&
            await _db.Libros.AnyAsync(l => l.CodigoBarras == dto.CodigoBarras && l.Id != id, ct))
        {
            return Conflict(new { mensaje = "Ya existe otro libro con ese código de barras" });
        }

        libro.Titulo = dto.Titulo.Trim();
        libro.Isbn = dto.Isbn.Trim();
        libro.AutorId = dto.AutorId;
        libro.CategoriaId = dto.CategoriaId;
        libro.CodigoQR = dto.CodigoQR.Trim();
        libro.CodigoBarras = string.IsNullOrWhiteSpace(dto.CodigoBarras) ? null : dto.CodigoBarras.Trim();
        libro.CantidadTotal = dto.CantidadTotal;
        libro.CantidadDisponible = Math.Min(dto.CantidadDisponible, dto.CantidadTotal);
        libro.Precio = dto.Precio;
        libro.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();
        libro.Introduccion = string.IsNullOrWhiteSpace(dto.Introduccion) ? null : dto.Introduccion.Trim();
        libro.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        libro.Editorial = string.IsNullOrWhiteSpace(dto.Editorial) ? null : dto.Editorial.Trim();
        libro.NumeroPaginas = dto.NumeroPaginas;
        libro.Idioma = string.IsNullOrWhiteSpace(dto.Idioma) ? null : dto.Idioma.Trim();
        libro.EstadoLibro = string.IsNullOrWhiteSpace(dto.EstadoLibro) ? null : dto.EstadoLibro.Trim();

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }


    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLibro(int id, CancellationToken ct)
    {
        var libro = await _db.Libros.FindAsync(new object?[] { id }, ct);
        if (libro is null)
        {
            return NotFound();
        }

        _db.Libros.Remove(libro);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private IQueryable<Libro> AsQueryableWithNavs() =>
        _db.Libros.Include(l => l.Autor).Include(l => l.Categoria);
}
