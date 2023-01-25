using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WebScrapper_PrototypeContext _context;

        public HomeController(ILogger<HomeController> logger, WebScrapper_PrototypeContext context)
        {
            _logger = logger;
            _context = context;

        }

        public IActionResult Index(string searchString)
        {            
            List<Product> products = _context.Product.ToList();
            Dictionary<int, string> search = new Dictionary<int, string>();        
            if (!String.IsNullOrEmpty(searchString))
            {
                search.Clear();             
                foreach (var item in products)
                {
                    search.Add(item.ID, item.ProductName.ToUpper());
                }
               return View(products.Where(q => q.ID.Equals(SearchDictionary(search, searchString.ToUpper()).Key)));              
            }
            return View(products);
        }
        public static KeyValuePair<int, string> SearchDictionary(Dictionary<int, string> dict, string searchTerm)
        {
            foreach (KeyValuePair<int, string> pair in dict)
            {
                if (pair.Value.Contains(searchTerm))
                {
                    return pair;
                }
            }
            return new KeyValuePair<int, string>(-1, "Search term not found.");
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