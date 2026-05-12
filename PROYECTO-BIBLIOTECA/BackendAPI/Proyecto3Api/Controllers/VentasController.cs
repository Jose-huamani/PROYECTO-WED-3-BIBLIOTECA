using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.DTOs;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;

    public VentasController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll(CancellationToken ct)
    {
        var ventas = await _db.Ventas.AsNoTracking()
            .Include(v => v.Usuario)
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => new VentaDto
            {
                Id = v.Id,
                Usuario = v.Usuario.NombreCompleto,
                FechaVenta = v.FechaVenta,
                Total = v.Total,
                MetodoPago = v.MetodoPago
            }).ToListAsync(ct);

        return Ok(ventas);
    }

    [HttpGet("mis-ventas")]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetMisVentas(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        var ventas = await _db.Ventas.AsNoTracking()
            .Where(v => v.UsuarioId == uid.Value)
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => new VentaDto
            {
                Id = v.Id,
                Usuario = v.Usuario.NombreCompleto,
                FechaVenta = v.FechaVenta,
                Total = v.Total,
                MetodoPago = v.MetodoPago
            }).ToListAsync(ct);

        return Ok(ventas);
    }

    [HttpPost("comprar-carrito")]
    public async Task<IActionResult> ComprarCarrito([FromBody] VentaCreateRequest req, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid == null) return Unauthorized();

        var carrito = await _db.Carritos
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Libro)
            .FirstOrDefaultAsync(c => c.UsuarioId == uid.Value, ct);

        if (carrito == null || !carrito.Detalles.Any())
        {
            return BadRequest("El carrito está vacío.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        decimal total = 0;
        var venta = new Venta
        {
            UsuarioId = uid.Value,
            FechaVenta = DateTime.UtcNow,
            MetodoPago = req.MetodoPago,
            ReferenciaPago = req.ReferenciaPago ?? string.Empty
        };

        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync(ct);

        foreach (var item in carrito.Detalles)
        {
            var libro = item.Libro;
            if (libro.CantidadDisponible < item.Cantidad)
            {
                await tx.RollbackAsync(ct);
                return Conflict($"Stock insuficiente para '{libro.Titulo}'. Disponible: {libro.CantidadDisponible}.");
            }

            libro.CantidadDisponible -= item.Cantidad;
            libro.CantidadTotal -= item.Cantidad; // La venta resta stock permanente

            var detalle = new DetalleVenta
            {
                VentaId = venta.Id,
                LibroId = libro.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = libro.Precio
            };
            
            _db.DetalleVentas.Add(detalle);
            total += (item.Cantidad * libro.Precio);
        }

        venta.Total = total;
        
        // Vaciar carrito
        _db.DetalleCarritos.RemoveRange(carrito.Detalles);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new { mensaje = "Compra realizada con éxito", ventaId = venta.Id });
    }
}
