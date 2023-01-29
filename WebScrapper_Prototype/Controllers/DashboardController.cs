using Microsoft.AspNetCore.Mvc;

namespace WebScrapper_Prototype.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ListedProducts()
        {
            return (RedirectToAction("Index", "Product", new { view = "Visible" }));
        }
        public IActionResult HiddenProducts()
        {
            return (RedirectToAction("Index", "Product", new { view = "Hidden" }));
        }
        public IActionResult AllProducts()
        {
            return (RedirectToAction("Index", "Product", new { view = "All" }));
        }
    }
}
