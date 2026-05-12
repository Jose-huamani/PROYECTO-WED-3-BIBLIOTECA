using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Filters;
using Proyecto3wed.Models;
using Proyecto3wed.Services;

namespace Proyecto3wed.Controllers;

[RequireLogin]
public class TiendaController : Controller
{
    private readonly BibliotecaApiClient _api;

    public TiendaController(BibliotecaApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var libros = await _api.GetLibrosAsync(ct) ?? new List<LibroDto>();
        // Solo mostrar libros disponibles para la venta (con precio mayor a 0)
        var disponibles = libros.Where(l => l.CantidadDisponible > 0 && l.Precio > 0).ToList();
        return View(disponibles);
    }

    public async Task<IActionResult> Carrito(CancellationToken ct)
    {
        var carrito = await _api.GetMiCarritoAsync(ct);
        if (carrito == null) carrito = new CarritoDto();
        return View(carrito);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int libroId, int cantidad, CancellationToken ct)
    {
        var (ok, err) = await _api.AgregarAlCarritoAsync(libroId, cantidad, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Añadido al carrito exitosamente." : (err ?? "Error al añadir al carrito.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int detalleId, CancellationToken ct)
    {
        var (ok, err) = await _api.RemoverDelCarritoAsync(detalleId, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Producto removido del carrito." : (err ?? "Error al remover.");
        return RedirectToAction(nameof(Carrito));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comprar(VentaCreateRequest form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Err"] = "Datos de pago inválidos.";
            return RedirectToAction(nameof(Carrito));
        }

        var (ok, err) = await _api.ComprarCarritoAsync(form, ct);
        if (ok)
        {
            TempData["Msg"] = "¡Compra realizada con éxito! Revisa tus ventas en el historial.";
            return RedirectToAction("MisVentas", "Usuario"); // Redirigir a mis compras (necesitamos crearlo si no existe)
        }
        else
        {
            TempData["Err"] = err ?? "No se pudo procesar la compra.";
            return RedirectToAction(nameof(Carrito));
        }
    }
}
