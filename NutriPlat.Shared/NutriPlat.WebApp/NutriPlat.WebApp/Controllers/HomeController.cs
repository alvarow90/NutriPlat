// NutriPlat.WebApp/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using NutriPlat.WebApp.Models; // Para ErrorViewModel
using System.Diagnostics;

namespace NutriPlat.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Esta acción se ejecuta cuando navegas a la raíz de tu WebApp (ej. / o /Home/Index)
        public IActionResult Index()
        {
            // Esto le dice a ASP.NET Core que busque y renderice el archivo:
            // Views/Home/Index.cshtml
            return View();
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
