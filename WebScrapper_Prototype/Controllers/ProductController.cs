using Microsoft.AspNetCore.Mvc;

namespace WebScrapper_Prototype.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
