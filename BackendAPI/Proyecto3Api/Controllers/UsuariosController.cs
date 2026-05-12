using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Domain.Enums;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Persistence;
using Proyecto3Api.Infrastructure.Security;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db)
    {
        _db = db;
    }

    // =========================================
    // PERFIL USUARIO LOGUEADO
    // =========================================

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Ranking)
            .FirstOrDefaultAsync(
                u => u.Id == userId,
                ct
            );

        if (usuario == null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado"
            });
        }

        return Ok(new
        {
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Email,
            Rol = usuario.Rol.Nombre,
            usuario.Activo,
            Puntos = usuario.Ranking == null
                ? 0
                : usuario.Ranking.Puntos
        });
    }

    // =========================================
    // EDITAR PERFIL PERSONAL
    // =========================================

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        UsuarioPerfilRequest dto,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(
                u => u.Id == userId,
                ct
            );

        if (usuario == null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado"
            });
        }

        usuario.NombreCompleto =
            dto.NombreCompleto.Trim();

        usuario.Email =
            dto.Email.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            usuario.PasswordHash =
                PasswordHasher.Hash(dto.Password);
        }

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // =========================================
    // LISTAR USUARIOS
    // SOLO ADMIN/BIBLIOTECARIO
    // =========================================

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? q,
        [FromQuery] bool? morosos,
        CancellationToken ct
    )
    {
        var query = _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Ranking)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();

            query = query.Where(u =>
                u.NombreCompleto.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Rol.Nombre.ToLower().Contains(term));
        }

        if (morosos == true)
        {
            query = query.Where(u =>
                u.Multas.Any(m => !m.Pagada));
        }

        var usuarios = await query
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new
            {
                u.Id,
                u.NombreCompleto,
                u.Email,
                Rol = u.Rol.Nombre,
                u.Activo,

                PrestamosActivos =
                    u.Prestamos.Count(
                        p => p.Estado ==
                             PrestamoEstado.Activo),

                MultasPendientes =
                    u.Multas.Count(
                        m => !m.Pagada),

                Puntos = u.Ranking == null
                    ? 0
                    : u.Ranking.Puntos
            })
            .ToListAsync(ct);

        return Ok(usuarios);
    }

    // =========================================
    // CREAR USUARIO
    // SOLO ADMIN/BIBLIOTECARIO
    // =========================================

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost]
    public async Task<IActionResult> Post(
        UsuarioCreateRequest dto,
        CancellationToken ct
    )
    {
        var email = dto.Email
            .Trim()
            .ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(
                u => u.Email == email,
                ct))
        {
            return Conflict(new
            {
                mensaje = "El correo ya esta registrado"
            });
        }

        if (!await _db.Roles.AnyAsync(
                r => r.Id == dto.RolId,
                ct))
        {
            return BadRequest(new
            {
                mensaje = "El rol seleccionado no existe"
            });
        }

        var usuario = new Usuario
        {
            NombreCompleto =
                dto.NombreCompleto.Trim(),

            Email = email,

            PasswordHash =
                PasswordHasher.Hash(dto.Password),

            RolId = dto.RolId,

            Activo = true
        };

        _db.Usuarios.Add(usuario);

        await _db.SaveChangesAsync(ct);

        _db.Rankings.Add(new RankingLector
        {
            UsuarioId = usuario.Id,
            Puntos = 0,
            UltimaActualizacion = DateTime.UtcNow
        });

        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = User.GetUserId(),

            Accion = "CrearUsuario",

            TipoEntidad = nameof(Usuario),

            EntidadId = usuario.Id.ToString(),

            Fecha = DateTime.UtcNow,

            IpAddress =
                HttpContext.Connection
                    .RemoteIpAddress
                    ?.ToString()
        });

        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(
            nameof(Get),
            new { id = usuario.Id },
            new { usuario.Id });
    }

    // =========================================
    // CAMBIAR ESTADO
    // SOLO ADMIN/BIBLIOTECARIO
    // =========================================

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        [FromBody] UsuarioEstadoRequest? dto,
        [FromQuery] bool? activo,
        CancellationToken ct
    )
    {
        var usuario = await _db.Usuarios
            .FindAsync(new object?[] { id }, ct);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado"
            });
        }

        var nuevoEstado =
            activo ?? dto?.Activo ?? usuario.Activo;

        usuario.Activo = nuevoEstado;

        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = User.GetUserId(),

            Accion = nuevoEstado
                ? "ActivarUsuario"
                : "BloquearUsuario",

            TipoEntidad = nameof(Usuario),

            EntidadId = usuario.Id.ToString(),

            Fecha = DateTime.UtcNow,

            IpAddress =
                HttpContext.Connection
                    .RemoteIpAddress
                    ?.ToString()
        });

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // =========================================
    // EDITAR USUARIO
    // SOLO ADMIN/BIBLIOTECARIO
    // =========================================

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(
        int id,
        UsuarioUpdateRequest dto,
        CancellationToken ct
    )
    {
        var usuario = await _db.Usuarios
            .FindAsync(new object?[] { id }, ct);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado"
            });
        }

        var email = dto.Email
            .Trim()
            .ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(
                u => u.Id != id &&
                     u.Email == email,
                ct))
        {
            return Conflict(new
            {
                mensaje = "El correo ya esta registrado"
            });
        }

        if (!await _db.Roles.AnyAsync(
                r => r.Id == dto.RolId,
                ct))
        {
            return BadRequest(new
            {
                mensaje = "El rol seleccionado no existe"
            });
        }

        usuario.NombreCompleto =
            dto.NombreCompleto.Trim();

        usuario.Email = email;

        usuario.RolId = dto.RolId;

        usuario.Activo = dto.Activo;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            usuario.PasswordHash =
                PasswordHasher.Hash(dto.Password);
        }

        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = User.GetUserId(),

            Accion = "EditarUsuario",

            TipoEntidad = nameof(Usuario),

            EntidadId = usuario.Id.ToString(),

            Fecha = DateTime.UtcNow,

            IpAddress =
                HttpContext.Connection
                    .RemoteIpAddress
                    ?.ToString()
        });

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // =========================================
    // ELIMINAR USUARIO
    // SOLO ADMIN/BIBLIOTECARIO
    // =========================================

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct
    )
    {
        var currentUserId = User.GetUserId();

        if (currentUserId == id)
        {
            return BadRequest(new
            {
                mensaje =
                    "No puedes eliminar tu propio usuario administrador"
            });
        }

        var usuario = await _db.Usuarios
            .Include(u => u.Prestamos)
            .Include(u => u.Reservas)
            .Include(u => u.Multas)
            .FirstOrDefaultAsync(
                u => u.Id == id,
                ct);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado"
            });
        }

        if (usuario.Prestamos.Any() ||
            usuario.Reservas.Any() ||
            usuario.Multas.Any())
        {
            usuario.Activo = false;

            _db.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,

                Accion =
                    "BloquearUsuarioConHistorial",

                TipoEntidad = nameof(Usuario),

                EntidadId = usuario.Id.ToString(),

                Fecha = DateTime.UtcNow,

                IpAddress =
                    HttpContext.Connection
                        .RemoteIpAddress
                        ?.ToString()
            });
        }
        else
        {
            _db.Usuarios.Remove(usuario);

            _db.Auditorias.Add(new Auditoria
            {
                UsuarioId = currentUserId,

                Accion = "EliminarUsuario",

                TipoEntidad = nameof(Usuario),

                EntidadId = id.ToString(),

                Fecha = DateTime.UtcNow,

                IpAddress =
                    HttpContext.Connection
                        .RemoteIpAddress
                        ?.ToString()
            });
        }

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}

// =========================================
// DTO PERFIL PERSONAL
// =========================================

public sealed class UsuarioPerfilRequest
{
    [Required]
    public string NombreCompleto { get; set; }
        = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; }
        = string.Empty;

    public string? Password { get; set; }
}

// =========================================
// DTO CREAR USUARIO
// =========================================

public sealed class UsuarioCreateRequest
{
    [Required, MaxLength(200)]
    public string NombreCompleto { get; set; }
        = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; }
        = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; }
        = string.Empty;

    [Range(1, int.MaxValue)]
    public int RolId { get; set; } = 2;
}

// =========================================
// DTO ESTADO USUARIO
// =========================================

public sealed class UsuarioEstadoRequest
{
    public bool Activo { get; set; }
}

// =========================================
// DTO EDITAR USUARIO ADMIN
// =========================================

public sealed class UsuarioUpdateRequest
{
    [Required, MaxLength(200)]
    public string NombreCompleto { get; set; }
        = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; }
        = string.Empty;

    [MinLength(6)]
    public string? Password { get; set; }

    [Range(1, int.MaxValue)]
    public int RolId { get; set; } = 2;

    public bool Activo { get; set; } = true;
}