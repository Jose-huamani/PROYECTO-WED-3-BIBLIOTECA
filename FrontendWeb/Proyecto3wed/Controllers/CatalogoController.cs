using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Infrastructure;
using Proyecto3wed.Services;
using QRCoder;

namespace Proyecto3wed.Controllers;

public class CatalogoController : Controller
{
    private readonly BibliotecaApiClient _api;

    public CatalogoController(BibliotecaApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var role = HttpContext.Session.GetString(SessionKeys.UserRole);
        if (role is "Administrador" or "Bibliotecario")
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        var libros = await _api.GetLibrosAsync(ct) ?? new();
        return View(libros);
    }

    public async Task<IActionResult> Detalle(int id, CancellationToken ct)
    {
        var libro = await _api.GetLibroAsync(id, ct);
        if (libro is null)
        {
            return NotFound();
        }

        ViewBag.LoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.AccessToken));
        ViewBag.Role = HttpContext.Session.GetString(SessionKeys.UserRole);
        return View(libro);
    }

    [HttpGet]
    public IActionResult BuscarQr() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuscarQr(string codigo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            ModelState.AddModelError(string.Empty, "Ingresa un cÃ³digo QR.");
            return View();
        }

        var libro = await _api.GetLibroByQrAsync(codigo.Trim(), ct);
        if (libro is null)
        {
            ModelState.AddModelError(string.Empty, "No se encontrÃ³ un libro con ese cÃ³digo.");
            return View();
        }

        return RedirectToAction(nameof(Detalle), new { id = libro.Id });
    }

    /// <summary>Imagen PNG del cÃ³digo QR del libro (texto almacenado en CodigoQR).</summary>
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> QrLibroPng(int id, CancellationToken ct)
    {
        var libro = await _api.GetLibroAsync(id, ct);
        if (libro is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(libro.CodigoQR))
        {
            return BadRequest();
        }

        using var gen = new QRCodeGenerator();
        var data = gen.CreateQrCode(libro.CodigoQR, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(16);
        return File(bytes, "image/png");
    }

    /// <summary>Returns books as JSON for AJAX home page preview.</summary>
    [HttpGet]
    public async Task<IActionResult> LibrosJson(CancellationToken ct)
    {
        var libros = await _api.GetLibrosAsync(ct) ?? new();
        return Json(libros);
    }
}
