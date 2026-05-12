using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Filters;
using Proyecto3wed.Models;
using Proyecto3wed.Services;

namespace Proyecto3wed.Controllers;

[RequireLogin]
public class MiBibliotecaController : Controller
{
    private readonly BibliotecaApiClient _api;

    public MiBibliotecaController(BibliotecaApiClient api) => _api = api;

    private async Task CargarLibrosDisponiblesAsync(CancellationToken ct)
    {
        var libros = await _api.GetLibrosAsync(ct) ?? new();
        ViewBag.Libros = libros.Where(l => l.CantidadDisponible > 0).OrderBy(l => l.Titulo).ToList();
    }

    public async Task<IActionResult> Prestamos(CancellationToken ct)
    {
        var data = await _api.GetPrestamosAsync(ct);
        if (data is null)
        {
            TempData["Err"] = "No se pudieron cargar tus prestamos. Verifica que la API este activa y vuelve a intentar.";
            data = new();
        }

        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> SolicitarPrestamo(CancellationToken ct)
    {
        await CargarLibrosDisponiblesAsync(ct);
        return View(new CrearPrestamoForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SolicitarPrestamo(CrearPrestamoForm model, CancellationToken ct)
    {
        if (model.LibroId <= 0 || model.Cantidad < 1)
        {
            ModelState.AddModelError(string.Empty, "Selecciona un libro y una cantidad vÃ¡lida.");
        }

        var librosDisponibles = await _api.GetLibrosAsync(ct) ?? new();
        var libroSeleccionado = librosDisponibles.FirstOrDefault(l => l.Id == model.LibroId);
        if (libroSeleccionado is null)
        {
            ModelState.AddModelError(string.Empty, "El libro seleccionado ya no esta disponible.");
        }
        else if (model.Cantidad > libroSeleccionado.CantidadDisponible)
        {
            ModelState.AddModelError(
                nameof(model.Cantidad),
                $"Solo hay {libroSeleccionado.CantidadDisponible} ejemplar(es) disponible(s) de este libro.");
        }

        if (!ModelState.IsValid)
        {
            await CargarLibrosDisponiblesAsync(ct);
            return View(model);
        }

        var (ok, err) = await _api.CrearPrestamoAsync(model.LibroId, model.Cantidad, model.DiasPrestamo, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, err ?? "No se pudo crear el prÃ©stamo.");
            await CargarLibrosDisponiblesAsync(ct);
            return View(model);
        }

        ViewBag.Success = "Solicitud registrada correctamente.";
        ViewBag.SuccessDetail = "Tu prestamo fue creado y ya aparece en Mis prestamos.";
        await CargarLibrosDisponiblesAsync(ct);
        return View(new CrearPrestamoForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int id, CancellationToken ct)
    {
        var (ok, err) = await _api.DevolverPrestamoAsync(id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "DevoluciÃ³n registrada." : (err ?? "No se pudo devolver.");
        return RedirectToAction(nameof(Prestamos));
    }

    public async Task<IActionResult> Favoritos(CancellationToken ct)
    {
        var data = await _api.GetFavoritosAsync(ct);
        if (data is null)
        {
            TempData["Err"] = "No se pudieron cargar tus favoritos. Verifica que la API este activa y vuelve a intentar.";
            data = new();
        }

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarFavorito(int libroId, CancellationToken ct)
    {
        var (ok, err) = await _api.AddFavoritoAsync(libroId, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "AÃ±adido a favoritos." : (err ?? "Error");
        return RedirectToAction("Detalle", "Catalogo", new { id = libroId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarFavorito(int libroId, CancellationToken ct)
    {
        await _api.RemoveFavoritoAsync(libroId, ct);
        TempData["Msg"] = "Eliminado de favoritos.";
        return RedirectToAction(nameof(Favoritos));
    }

    public async Task<IActionResult> Reservas(CancellationToken ct)
    {
        var data = await _api.GetReservasAsync(ct);
        if (data is null)
        {
            TempData["Err"] = "No se pudieron cargar tus reservas. Verifica que la API este activa y vuelve a intentar.";
            data = new();
        }

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearReserva(int libroId, CancellationToken ct)
    {
        var (ok, err) = await _api.CrearReservaAsync(libroId, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Reserva realizada correctamente" : (err ?? "No se pudo realizar la reserva.");
        return RedirectToAction("Detalle", "Catalogo", new { id = libroId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarReserva(int id, CancellationToken ct)
    {
        await _api.CancelarReservaAsync(id, ct);
        TempData["Msg"] = "Reserva cancelada.";
        return RedirectToAction(nameof(Reservas));
    }

    public async Task<IActionResult> Notificaciones(CancellationToken ct)
    {
        var data = await _api.GetNotificacionesAsync(ct);
        if (data is null)
        {
            TempData["Err"] = "No se pudieron cargar tus avisos. Verifica que la API este activa y vuelve a intentar.";
            data = new();
        }

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLeida(int id, CancellationToken ct)
    {
        try
        {
            await _api.MarcarNotificacionLeidaAsync(id, ct);
        }
        catch
        {
            /* ignorar */
        }

        return RedirectToAction(nameof(Notificaciones));
    }

    public async Task<IActionResult> Multas(CancellationToken ct)
    {
        var data = await _api.GetMultasAsync(ct);
        if (data is null)
        {
            TempData["Err"] = "No se pudieron cargar tus multas.";
            data = new();
        }
        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagarMulta(int id, CancellationToken ct)
    {
        var (ok, err) = await _api.MarcarMultaPagadaAsync(id, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Multa registrada como pagada." : (err ?? "No se pudo registrar el pago.");
        return RedirectToAction(nameof(Multas));
    }

    public IActionResult PagoQr(int id, decimal monto)
    {
        ViewBag.Monto = monto;
        ViewBag.Referencia = $"MUL-{id}-{DateTime.Now:yyyyMMdd}";
        ViewBag.Operacion = new Random().Next(100000, 999999).ToString();
        return View();
    }
}
