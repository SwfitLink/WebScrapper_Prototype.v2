using Microsoft.AspNetCore.Mvc;

namespace WebScrapper_Prototype.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
