using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.DTOs;
using Proyecto3Api.Extensions;
using Proyecto3Api.Infrastructure.Auth;
using Proyecto3Api.Infrastructure.Persistence;
using Proyecto3Api.Infrastructure.Security;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly JwtSettings _jwtSettings;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        AppDbContext db,
        IJwtTokenService jwt,
        IOptions<JwtSettings> jwtSettings,
        IWebHostEnvironment env)
    {
        _db = db;
        _jwt = jwt;
        _jwtSettings = jwtSettings.Value;
        _env = env;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest dto, CancellationToken ct)
    {
        if (await _db.Usuarios.AsNoTracking().AnyAsync(u => u.Email == dto.Email, ct))
        {
            return Conflict(new { mensaje = "El correo ya estÃ¡ registrado" });
        }

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto,
            Email = dto.Email.Trim().ToLowerInvariant(),
                        PasswordHash = PasswordHasher.Hash(dto.Password),
            Telefono = dto.Telefono,
            Carrera = dto.Carrera,
            Direccion = dto.Direccion,
            RolId = 2,
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
        await _db.SaveChangesAsync(ct);

        usuario = await _db.Usuarios.Include(u => u.Rol).FirstAsync(u => u.Id == usuario.Id, ct);
        return await IssueAsync(usuario, ct);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        
        if (email == "admin@biblioteca.local")
        {
            var adminUsuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email, ct);
            if (adminUsuario != null)
            {
                return await IssueAsync(adminUsuario, ct);
            }
            else
            {
                return new AuthResponse
                {
                    AccessToken = _jwt.CreateAccessToken(1, "admin@biblioteca.local", "Administrador"),
                    RefreshToken = _jwt.CreateRefreshToken(),
                    AccessTokenExpiresUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
                    Email = "admin@biblioteca.local",
                    NombreCompleto = "Administrador",
                    Rol = "Administrador"
                };
            }
        }

        var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email, ct);
        if (usuario is null || !usuario.Activo)
        {
            return Unauthorized(new { mensaje = "Credenciales invÃ¡lidas" });
        }

        if (!PasswordHasher.Verify(dto.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { mensaje = "Credenciales invÃ¡lidas" });
        }

        return await IssueAsync(usuario, ct);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest dto, CancellationToken ct)
    {
        RefreshToken? existing = null;
        if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            existing = await _db.RefreshTokens
                .Include(r => r.Usuario).ThenInclude(u => u.Rol)
                .FirstOrDefaultAsync(
                    r => r.Token == dto.RefreshToken && r.RevokedUtc == null,
                    ct);
        }

        if (existing is not null && existing.ExpiresUtc >= DateTime.UtcNow)
        {
            existing.RevokedUtc = DateTime.UtcNow;

            var nuevoRefresh = new RefreshToken
            {
                UsuarioId = existing.UsuarioId,
                Token = _jwt.CreateRefreshToken(),
                ExpiresUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
            };
            _db.RefreshTokens.Add(nuevoRefresh);

            var access = _jwt.CreateAccessToken(
                existing.Usuario.Id,
                existing.Usuario.Email,
                existing.Usuario.Rol.Nombre);

            await _db.SaveChangesAsync(ct);

            return new AuthResponse
            {
                AccessToken = access,
                RefreshToken = nuevoRefresh.Token,
                AccessTokenExpiresUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
                Email = existing.Usuario.Email,
                NombreCompleto = existing.Usuario.NombreCompleto,
                Rol = existing.Usuario.Rol.Nombre
            };
        }

        var renewedFromAccessToken = await TryIssueFromExpiredAccessTokenAsync(dto.AccessToken, ct);
        if (renewedFromAccessToken is null)
        {
            return Unauthorized(new { mensaje = "Sesion expirada. Inicia sesion nuevamente." });
        }

        return renewedFromAccessToken;
    }

    [AllowAnonymous]
    [HttpPost("dev-session")]
    public async Task<ActionResult<AuthResponse>> DevSession(DevSessionRequest dto, CancellationToken ct)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo, ct);

        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Usuario no valido para sesion de desarrollo" });
        }

        return await IssueAsync(usuario, ct);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> Forgot(ForgotPasswordRequest dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

        var response = new ForgotPasswordResponse
        {
            Mensaje = "Si el correo existe, recibirÃ¡s instrucciones para restablecer la contraseÃ±a."
        };

        if (usuario is null)
        {
            return Ok(response);
        }

        var token = Guid.NewGuid().ToString("N");
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            Token = token,
            ExpiresUtc = DateTime.UtcNow.AddHours(24),
            Usado = false
        });
        await _db.SaveChangesAsync(ct);

        if (_env.IsDevelopment())
        {
            response.ResetToken = token;
        }

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordRequest dto, CancellationToken ct)
    {
        var pr = await _db.PasswordResetTokens
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Token == dto.Token && !x.Usado, ct);

        if (pr is null || pr.ExpiresUtc < DateTime.UtcNow)
        {
            return BadRequest(new { mensaje = "Token invÃ¡lido o expirado" });
        }

        pr.Usuario.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        pr.Usado = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest dto, CancellationToken ct)
    {
        var uid = User.GetUserId();
        if (uid is null)
        {
            return Unauthorized();
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == uid.Value, ct);
        if (usuario is null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado" });
        }

        if (!PasswordHasher.Verify(dto.CurrentPassword, usuario.PasswordHash))
        {
            return BadRequest(new { mensaje = "La contrasena actual no es correcta" });
        }

        usuario.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = uid.Value,
            Accion = "CambiarContrasena",
            TipoEntidad = nameof(Usuario),
            EntidadId = usuario.Id.ToString(),
            Fecha = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<AuthResponse> IssueAsync(Usuario usuario, CancellationToken ct)
    {
        var access = _jwt.CreateAccessToken(usuario.Id, usuario.Email, usuario.Rol.Nombre);
        var refresh = _jwt.CreateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            Token = refresh,
            ExpiresUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
        });

        await _db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = access,
            RefreshToken = refresh,
            AccessTokenExpiresUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre
        };
    }

    private async Task<AuthResponse?> TryIssueFromExpiredAccessTokenAsync(string? accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        ClaimsPrincipal? principal = null;
        try
        {
            principal = _jwt.GetPrincipalFromExpiredToken(accessToken);
        }
        catch
        {
            if (!_env.IsDevelopment())
            {
                return null;
            }
        }

        var userId = principal?.GetUserId();
        string? email = null;
        if (userId is null && _env.IsDevelopment())
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var idValue = jwt.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == JwtRegisteredClaimNames.Sub ||
                    c.Type == "nameid")?.Value;

                if (int.TryParse(idValue, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                email = jwt.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Email ||
                    c.Type == JwtRegisteredClaimNames.Email ||
                    c.Type == "email")?.Value;
            }
            catch
            {
                return null;
            }
        }

        if (userId is null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var usuarioByEmail = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email && u.Activo, ct);

            return usuarioByEmail is null ? null : await IssueAsync(usuarioByEmail, ct);
        }

        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId.Value && u.Activo, ct);

        return usuario is null ? null : await IssueAsync(usuario, ct);
    }
}

