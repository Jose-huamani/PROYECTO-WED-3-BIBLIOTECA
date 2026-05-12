using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Proyecto3wed.Infrastructure;

namespace Proyecto3wed.Filters;

public sealed class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var token = context.HttpContext.Session.GetString(SessionKeys.AccessToken);
        if (string.IsNullOrEmpty(token))
        {
            var path = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = path.ToString() });
        }
    }
}

public sealed class RequireStaffAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString(SessionKeys.UserRole);
        if (role != "Administrador" && role != "Bibliotecario")
        {
            context.Result = new RedirectToActionResult("Index", "Catalogo", null);
        }
    }
}
