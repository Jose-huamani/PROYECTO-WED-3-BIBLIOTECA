using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Domain.Enums;
using Proyecto3Api.DTOs;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;

namespace Proyecto3Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReservasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaResponse>>> Get(CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var q = _db.Reservas.AsNoTracking().Where(r => r.UsuarioId == uid.Value)
            .Include(r => r.Libro).ThenInclude(l => l.Autor)
            .OrderByDescending(r => r.FechaReserva);

        var data = await q.ToListAsync(ct);
        return Ok(data.Select(ToResponse));
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpGet("admin")]
    public async Task<ActionResult<IEnumerable<ReservaAdminResponse>>> GetAdmin(CancellationToken ct)
    {
        var data = await _db.Reservas.AsNoTracking()
            .Include(r => r.Usuario)
            .Include(r => r.Libro).ThenInclude(l => l.Autor)
            .OrderByDescending(r => r.FechaReserva)
            .Select(r => new ReservaAdminResponse
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                LibroId = r.LibroId,
                FechaReserva = r.FechaReserva,
                FechaExpiracion = r.FechaExpiracion,
                Estado = (int)r.Estado,
                EstadoNombre = r.Estado.ToString(),
                Libro = new LibroMiniResponse
                {
                    Id = r.Libro.Id,
                    Titulo = r.Libro.Titulo,
                    Autor = r.Libro.Autor.Nombre
                },
                Usuario = new UsuarioMiniResponse
                {
                    Id = r.Usuario.Id,
                    NombreCompleto = r.Usuario.NombreCompleto,
                    Email = r.Usuario.Email
                },
                Observaciones = null
            })
            .ToListAsync(ct);

        return Ok(data);
    }

    [HttpPost("{libroId:int}")]
    public async Task<ActionResult<ReservaResponse>> Crear(int libroId, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        if (!await _db.Libros.AnyAsync(l => l.Id == libroId, ct))
        {
            return NotFound(new { mensaje = "Libro no existe" });
        }

        if (await _db.Reservas.AnyAsync(
                r => r.UsuarioId == uid.Value && r.LibroId == libroId && r.Estado == ReservaEstado.Pendiente,
                ct))
        {
            return Conflict(new { mensaje = "Ya tienes una reserva pendiente para este libro" });
        }

        var r = new Reserva
        {
            UsuarioId = uid.Value,
            LibroId = libroId,
            FechaReserva = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(3),
            Estado = ReservaEstado.Pendiente
        };
        _db.Reservas.Add(r);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(r).Reference(x => x.Libro).Query().Include(l => l.Autor).LoadAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = r.Id }, ToResponse(r));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var r = await _db.Reservas.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null)
        {
            return NotFound();
        }

        if (r.UsuarioId != uid.Value)
        {
            return Forbid();
        }

        r.Estado = ReservaEstado.Cancelada;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPut("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id, CancellationToken ct)
    {
        var reserva = await _db.Reservas
            .Include(r => r.Libro)
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reserva is null)
        {
            return NotFound(new { mensaje = "Reserva no encontrada" });
        }

        if (reserva.Estado != ReservaEstado.Pendiente)
        {
            return BadRequest(new { mensaje = "Solo se pueden aprobar reservas pendientes" });
        }

        reserva.Estado = ReservaEstado.Atendida;
        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = reserva.UsuarioId,
            Titulo = "Reserva aprobada",
            Mensaje = $"Tu reserva del libro {reserva.Libro.Titulo} fue aprobada.",
            FechaCreacion = DateTime.UtcNow
        });
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = User.GetUserId(),
            Accion = "AprobarReserva",
            TipoEntidad = nameof(Reserva),
            EntidadId = reserva.Id.ToString(),
            Fecha = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPut("{id:int}/rechazar")]
    public async Task<IActionResult> Rechazar(int id, CancellationToken ct)
    {
        var reserva = await _db.Reservas
            .Include(r => r.Libro)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reserva is null)
        {
            return NotFound(new { mensaje = "Reserva no encontrada" });
        }

        if (reserva.Estado != ReservaEstado.Pendiente)
        {
            return BadRequest(new { mensaje = "Solo se pueden rechazar reservas pendientes" });
        }

        reserva.Estado = ReservaEstado.Cancelada;
        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = reserva.UsuarioId,
            Titulo = "Reserva rechazada",
            Mensaje = $"Tu reserva del libro {reserva.Libro.Titulo} fue rechazada.",
            FechaCreacion = DateTime.UtcNow
        });
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = User.GetUserId(),
            Accion = "RechazarReserva",
            TipoEntidad = nameof(Reserva),
            EntidadId = reserva.Id.ToString(),
            Fecha = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static ReservaResponse ToResponse(Reserva r) => new()
    {
        Id = r.Id,
        UsuarioId = r.UsuarioId,
        LibroId = r.LibroId,
        FechaReserva = r.FechaReserva,
        FechaExpiracion = r.FechaExpiracion,
        Estado = (int)r.Estado,
        EstadoNombre = r.Estado.ToString(),
        Libro = new LibroMiniResponse
        {
            Id = r.Libro.Id,
            Titulo = r.Libro.Titulo,
            Autor = r.Libro.Autor?.Nombre
        }
    };
}
