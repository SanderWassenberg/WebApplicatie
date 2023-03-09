using Microsoft.AspNetCore.Mvc;
using SwapGame_MVC.Models;
using System.Diagnostics;

namespace SwapGame_MVC.Controllers
{
    public class MainPageController : Controller
    {
        private readonly ILogger<MainPageController> _logger;

        public MainPageController(ILogger<MainPageController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
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