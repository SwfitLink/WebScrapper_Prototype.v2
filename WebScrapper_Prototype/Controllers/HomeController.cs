using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Drawing.Printing;
using WebMatrix.WebData;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Migrations.WebScrapper_Prototype;
using WebScrapper_Prototype.Models;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using NuGet.ContentModel;

namespace WebScrapper_Prototype.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly WebScrapper_PrototypeContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		public HomeController(ILogger<HomeController> logger, WebScrapper_PrototypeContext context, UserManager<ApplicationUser> userManager)
		{
			_logger = logger;
			_context = context;
			_userManager = userManager;
		}
		[HttpPost]
		public IActionResult Index(int id)
		{
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.ID == id);

			}
			return View(products.ToPagedList());
		}
		[HttpGet]
		public IActionResult Index(int productId, int BasketRemovePID, int basketId, string sortOrder, string findProduct, string filter, string currentFilter, string searchString, decimal? total, string action, int? page)
		{
			ViewBag.CurrentSort = sortOrder;
			ViewBag.CurrentProduct = findProduct;
			ViewBag.ProductStatusSortParm = String.IsNullOrEmpty(sortOrder) ? "new" : "";
			ViewBag.LowestPriceParm = sortOrder == "price_desc" ? "price_desc" : "price_desc";
			ViewBag.SavingParm = sortOrder == "savings" ? "savings" : "savings";

			ViewBag.ProductCatParm = String.IsNullOrEmpty(findProduct) ? "All" : "";
			ViewBag.CatLaptopParm = findProduct == "laptops" ? "laptops" : "laptops";
			ViewBag.CatDesktopParm = findProduct == "desktop" ? "desktop" : "desktop";
			ViewBag.CatHardwareParm = findProduct == "hardware" ? "hardware" : "hardware";
			ViewBag.CatAccessoriesParm = findProduct == "accessories" ? "accessories" : "accessories";
			ViewBag.Total = total;
			int pageSize = 12;
			int pageNumber = (page ?? 1);
			var products = from p in _context.Products
						   select p;
			var basket = from b in _context.ShopingBasket
						 select b;
			products = products.Where(p => p.Visible.Equals("Visible"));
			var a = AddToCart(productId);
			basket = basket.Where(b => b.BasketId.Equals(a));
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
				products = products.Where(s => s.ProductName.Contains(searchString));
			}
			if (!String.IsNullOrEmpty(filter))
			{
				Console.WriteLine("Looking for Products that contain:" + filter);
				switch (filter)
				{
					case "Intel":
						products = products.Where(s => s.ProductName.Contains(filter) ||
						s.ProductName.Contains("i7") ||
						s.ProductName.Contains("i5"));
						break;
					case "AMD":
						products = products.Where(s => s.ProductName.Contains(filter) ||
						s.ProductName.Contains("Ryzen"));
						break;
					case "Nvidia":
						products = products.Where(s => s.ProductName.Contains(filter) ||
						s.ProductName.Contains("GeForce") ||
						s.ProductName.Contains("RTX"));
						break;
				}
			}
			if(basketId == 1)
			{	
				var query = from p in _context.Products
							join b in _context.ShopingBasket
							on p.ID equals b.ProductId
							select new { p, b };
				products = query.Select(q => q.p);
				ViewBag.BasketLoad = 1;
				return View(products.ToPagedList(1,10));
			}
			else
			{			
				ViewBag.BasketLoad = 0;
			}
			var pagedProducts = products.ToPagedList(pageNumber, pageSize);
			return View(pagedProducts);
		}
		public async Task<string> getUser()
		{
			var user = await _userManager.GetUserAsync(User);
			var email = user.Email;
			return email;
		}
		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int BasketRemovePID)
		{
			var basket = from b in _context.ShopingBasket
						 select b;
					var basketRowsToDelete = basket.Where(b => b.ProductId == BasketRemovePID);

			if (basketRowsToDelete != null)
			{
				_context.ShopingBasket.RemoveRange(basketRowsToDelete);
			}
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", new { BasketID = 1 });
		}
		[Authorize(Roles = "User, Manager")]
		[HttpPost]
		public string AddToCart(int productId)
		{
			var prod = from p in _context.Products
					   select p;
			if (productId > 0)
			{
				prod = prod.Where(p => p.ID == productId);
				ShoppingBasket shoppingBasket = new ShoppingBasket();
				shoppingBasket.ProductId = productId;
				shoppingBasket.BasketId = getUser().Result;
				_context.Attach(shoppingBasket);
				_context.Entry(shoppingBasket).State = EntityState.Added;
				_context.SaveChanges();
				
				return shoppingBasket.BasketId;
			}
			return null;
		}
		[HttpPost]
		public ActionResult CreateBasket()
		{
			ShoppingBasket basket = new ShoppingBasket
			{
				BasketId = getUser().Result,
				ProductId = 0
			};
			_context.Attach(basket);
			_context.Entry(basket).State = EntityState.Added;
			_context.SaveChanges();
			return View();
		}
		public IActionResult Checkout()
		{
			if (getUser().Result != null)
			{
				var userEmail = getUser().Result;
				var items = from b in _context.ShopingBasket
							select b;
				var prod = from p in _context.Products
						   select p;
				if (items.Where(s => s.BasketId.Equals(userEmail)) == null)
				{
					CreateBasket();
				}
				items = items.Where(s => s.BasketId.Equals(userEmail));
				foreach (var i in items)
				{
					prod = prod.Where(p => p.ID == i.ProductId).Distinct();
				}

				return View(prod.ToPagedList());
			}
			return RedirectToAction(nameof(Index));
		}
		public IActionResult GoShop(string categories)
		{
			ViewBag.CategoryParm = categories;
			ViewBag.CategoryGPUParm = categories == "GPUs" ? "GPUs" : "GPUs";
			if (!String.IsNullOrEmpty(categories))
			{
				switch (categories)
				{
					case "GPUs":
						return RedirectToAction("Index", "Shop", new { categories = "GPUs" });

				}
			}
			return RedirectToAction("Index", "Shop", new { categories = ViewBag.CategoryParm });
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