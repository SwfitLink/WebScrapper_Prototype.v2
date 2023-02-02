using Microsoft.AspNetCore.Mvc;

namespace WebScrapper_Prototype.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ManualProductAdd()
        {
            return (RedirectToAction("Create", "Products", new { view = "Visible" }));
        }
        public IActionResult BulkInsertProduct()
        {
            return (RedirectToAction("AutoProductCreateTest", "Products", new { view = "Hidden" }));
        }
        public IActionResult AllProducts()
        {
            return (RedirectToAction("Index", "Products", new { view = "All" }));
        }
    }
}
