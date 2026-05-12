using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Infrastructure;
using Proyecto3wed.Models;
using Proyecto3wed.Services;

namespace Proyecto3wed.Controllers;

public class AccountController : Controller
{
    private readonly BibliotecaApiClient _api;

    public AccountController(BibliotecaApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginFormModel model, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var (ok, err, data) = await _api.LoginAsync(model.Email, model.Password, ct);
        if (!ok || data is null)
        {
            ModelState.AddModelError(string.Empty, err ?? "Error de inicio de sesiÃ³n.");
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        SetSession(data);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (data.Rol is "Administrador" or "Bibliotecario")
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        return RedirectToAction("Index", "Catalogo");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Err"] = "Ingresa un correo valido.";
            return RedirectToAction(nameof(Login));
        }

        var (ok, err, data) = await _api.ForgotPasswordAsync(model.Email, ct);
        if (!ok)
        {
            TempData["Err"] = err ?? "No se pudo enviar el codigo.";
            return RedirectToAction(nameof(Login));
        }

        TempData["Msg"] = data?.ResetToken is { Length: > 0 }
            ? $"Codigo generado. En desarrollo usa este codigo: {data.ResetToken}"
            : (data?.Mensaje ?? "Si el correo existe, recibiras instrucciones.");
        return RedirectToAction(nameof(ResetPassword), new { token = data?.ResetToken });
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token) => View(new ResetPasswordFormModel { Token = token ?? string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (ok, err) = await _api.ResetPasswordAsync(model.Token, model.NewPassword, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, err ?? "No se pudo cambiar la contrasena.");
            return View(model);
        }

        TempData["Msg"] = "Contrasena actualizada. Ya puedes iniciar sesion.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

                var (ok, err, data) = await _api.RegisterAsync(
            model.NombreCompleto, 
            model.Email, 
            model.Password, 
            model.Telefono, 
            model.Carrera, 
            model.Direccion, 
            ct);
        if (!ok || data is null)
        {
            ModelState.AddModelError(string.Empty, err ?? "No se pudo registrar.");
            return View(model);
        }

        SetSession(data);
        TempData["Msg"] = "Cuenta creada. Bienvenido.";
        return RedirectToAction("Index", "Catalogo");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Catalogo");
    }

    private void SetSession(AuthResponseDto data)
    {
        HttpContext.Session.SetString(SessionKeys.AccessToken, data.AccessToken);
        HttpContext.Session.SetString(SessionKeys.RefreshToken, data.RefreshToken);
        HttpContext.Session.SetString(SessionKeys.UserEmail, data.Email);
        HttpContext.Session.SetString(SessionKeys.UserName, data.NombreCompleto);
        HttpContext.Session.SetString(SessionKeys.UserRole, data.Rol);
    }
}

