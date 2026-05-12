using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Proyecto3Api.Configuration;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Domain.Enums;
using Proyecto3Api.DTOs;
using Proyecto3Api.Extensions;
using Proyecto3Api.Hubs;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LibrarySettings _library;
    private readonly IHubContext<NotificationHub> _hub;

    public PrestamosController(
        AppDbContext db,
        IOptions<LibrarySettings> library,
        IHubContext<NotificationHub> hub)
    {
        _db = db;
        _library = library.Value;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamos(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        if (User.IsInRole(AppRoles.Administrador) || User.IsInRole(AppRoles.Bibliotecario))
        {
            var all = await _db.Prestamos.AsNoTracking()
                .Include(p => p.Detalles).ThenInclude(d => d.Libro)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync(ct);
            return Ok(all);
        }

        var mine = await _db.Prestamos.AsNoTracking()
            .Where(p => p.UsuarioId == uid.Value)
            .Include(p => p.Detalles).ThenInclude(d => d.Libro)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToListAsync(ct);
        return Ok(mine);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Prestamo>> GetById(int id, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var p = await _db.Prestamos.AsNoTracking()
            .Include(x => x.Detalles).ThenInclude(d => d.Libro)
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (p is null)
        {
            return NotFound();
        }

        if (p.UsuarioId != uid && !User.IsInRole(AppRoles.Administrador) && !User.IsInRole(AppRoles.Bibliotecario))
        {
            return Forbid();
        }

        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<Prestamo>> Crear(CrearPrestamoRequest dto, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var usuarioPrestamo = uid.Value;
        if (dto.UsuarioId is not null)
        {
            if (!User.IsInRole(AppRoles.Administrador) && !User.IsInRole(AppRoles.Bibliotecario))
            {
                return Forbid();
            }

            usuarioPrestamo = dto.UsuarioId.Value;
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioPrestamo && u.Activo, ct);
        if (usuario is null)
        {
            return BadRequest(new { mensaje = "Usuario no válido" });
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var prestamo = new Prestamo
        {
            UsuarioId = usuarioPrestamo,
            FechaPrestamo = DateTime.UtcNow,
            FechaDevolucionEsperada = DateTime.UtcNow.AddDays(dto.DiasPrestamo),
            Estado = PrestamoEstado.Activo
        };
        _db.Prestamos.Add(prestamo);
        await _db.SaveChangesAsync(ct);

        foreach (var linea in dto.Lineas)
        {
            var libro = await _db.Libros.FirstOrDefaultAsync(l => l.Id == linea.LibroId, ct);
            if (libro is null)
            {
                await tx.RollbackAsync(ct);
                return BadRequest(new { mensaje = $"Libro {linea.LibroId} no existe" });
            }

            if (libro.CantidadDisponible < linea.Cantidad)
            {
                await tx.RollbackAsync(ct);
                return Conflict(new { mensaje = $"Stock insuficiente para '{libro.Titulo}'" });
            }

            libro.CantidadDisponible -= linea.Cantidad;
            _db.DetallePrestamos.Add(new DetallePrestamo
            {
                PrestamoId = prestamo.Id,
                LibroId = libro.Id,
                Cantidad = linea.Cantidad
            });
        }

        _db.HistorialActividades.Add(new HistorialActividad
        {
            UsuarioId = usuarioPrestamo,
            TipoEvento = "PrestamoCreado",
            Descripcion = $"Préstamo #{prestamo.Id} registrado",
            PrestamoId = prestamo.Id,
            Fecha = DateTime.UtcNow
        });

        var notif = new Notificacion
        {
            UsuarioId = usuarioPrestamo,
            Titulo = "Préstamo registrado",
            Mensaje = $"Tu préstamo #{prestamo.Id} fue creado. Devolución esperada: {prestamo.FechaDevolucionEsperada:yyyy-MM-dd} UTC.",
            Tipo = "prestamo",
            Leida = false,
            FechaCreacion = DateTime.UtcNow
        };
        _db.Notificaciones.Add(notif);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await _hub.Clients.User(usuarioPrestamo.ToString())
            .SendAsync("notificacion", notif.Titulo, notif.Mensaje, ct);

        var result = await _db.Prestamos.AsNoTracking()
            .Include(p => p.Detalles).ThenInclude(d => d.Libro)
            .Include(p => p.Usuario)
            .FirstAsync(p => p.Id == prestamo.Id, ct);

        return CreatedAtAction(nameof(GetById), new { id = prestamo.Id }, result);
    }

    [HttpPost("{id:int}/devolver")]
    public async Task<IActionResult> Devolver(int id, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var prestamo = await _db.Prestamos
            .Include(p => p.Detalles)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (prestamo is null)
        {
            return NotFound();
        }

        if (prestamo.UsuarioId != uid && !User.IsInRole(AppRoles.Administrador) && !User.IsInRole(AppRoles.Bibliotecario))
        {
            return Forbid();
        }

        if (prestamo.Estado != PrestamoEstado.Activo)
        {
            await tx.RollbackAsync(ct);
            return Conflict(new { mensaje = "El préstamo ya fue cerrado" });
        }

        var ahora = DateTime.UtcNow;
        prestamo.FechaDevolucionReal = ahora;
        prestamo.Estado = PrestamoEstado.Devuelto;

        foreach (var d in prestamo.Detalles)
        {
            var libro = await _db.Libros.FirstAsync(l => l.Id == d.LibroId, ct);
            libro.CantidadDisponible += d.Cantidad;
        }

        _db.HistorialActividades.Add(new HistorialActividad
        {
            UsuarioId = prestamo.UsuarioId,
            TipoEvento = "PrestamoDevuelto",
            Descripcion = $"Préstamo #{prestamo.Id} devuelto",
            PrestamoId = prestamo.Id,
            Fecha = ahora
        });

        var puntual = ahora <= prestamo.FechaDevolucionEsperada;
        var ranking = await _db.Rankings.FirstOrDefaultAsync(r => r.UsuarioId == prestamo.UsuarioId, ct);
        if (ranking is null)
        {
            ranking = new RankingLector { UsuarioId = prestamo.UsuarioId, Puntos = 0, UltimaActualizacion = ahora };
            _db.Rankings.Add(ranking);
        }

        if (puntual)
        {
            ranking.Puntos += _library.PuntosDevolucionPuntual;
        }

        ranking.UltimaActualizacion = ahora;

        if (!puntual)
        {
            var diasRetraso = (int)Math.Ceiling((ahora - prestamo.FechaDevolucionEsperada).TotalDays);
            if (diasRetraso < 1)
            {
                diasRetraso = 1;
            }

            var multa = new Multa
            {
                PrestamoId = prestamo.Id,
                UsuarioId = prestamo.UsuarioId,
                Monto = diasRetraso * _library.MultaPorDiaRetraso,
                Motivo = $"Retraso de {diasRetraso} día(s) en préstamo #{prestamo.Id}",
                FechaGeneracion = ahora,
                Pagada = false
            };
            _db.Multas.Add(multa);
        }

        var notif = new Notificacion
        {
            UsuarioId = prestamo.UsuarioId,
            Titulo = puntual ? "Devolución registrada" : "Devolución con retraso",
            Mensaje = puntual
                ? $"Gracias por devolver el préstamo #{prestamo.Id} a tiempo."
                : $"Se generó una multa por retraso en el préstamo #{prestamo.Id}.",
            Tipo = puntual ? "info" : "warning",
            Leida = false,
            FechaCreacion = ahora
        };
        _db.Notificaciones.Add(notif);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await _hub.Clients.User(prestamo.UsuarioId.ToString())
            .SendAsync("notificacion", notif.Titulo, notif.Mensaje, ct);

        return NoContent();
    }
}
