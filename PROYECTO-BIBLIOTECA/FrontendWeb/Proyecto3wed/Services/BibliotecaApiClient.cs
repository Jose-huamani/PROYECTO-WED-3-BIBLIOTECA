using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proyecto3wed.Infrastructure;
using Proyecto3wed.Models;
using Proyecto3wed.Options;

namespace Proyecto3wed.Services;

public class BibliotecaApiClient
{
    private static readonly JsonSerializerOptions ReadJson = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions WriteJson = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    public BibliotecaApiClient(HttpClient http, IHttpContextAccessor ctx)
    {
        _http = http;
        _ctx = ctx;
    }

    private HttpContext Http => _ctx.HttpContext ?? throw new InvalidOperationException("Sin HttpContext.");

    private void AuthHeader(HttpRequestMessage req)
    {
        var token = Http.Session.GetString(SessionKeys.AccessToken);
        if (!string.IsNullOrEmpty(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken ct)
    {
        var refreshToken = Http.Session.GetString(SessionKeys.RefreshToken);
        var accessToken = Http.Session.GetString(SessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(refreshToken) && string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        using var res = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken, accessToken }, WriteJson, ct);
        AuthResponseDto? data = null;
        if (res.IsSuccessStatusCode)
        {
            data = await ReadAsync<AuthResponseDto>(res, ct);
        }

        if (data is null || string.IsNullOrWhiteSpace(data.AccessToken))
        {
            data = await TryCreateDevelopmentSessionAsync(ct);
        }

        if (data is null || string.IsNullOrWhiteSpace(data.AccessToken))
        {
            return false;
        }

        Http.Session.SetString(SessionKeys.AccessToken, data.AccessToken);
        Http.Session.SetString(SessionKeys.RefreshToken, data.RefreshToken);
        Http.Session.SetString(SessionKeys.UserEmail, data.Email);
        Http.Session.SetString(SessionKeys.UserName, data.NombreCompleto);
        Http.Session.SetString(SessionKeys.UserRole, data.Rol);
        return true;
    }

    private async Task<AuthResponseDto?> TryCreateDevelopmentSessionAsync(CancellationToken ct)
    {
        var email = Http.Session.GetString(SessionKeys.UserEmail);
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        using var res = await _http.PostAsJsonAsync("api/auth/dev-session", new { email }, WriteJson, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<AuthResponseDto>(res, ct) : null;
    }

    private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<HttpRequestMessage> createRequest, CancellationToken ct)
    {
        var res = await _http.SendAsync(createRequest(), ct);
        if (res.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            return res;
        }

        if (!await TryRefreshTokenAsync(ct))
        {
            return res;
        }

        res.Dispose();
        return await _http.SendAsync(createRequest(), ct);
    }

    private async Task<T?> ReadAsync<T>(HttpResponseMessage res, CancellationToken ct = default)
    {
        if (res.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await res.Content.ReadFromJsonAsync<T>(ReadJson, ct);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage res, CancellationToken ct)
    {
        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return "Tu sesion expiro o la API rechazo el acceso. Inicia sesion nuevamente.";
        }

        var body = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Error HTTP {(int)res.StatusCode}: {res.ReasonPhrase}";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                return $"Error de validaciÃ³n: {errors.GetRawText()}";
            }
            if (doc.RootElement.TryGetProperty("mensaje", out var mensaje))
            {
                return mensaje.GetString() ?? body;
            }

            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
            // Se devuelve el cuerpo original si la API no respondio JSON.
        }

        return body;
    }

    public async Task<(bool ok, string? error, AuthResponseDto? data)> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        using var res = await _http.PostAsJsonAsync("api/auth/login", new { email, password }, WriteJson, ct);
        if (!res.IsSuccessStatusCode)
        {
            return (false, "Credenciales incorrectas o cuenta inactiva.", null);
        }

        var data = await ReadAsync<AuthResponseDto>(res, ct);
        return (data is not null, data is null ? "Respuesta invÃ¡lida" : null, data);
    }

        public async Task<(bool ok, string? error, AuthResponseDto? data)> RegisterAsync(
        string nombre, string email, string password, 
        string? telefono = null, string? carrera = null, string? direccion = null,
        CancellationToken ct = default)
    {
        using var res = await _http.PostAsJsonAsync("api/auth/register", 
            new { nombreCompleto = nombre, email, password, telefono, carrera, direccion }, 
            WriteJson, ct);
        if (!res.IsSuccessStatusCode)
        {
            return (false, await ReadErrorAsync(res, ct), null);
        }

        var data = await ReadAsync<AuthResponseDto>(res, ct);
        return (data is not null, data is null ? "Respuesta invÃ¡lida" : null, data);
    }

    public async Task<(bool ok, string? error, ForgotPasswordResponseDto? data)> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        using var res = await _http.PostAsJsonAsync("api/auth/forgot-password", new { email }, WriteJson, ct);
        if (!res.IsSuccessStatusCode)
        {
            return (false, await ReadErrorAsync(res, ct), null);
        }

        var data = await ReadAsync<ForgotPasswordResponseDto>(res, ct);
        return (true, null, data);
    }

    public async Task<(bool ok, string? error)> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        using var res = await _http.PostAsJsonAsync("api/auth/reset-password", new { token, newPassword }, WriteJson, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<List<LibroDto>?> GetLibrosAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/libros");
        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<LibroDto>>(res, ct);
    }

    public async Task<LibroDto?> GetLibroAsync(int id, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"api/libros/{id}");
        using var res = await _http.SendAsync(req, ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<LibroDto>(res, ct);
    }

    public async Task<LibroDto?> GetLibroByQrAsync(string codigo, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"api/libros/qr/{Uri.EscapeDataString(codigo)}");
        using var res = await _http.SendAsync(req, ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<LibroDto>(res, ct);
    }

    public async Task<List<PrestamoDto>?> GetPrestamosAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/prestamos");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<PrestamoDto>>(res, ct);
    }

    public async Task<(bool ok, string? error)> CrearPrestamoAsync(int libroId, int cantidad, int dias, CancellationToken ct = default)
    {
        var body = new
        {
            usuarioId = (int?)null,
            diasPrestamo = dias,
            lineas = new[] { new { libroId, cantidad } }
        };

        HttpRequestMessage CreateRequest()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/prestamos")
            {
                Content = JsonContent.Create(body, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }

        using var res = await SendWithRefreshAsync(CreateRequest, ct);
        if (!res.IsSuccessStatusCode)
        {
            return (false, await ReadErrorAsync(res, ct));
        }

        return (true, null);
    }

    public async Task<(bool ok, string? error)> DevolverPrestamoAsync(int prestamoId, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/prestamos/{prestamoId}/devolver");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return (false, await ReadErrorAsync(res, ct));
        }

        return (true, null);
    }

    public async Task<List<FavoritoDto>?> GetFavoritosAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/favoritos");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<FavoritoDto>>(res, ct);
    }

    public async Task<(bool ok, string? err)> AddFavoritoAsync(int libroId, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/favoritos/{libroId}");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return (false, await ReadErrorAsync(res, ct));
        }

        return (true, null);
    }

    public async Task<(bool ok, string? err)> RemoveFavoritoAsync(int libroId, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/favoritos/{libroId}");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<List<ReservaDto>?> GetReservasAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/reservas");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<ReservaDto>>(res, ct);
    }

    public async Task<List<ReservaAdminDto>?> GetReservasAdminAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/reservas/admin");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<ReservaAdminDto>>(res, ct);
    }

    public async Task<(bool ok, string? err)> CrearReservaAsync(int libroId, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/reservas/{libroId}");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> CancelarReservaAsync(int id, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/reservas/{id}");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<List<NotificacionDto>?> GetNotificacionesAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/notificaciones");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<NotificacionDto>>(res, ct);
    }

    public async Task MarcarNotificacionLeidaAsync(int id, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/notificaciones/{id}/marcar-leida");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return;
        }
    }

    public async Task<DashboardResumenDto?> GetDashboardAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/dashboard/resumen");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<DashboardResumenDto>(res, ct);
    }

    public async Task<List<AutorDto>?> GetAutoresAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/autores");
        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<AutorDto>>(res, ct);
    }

    public async Task<List<CategoriaDto>?> GetCategoriasAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/categorias");
        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadAsync<List<CategoriaDto>>(res, ct);
    }

    public async Task<List<AdminUsuarioDto>?> GetUsuariosAdminAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/usuarios");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<List<AdminUsuarioDto>>(res, ct) : null;
    }

    public async Task<List<PrestamoDto>?> GetPrestamosAdminAsync(CancellationToken ct = default) => await GetPrestamosAsync(ct);

    public async Task<(bool ok, string? err)> CreateUsuarioAsync(UsuarioCreateForm f, CancellationToken ct = default)
    {
        var payload = new
        {
            nombreCompleto = f.NombreCompleto,
            email = f.Email,
            password = f.Password,
            rolId = f.RolId
        };

        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/usuarios")
            {
                Content = JsonContent.Create(payload, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);

        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> SetUsuarioEstadoAsync(int id, bool activo, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/usuarios/{id}/estado")
            {
                Content = JsonContent.Create(new { activo }, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);

        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> UpdateUsuarioAsync(int id, string nombreCompleto, string email, int rolId, bool activo, string? password, CancellationToken ct = default)
    {
        var payload = new
        {
            nombreCompleto,
            email,
            rolId,
            activo,
            password
        };

        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{id}")
            {
                Content = JsonContent.Create(payload, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);

        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> DeleteUsuarioAsync(int id, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/usuarios/{id}");
            AuthHeader(req);
            return req;
        }, ct);

        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }


    public async Task<List<MultaDto>?> GetMultasAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/multas");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<List<MultaDto>>(res, ct) : null;
    }

    public async Task<(bool ok, string? err)> MarcarMultaPagadaAsync(int id, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/multas/{id}/pagar");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<List<AuditoriaDto>?> GetAuditoriasAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/auditorias");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<List<AuditoriaDto>>(res, ct) : null;
    }

    public async Task<List<RankingDto>?> GetRankingAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/ranking/tabla");
        using var res = await _http.SendAsync(req, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<List<RankingDto>>(res, ct) : null;
    }

    public async Task<(bool ok, string? err)> CreateLibroAsync(LibroCreateForm f, CancellationToken ct = default)
    {
        var autorId = f.AutorId > 0 ? f.AutorId : await GetOrCreateAutorAsync(f.AutorNombre, ct);
        var categoriaId = f.CategoriaId > 0 ? f.CategoriaId : await GetOrCreateCategoriaAsync(f.CategoriaNombre, ct);
        if (autorId is null)
        {
            return (false, "No se pudo registrar o encontrar el autor.");
        }

        if (categoriaId is null)
        {
            return (false, "No se pudo registrar o encontrar la categoria.");
        }

        var payload = new
        {
            id = 0,
            titulo = f.Titulo,
            isbn = f.Isbn,
            autorId,
            categoriaId,
            codigoQR = f.CodigoQR,
            codigoBarras = f.CodigoBarras,
            cantidadTotal = f.CantidadTotal,
            cantidadDisponible = f.CantidadDisponible,
            imagenUrl = f.ImagenUrl,
            precio = f.Precio,
            introduccion = f.Introduccion,
            descripcion = f.Descripcion,
            editorial = f.Editorial,
            numeroPaginas = f.NumeroPaginas,
            idioma = f.Idioma,
            estadoLibro = f.EstadoLibro
        };
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/libros")
            {
                Content = JsonContent.Create(payload, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    private async Task<int?> GetOrCreateAutorAsync(string nombre, CancellationToken ct)
    {
        var limpio = nombre.Trim();
        var existentes = await GetAutoresAsync(ct) ?? new();
        var autor = existentes.FirstOrDefault(a => string.Equals(a.Nombre.Trim(), limpio, StringComparison.OrdinalIgnoreCase));
        if (autor is not null)
        {
            return autor.Id;
        }

        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/autores")
            {
                Content = JsonContent.Create(new { nombre = limpio }, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);

        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        var creado = await ReadAsync<AutorDto>(res, ct);
        return creado?.Id;
    }

    private async Task<int?> GetOrCreateCategoriaAsync(string nombre, CancellationToken ct)
    {
        var limpio = nombre.Trim();
        var existentes = await GetCategoriasAsync(ct) ?? new();
        var categoria = existentes.FirstOrDefault(c => string.Equals(c.Nombre.Trim(), limpio, StringComparison.OrdinalIgnoreCase));
        if (categoria is not null)
        {
            return categoria.Id;
        }

        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/categorias")
            {
                Content = JsonContent.Create(new { nombre = limpio }, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);

        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        var creada = await ReadAsync<CategoriaDto>(res, ct);
        return creada?.Id;
    }

    public async Task<(bool ok, string? err)> DeleteLibroAsync(int id, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/libros/{id}");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(byte[]? bytes, string? contentType, string? fileName)> DownloadExcelAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/reportes/libros/excel");
            AuthHeader(req);
            return req;
        }, ct);

        if (!res.IsSuccessStatusCode) return (null, null, null);

        var bytes = await res.Content.ReadAsByteArrayAsync(ct);
        var contentType = res.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = res.Content.Headers.ContentDisposition?.FileNameStar ?? res.Content.Headers.ContentDisposition?.FileName ?? "reporte.xlsx";
        return (bytes, contentType, fileName);
    }

    public async Task<(byte[]? bytes, string? contentType, string? fileName)> DownloadPdfAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/reportes/libros/pdf");
            AuthHeader(req);
            return req;
        }, ct);

        if (!res.IsSuccessStatusCode) return (null, null, null);

        var bytes = await res.Content.ReadAsByteArrayAsync(ct);
        var contentType = res.Content.Headers.ContentType?.ToString() ?? "application/pdf";
        var fileName = res.Content.Headers.ContentDisposition?.FileNameStar ?? res.Content.Headers.ContentDisposition?.FileName ?? "reporte.pdf";
        return (bytes, contentType, fileName);
    }

    public async Task<(bool ok, string? msg)> UploadExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/reportes/libros/import");
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "archivo", fileName);
            req.Content = content;
            AuthHeader(req);
            return req;
        }, ct);

        if (res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var msg = doc.RootElement.GetProperty("mensaje").GetString();
            return (true, msg);
        }
        return (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> UpdateLibroAsync(LibroCreateForm f, int id, CancellationToken ct = default)
    {
        var autorId = f.AutorId > 0 ? f.AutorId : await GetOrCreateAutorAsync(f.AutorNombre, ct);
        var categoriaId = f.CategoriaId > 0 ? f.CategoriaId : await GetOrCreateCategoriaAsync(f.CategoriaNombre, ct);
        if (autorId is null || categoriaId is null)
        {
            return (false, "No se pudo resolver autor o categoria.");
        }

        var payload = new
        {
            id,
            titulo = f.Titulo,
            isbn = f.Isbn,
            autorId,
            categoriaId,
            codigoQR = f.CodigoQR,
            codigoBarras = f.CodigoBarras,
            cantidadTotal = f.CantidadTotal,
            cantidadDisponible = f.CantidadDisponible,
            imagenUrl = f.ImagenUrl,
            precio = f.Precio,
            introduccion = f.Introduccion,
            descripcion = f.Descripcion,
            editorial = f.Editorial,
            numeroPaginas = f.NumeroPaginas,
            idioma = f.Idioma,
            estadoLibro = f.EstadoLibro
        };

        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/libros/{id}")
            {
                Content = JsonContent.Create(payload, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    // --- CARRITO ---
    public async Task<CarritoDto?> GetMiCarritoAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/carrito");
        AuthHeader(req);
        using var res = await SendWithRefreshAsync(() => req, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<CarritoDto>(res, ct) : null;
    }

    public async Task<(bool ok, string? err)> AgregarAlCarritoAsync(int libroId, int cantidad, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/carrito/agregar/{libroId}?cantidad={cantidad}");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> RemoverDelCarritoAsync(int detalleId, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/carrito/remover/{detalleId}");
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<(bool ok, string? err)> ComprarCarritoAsync(VentaCreateRequest form, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/ventas/comprar-carrito")
            {
                Content = JsonContent.Create(form, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }

    public async Task<List<VentaDto>?> GetVentasAdminAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/ventas");
        AuthHeader(req);
        using var res = await SendWithRefreshAsync(() => req, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<List<VentaDto>>(res, ct) : null;
    }

    public async Task<List<VentaDto>?> GetMisVentasAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/ventas/mis-ventas");
        AuthHeader(req);
        using var res = await SendWithRefreshAsync(() => req, ct);
        return res.IsSuccessStatusCode ? await ReadAsync<List<VentaDto>>(res, ct) : null;
    }

    public async Task<byte[]?> GetReportePrestamosExcelAsync(CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/reportes/prestamos/excel");
            AuthHeader(req);
            return req;
        }, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await res.Content.ReadAsByteArrayAsync(ct);
    }

    // --- NOTIFICACIONES PUSH ---
    public async Task<(bool ok, string? err)> SendNotificationAsync(SendNotificationRequest reqBody, CancellationToken ct = default)
    {
        using var res = await SendWithRefreshAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/notifications/send")
            {
                Content = JsonContent.Create(reqBody, options: WriteJson)
            };
            AuthHeader(req);
            return req;
        }, ct);
        return res.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(res, ct));
    }
}

