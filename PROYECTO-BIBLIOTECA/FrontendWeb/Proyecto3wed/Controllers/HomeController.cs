using Microsoft.AspNetCore.Mvc;
using Proyecto3wed.Infrastructure;
using Proyecto3wed.Models;
using System.Diagnostics;

namespace Proyecto3wed.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            var loggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.AccessToken));

            if (loggedIn)
            {
                if (role == "Administrador" || role == "Bibliotecario")
                    return RedirectToAction("Dashboard", "Admin");
                else
                    return RedirectToAction("Dashboard", "Usuario");
            }
            
            return RedirectToAction("Index", "Catalogo");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
