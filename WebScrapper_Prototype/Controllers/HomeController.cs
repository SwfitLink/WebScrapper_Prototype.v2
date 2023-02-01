using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using X.PagedList;

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
        [HttpPost]
		public IActionResult CountDownProduct()
		{
			return View();
		}
		[HttpGet]
		public IActionResult CountDownProduct(int? page)
        {
			var products = from p in _context.Product
						   select p;
            decimal? HighDiff = 0;
			Dictionary<int, decimal?> dic = new Dictionary<int, decimal?>();
			List<decimal> arr = new List<decimal>();
            foreach(var item in products)
            {
                decimal? diff = item.ProductBasePrice + item.ProductSalePrice;

				if (diff > HighDiff)
                {
                    HighDiff = diff;
                    dic.Add(item.ID, HighDiff);
                }
			}
            foreach(KeyValuePair<int, decimal?> pair in dic)
            {
				if (!dic.ContainsValue(HighDiff))
				{
                    dic.Remove(pair.Key);
                }
                products = products.Where(a => a.ID.Equals(pair.Key));
			}     
            return View(products);            
		}
		[HttpPost]
		public IActionResult Index()
        {
            return View();
        }
		[HttpGet]
		public IActionResult Index(string sortOrder, string findProduct, string filter,Boolean state, string currentFilter, string searchString, string location, int? page)
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
            var products = from p in _context.Product
                            select p;
            products = products.Where(s => s.Visible.Contains("Visible"));
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.ProductCategory.Contains(searchString)
                       || s.ProductName.Contains(searchString));
            }
            switch (findProduct)
            {
                case "All":
                    products = products.OrderBy(s => s.ProductCategory);
                    break;
                case "laptops":
                    products = products.Where(s => s.ProductCategory.Contains("Laptops"));
                    break;
                case "desktop":
                    products = products.Where(s => s.ProductCategory.Contains("Pre-Built PC"));
                    break;
                case "hardware":
                    products = products.Where(s => s.ProductCategory.Contains("Hardware"));
                    break;
                case "accessories":
                    products = products.Where(s => s.ProductCategory.Contains("Accessories"));
                    break;
                default:
                    break;
            }
            switch (sortOrder)
            {
                case "new":
                    products = products.Where(s => s.ProductStatus.Equals("New"));
                    break;
                case "savings":
                    //products = products.Where(s =>
                    //s.ProductSalePrice < s.ProductBasePrice / 2);
                    break;
                case "price_desc":
                    products = products.OrderBy(s => s.ProductSalePrice);
                    break;
                default:
                    break;        
            }
            if (!String.IsNullOrEmpty(filter))
            {
                products = products.Where(a => a.ProductName.Contains(filter));
            }
			int pageSize = 12;
            int pageNumber = (page ?? 1);
            var onePageOfProducts = products.ToPagedList(pageNumber, pageSize);
            ViewBag.OnePageOfProducts = onePageOfProducts;
            return View(onePageOfProducts);
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