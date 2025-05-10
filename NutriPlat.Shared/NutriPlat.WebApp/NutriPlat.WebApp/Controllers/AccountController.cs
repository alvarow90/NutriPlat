// NutriPlat.WebApp/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;

namespace NutriPlat.WebApp.Controllers
{
    public class AccountController : Controller
    {
        // Acción para mostrar la página de inicio de sesión
        public IActionResult Login()
        {
            // Esta acción buscará una vista en Views/Account/Login.cshtml
            return View();
        }

        // Podrías añadir una acción para el registro aquí también en el futuro
        // public IActionResult Register()
        // {
        //     return View(); // Buscaría Views/Account/Register.cshtml
        // }

        // Después del login exitoso desde JavaScript, podrías redirigir a un Dashboard
        // Esta acción es solo un ejemplo de a dónde podría ir el usuario.
        // No la llamaremos directamente desde el formulario de login (eso lo hará JS).
        // [Authorize] // Si tuvieras autenticación basada en cookies en WebApp
        public IActionResult Dashboard()
        {
            // Esta acción buscaría una vista en Views/Account/Dashboard.cshtml o Views/Home/Dashboard.cshtml etc.
            // Por ahora, solo un placeholder.
            // ViewData["UserName"] = HttpContext.Session.GetString("userName"); // Ejemplo si usaras sesiones
            return View(); // Necesitarás crear Views/Account/Dashboard.cshtml
        }
    }
}
