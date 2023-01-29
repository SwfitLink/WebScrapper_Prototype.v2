using Microsoft.AspNetCore.Mvc;
using PagedList;
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

        public IActionResult Index(string sortOrder, string findProduct, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentProduct = findProduct;
            ViewBag.ProductStatusSortParm = String.IsNullOrEmpty(sortOrder) ? "new" : "";
            ViewBag.LowestPriceParm = sortOrder == "price_desc" ? "price_desc" : "price_desc";
            ViewBag.SavingParm = sortOrder == "savings" ? "savings" : "savings";


            ViewBag.ProductCatParm = String.IsNullOrEmpty(findProduct) ? "All" : "";
            ViewBag.LaptopParm = findProduct == "laptops" ? "laptops" : "laptops";
            ViewBag.DesktopParm = findProduct == "desktop" ? "desktop" : "desktop";
            ViewBag.HardwareParm = findProduct == "hardware" ? "hardware" : "hardware";
            ViewBag.AccessoriesParm = findProduct == "accessories" ? "accessories" : "accessories";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;

            }
            ViewBag.CurrentFilter = searchString;
            var productsT = from p in _context.Product
                            select p;
            productsT = productsT.Where(s => s.Visible.Contains("Visible"));
            if (!String.IsNullOrEmpty(searchString))
            {
                productsT = productsT.Where(s => s.ProductCategory.Contains(searchString)
                       || s.ProductName.Contains(searchString));
            }
            switch (findProduct)
            {
                case "All":
                    productsT = productsT.OrderBy(s => s.ProductCategory);
                    break;
                case "laptops":
                    productsT = productsT.Where(s => s.ProductCategory.Contains("Laptops"));
                    break;
                case "desktop":
                    productsT = productsT.Where(s => s.ProductCategory.Contains("Pre-Built PC"));
                    break;
                case "hardware":
                    productsT = productsT.Where(s => s.ProductCategory.Contains("Hardware"));
                    break;
                case "accessories":
                    productsT = productsT.Where(s => s.ProductCategory.Contains("Accessories"));
                    break;
                default:
                    break;
            }
            switch (sortOrder)
            {
                case "new":
                    productsT = productsT.Where(s => s.ProductStatus.Equals("New"));
                    break;
                case "savings":
                    //productsT = productsT.Where(s =>
                    //s.ProductSalePrice < s.ProductBasePrice / 2);
                    break;
                case "price_desc":
                    productsT = productsT.OrderBy(s => s.ProductSalePrice);
                    break;
                default:
                    break;
            }
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(productsT.ToPagedList(pageNumber, pageSize));
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
        public IActionResult ReleaseNotes()
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