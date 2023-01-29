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
            return (RedirectToAction("Create", "Product", new { view = "Visible" }));
        }
        public IActionResult BulkInsertProduct()
        {
            return (RedirectToAction("AutoProductCreateTest", "Product", new { view = "Hidden" }));
        }
        public IActionResult AllProducts()
        {
            return (RedirectToAction("Index", "Product", new { view = "All" }));
        }
    }
}
