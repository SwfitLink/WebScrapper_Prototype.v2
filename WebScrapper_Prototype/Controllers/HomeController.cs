using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Drawing.Printing;
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
		[HttpPost]
		public IActionResult Index(int id)
        {
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.ID== id);

			}
			return View(products.ToPagedList());
		}
		[HttpGet]
		public IActionResult Index(string sortOrder, string findProduct, string filter, int id, string currentFilter, string searchString, string location, int? page)
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
			var products = from p in _context.Products
						   select p;
			products = products.Where(p => p.Visible.Equals("Visible"));
			if (searchString != null)
			{
				page = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			ViewBag.CurrentFilter = searchString;
			if (!String.IsNullOrEmpty(searchString))
			{
				products = products.Where(s => s.ProductCategory.Contains(searchString)
					   || s.ProductName.Contains(searchString));
			}

			if (!String.IsNullOrEmpty(filter))
			{
				Console.WriteLine("Looking for Products that contain:" + filter);
				switch (filter)
				{
					case "Intel":
						products = products.Where(s => s.ProductName.Contains(filter));
						break;
					case "AMD":
						break;
					case "Nvidia":
						break;
				}
			}			
			int pageSize = 12;
			int pageNumber = (page ?? 1);
			return View(products.ToPagedList(pageNumber, pageSize));
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