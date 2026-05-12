using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Filters;
using Proyecto3wed.Models;
using Proyecto3wed.Services;

namespace Proyecto3wed.Controllers;

[RequireLogin]
[RequireStaff]
public class AdminController : Controller
{
    private readonly BibliotecaApiClient _api;

    public AdminController(BibliotecaApiClient api) => _api = api;

    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var resumen = await _api.GetDashboardAsync(ct);
        return View(resumen);
    }

    public async Task<IActionResult> Libros(CancellationToken ct)
    {
        var libros = await _api.GetLibrosAsync(ct) ?? new();
        return View(libros);
    }

    public async Task<IActionResult> Reservas(CancellationToken ct)
    {
        var reservas = await _api.GetReservasAdminAsync(ct) ?? new();
        return View(reservas);
    }

    public async Task<IActionResult> Usuarios(CancellationToken ct)
    {
        var usuarios = await _api.GetUsuariosAdminAsync(ct) ?? new();
        return View(usuarios.Where(u => u.Rol == "Usuario" || u.Rol == "Lector").ToList());
    }

    public async Task<IActionResult> Empleados(CancellationToken ct)
    {
        var usuarios = await _api.GetUsuariosAdminAsync(ct) ?? new();
        var empleados = usuarios.Where(u => u.Rol == "Administrador" || u.Rol == "Bibliotecario").ToList();
        return View(empleados);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUsuario(UsuarioCreateForm model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Err"] = "Completa los datos obligatorios del usuario.";
            return RedirectToAction(nameof(Usuarios));
        }

        var (ok, err) = await _api.CreateUsuarioAsync(model, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Usuario registrado correctamente." : (err ?? "No se pudo registrar el usuario.");
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoUsuario(int id, bool activo, CancellationToken ct)
    {
        var (ok, err) = await _api.SetUsuarioEstadoAsync(id, activo, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Estado del usuario actualizado." : (err ?? "No se pudo actualizar el usuario.");
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUsuario(int id, string NombreCompleto, string Email, int RolId, bool Activo, string? Password, CancellationToken ct)
    {
        var (ok, err) = await _api.UpdateUsuarioAsync(id, NombreCompleto, Email, RolId, Activo, Password, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Usuario actualizado correctamente." : (err ?? "No se pudo actualizar el usuario.");
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(int id, CancellationToken ct)
    {
        var (ok, err) = await _api.DeleteUsuarioAsync(id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Usuario eliminado del sistema." : (err ?? "No se pudo eliminar el usuario.");
        return RedirectToAction(nameof(Usuarios));
    }

    public async Task<IActionResult> Prestamos(CancellationToken ct)
    {
        var prestamos = await _api.GetPrestamosAdminAsync(ct) ?? new();
        return View(prestamos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarDevolucion(int id, CancellationToken ct)
    {
        var (ok, err) = await _api.DevolverPrestamoAsync(id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Devolucion registrada. El stock se actualizo automaticamente." : (err ?? "No se pudo registrar la devolucion.");
        return RedirectToAction(nameof(Prestamos));
    }

    public async Task<IActionResult> Multas(CancellationToken ct)
    {
        var multas = await _api.GetMultasAsync(ct) ?? new();
        return View(multas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagarMulta(int id, CancellationToken ct)
    {
        var (ok, err) = await _api.MarcarMultaPagadaAsync(id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Pago de multa registrado." : (err ?? "No se pudo registrar el pago.");
        return RedirectToAction(nameof(Multas));
    }

    public async Task<IActionResult> Auditoria(CancellationToken ct)
    {
        var auditorias = await _api.GetAuditoriasAsync(ct) ?? new();
        return View(auditorias);
    }

    public async Task<IActionResult> Gamificacion(CancellationToken ct)
    {
        var ranking = await _api.GetRankingAsync(ct) ?? new();
        return View(ranking);
    }

    public IActionResult Seguridad() => View();

    public IActionResult Perfil() => View();


    public async Task<IActionResult> Ventas(CancellationToken ct)
    {
        var ventas = await _api.GetVentasAdminAsync(ct) ?? new();
        return View(ventas);
    }

    public IActionResult Pagos() => View();

    public IActionResult Reportes() => View();

    public async Task<IActionResult> Notificaciones(CancellationToken ct)
    {
        var usuarios = await _api.GetUsuariosAdminAsync(ct) ?? new();
        ViewBag.Usuarios = usuarios;
        return View();
    }



    public IActionResult InteligenciaArtificial() => View();

    public IActionResult SistemaTecnico() => View();

    public IActionResult Configuracion() => View();

    [HttpGet]
    public async Task<IActionResult> NuevoLibro(CancellationToken ct)
    {
        ViewBag.Autores = await _api.GetAutoresAsync(ct) ?? new();
        ViewBag.Categorias = await _api.GetCategoriasAsync(ct) ?? new();
        return View(new LibroCreateForm
        {
            CodigoQR = $"LIB-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CodigoBarras = $"BC{DateTime.UtcNow:yyyyMMddHHmmss}",
            FechaIngreso = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevoLibro(LibroCreateForm model, CancellationToken ct)
    {
        if (model.CantidadDisponible > model.CantidadTotal)
        {
            ModelState.AddModelError(nameof(model.CantidadDisponible), "Los disponibles no pueden superar el total de ejemplares.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Autores = await _api.GetAutoresAsync(ct) ?? new();
            ViewBag.Categorias = await _api.GetCategoriasAsync(ct) ?? new();
            return View(model);
        }

        var (ok, err) = await _api.CreateLibroAsync(model, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, err ?? "No se pudo crear el libro.");
            ViewBag.Autores = await _api.GetAutoresAsync(ct) ?? new();
            ViewBag.Categorias = await _api.GetCategoriasAsync(ct) ?? new();
            return View(model);
        }

        TempData["Msg"] = "Libro guardado correctamente.";
        return RedirectToAction(nameof(Libros));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarLibro(int id, CancellationToken ct)
    {
        var (ok, err) = await _api.DeleteLibroAsync(id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Libro eliminado." : (err ?? "No se pudo eliminar.");
        return RedirectToAction(nameof(Libros));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarLibro(int id, LibroCreateForm model, CancellationToken ct)
    {
        if (model.CantidadDisponible > model.CantidadTotal)
        {
            TempData["Err"] = "Los disponibles no pueden superar el stock total.";
            return RedirectToAction(nameof(Libros));
        }

        var (ok, err) = await _api.UpdateLibroAsync(model, id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Libro actualizado correctamente." : (err ?? "No se pudo actualizar el libro.");
        return RedirectToAction(nameof(Libros));
    }

    public async Task<IActionResult> ReportePrestamos(CancellationToken ct)
    {
        var bytes = await _api.GetReportePrestamosExcelAsync(ct);
        if (bytes is null || bytes.Length == 0)
        {
            TempData["Err"] = "No se pudo generar el reporte.";
            return RedirectToAction(nameof(Dashboard));
        }

        var name = $"prestamos-{DateTime.UtcNow:yyyyMMddHHmm}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarLibrosExcel(CancellationToken ct)
    {
        var (bytes, contentType, fileName) = await _api.DownloadExcelAsync(ct);
        if (bytes == null)
        {
            TempData["Err"] = "No se pudo descargar el archivo Excel.";
            return RedirectToAction(nameof(Libros));
        }
        return File(bytes, contentType!, fileName!);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarLibrosPdf(CancellationToken ct)
    {
        var (bytes, contentType, fileName) = await _api.DownloadPdfAsync(ct);
        if (bytes == null)
        {
            TempData["Err"] = "No se pudo descargar el archivo PDF.";
            return RedirectToAction(nameof(Libros));
        }
        return File(bytes, contentType!, fileName!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportarLibrosExcel(IFormFile archivoExcel, CancellationToken ct)
    {
        if (archivoExcel == null || archivoExcel.Length == 0)
        {
            TempData["Err"] = "Debes seleccionar un archivo Excel válido.";
            return RedirectToAction(nameof(Libros));
        }

        using var stream = archivoExcel.OpenReadStream();
        var (ok, msg) = await _api.UploadExcelAsync(stream, archivoExcel.FileName, ct);
        
        TempData[ok ? "Msg" : "Err"] = msg ?? (ok ? "Importación exitosa" : "Error al importar");
        return RedirectToAction(nameof(Libros));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarNotificacion(SendNotificationRequest req, string returnUrl, CancellationToken ct)
    {
        var (ok, err) = await _api.SendNotificationAsync(req, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Notificación enviada exitosamente" : (err ?? "Error al enviar notificación");
        
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Notificaciones));
    }
}
