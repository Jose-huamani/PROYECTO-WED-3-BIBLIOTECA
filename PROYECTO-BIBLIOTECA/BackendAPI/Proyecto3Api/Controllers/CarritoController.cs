using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.DTOs;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CarritoController : ControllerBase
{
    private readonly AppDbContext _db;

    public CarritoController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<CarritoDto>> GetMiCarrito(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        var carrito = await _db.Carritos
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Libro)
            .FirstOrDefaultAsync(c => c.UsuarioId == uid.Value, ct);

        if (carrito == null)
        {
            return Ok(new CarritoDto { Items = new() });
        }

        var dto = new CarritoDto
        {
            Id = carrito.Id,
            Items = carrito.Detalles.Select(d => new CarritoItemDto
            {
                Id = d.Id,
                LibroId = d.LibroId,
                Titulo = d.Libro.Titulo,
                PrecioUnitario = d.Libro.Precio,
                Cantidad = d.Cantidad
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("agregar/{libroId:int}")]
    public async Task<IActionResult> Agregar(int libroId, [FromQuery] int cantidad = 1, CancellationToken ct = default)
    {
        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        if (cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0.");

        var libro = await _db.Libros.FirstOrDefaultAsync(l => l.Id == libroId, ct);
        if (libro == null) return NotFound("Libro no encontrado.");
        if (libro.CantidadDisponible < cantidad) return BadRequest("No hay suficiente stock disponible.");

        var carrito = await _db.Carritos
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.UsuarioId == uid.Value, ct);

        if (carrito == null)
        {
            carrito = new Carrito { UsuarioId = uid.Value, FechaCreacion = DateTime.UtcNow };
            _db.Carritos.Add(carrito);
            await _db.SaveChangesAsync(ct);
        }

        var detalle = carrito.Detalles.FirstOrDefault(d => d.LibroId == libroId);
        if (detalle != null)
        {
            if (libro.CantidadDisponible < (detalle.Cantidad + cantidad))
                return BadRequest("No hay suficiente stock disponible para la nueva cantidad total.");
            
            detalle.Cantidad += cantidad;
        }
        else
        {
            _db.DetalleCarritos.Add(new DetalleCarrito
            {
                CarritoId = carrito.Id,
                LibroId = libroId,
                Cantidad = cantidad
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Añadido al carrito" });
    }

    [HttpDelete("remover/{detalleId:int}")]
    public async Task<IActionResult> Remover(int detalleId, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        var detalle = await _db.DetalleCarritos
            .Include(d => d.Carrito)
            .FirstOrDefaultAsync(d => d.Id == detalleId && d.Carrito.UsuarioId == uid.Value, ct);

        if (detalle == null) return NotFound();

        _db.DetalleCarritos.Remove(detalle);
        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Item removido" });
    }

    [HttpDelete("vaciar")]
    public async Task<IActionResult> Vaciar(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        var carrito = await _db.Carritos
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.UsuarioId == uid.Value, ct);

        if (carrito != null)
        {
            _db.DetalleCarritos.RemoveRange(carrito.Detalles);
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new { mensaje = "Carrito vaciado" });
    }
}
