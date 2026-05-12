using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Filters;
using Proyecto3wed.Infrastructure;
using Proyecto3wed.Services;

namespace Proyecto3wed.Controllers;

[RequireLogin]
public class UsuarioController : Controller
{
    private readonly BibliotecaApiClient _api;

    public UsuarioController(BibliotecaApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        ViewBag.UserName = HttpContext.Session.GetString(SessionKeys.UserName);
        
        // Fetch data for the dashboard cards
        ViewBag.Prestamos = await _api.GetPrestamosAsync(ct) ?? new();
        ViewBag.Reservas = await _api.GetReservasAsync(ct) ?? new();
        ViewBag.Multas = await _api.GetMultasAsync(ct) ?? new();
        ViewBag.Ventas = await _api.GetMisVentasAsync(ct) ?? new();
        
        // Also load some books for the highlights
        var libros = await _api.GetLibrosAsync(ct) ?? new();
        ViewBag.LibrosDestacados = libros.Take(4).ToList();
        ViewBag.LibrosPopulares = libros.OrderByDescending(l => l.CantidadTotal).Take(4).ToList();
        
        return View();
    }

    public IActionResult Configuracion()
    {
        return View();
    }

    public IActionResult Perfil()
    {
        // En una app real, podrías llamar a _api.GetPerfilAsync()
        // Por ahora, mostraremos datos ficticios o tomados de la sesión
        ViewBag.UserName = HttpContext.Session.GetString(SessionKeys.UserName);
        ViewBag.UserEmail = HttpContext.Session.GetString(SessionKeys.UserEmail);
        ViewBag.UserRole = HttpContext.Session.GetString(SessionKeys.UserRole);
        return View();
    }

    public async Task<IActionResult> MisVentas(CancellationToken ct)
    {
        var ventas = await _api.GetMisVentasAsync(ct) ?? new();
        return View(ventas);
    }

    public async Task<IActionResult> Ranking(CancellationToken ct)
    {
        var ranking = await _api.GetRankingAsync(ct) ?? new();
        return View(ranking);
    }

    public async Task<IActionResult> Historial(CancellationToken ct)
    {
        // Fetch all data for the unified history view
        ViewBag.Prestamos = await _api.GetPrestamosAsync(ct) ?? new();
        ViewBag.Reservas = await _api.GetReservasAsync(ct) ?? new();
        ViewBag.Ventas = await _api.GetMisVentasAsync(ct) ?? new();
        ViewBag.Multas = await _api.GetMultasAsync(ct) ?? new();
        return View();
    }
}
