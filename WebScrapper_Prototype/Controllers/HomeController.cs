using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Linq;
using WebScrapper_Prototype.Services;

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
		[HttpGet]
		public IActionResult WishList(int userId)
		{
			var products = from p in _context.Products
						   select p;
			var wishList = from w in _context.UserWishList
						 select w;
			var query = from p in _context.Products
						join w in _context.UserWishList
						on p.ID equals w.ProductId
						select new { p, w };
			products = query.Select(q => q.p);
			return View(products.ToPagedList());
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
		public IActionResult Index(int productId, int productIdW, int wishlistStart, int BasketRemovePID, int basketId, string sortOrder, string findProduct, string filter, string currentFilter, string searchString, decimal? total, string action, int? page)
		{
			Queue<decimal?> basePrice = new Queue<decimal?>();
			Queue<decimal?> salePrice = new Queue<decimal?>();
			Queue<decimal?> cartTotal = new Queue<decimal?>();
			int pageSize = 12;
			int pageNumber = (page ?? 1);
			var products = from p in _context.Products
						   select p;
			var basket = from b in _context.ShopingBasket
						 select b;
			var wishlist = from w in _context.UserWishList
						 select w;
			products = products.Where(p => p.Visible.Equals("Visible"));
			if (productId > 0)
			{
				var a = AddToCart(productId);
				basket = basket.Where(b => b.BasketId.Equals(a));
			}
			if (productIdW > 0)
			{
				var v = AddToWishList(productIdW);
				wishlist = wishlist.Where(w => w.ProductId.Equals(v));
			}
			if (searchString != null)
			{
				page = 1;
			}
			else
			{
				searchString = currentFilter;
			}
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
			if(wishlistStart == 1)
			{
				var query = from p in _context.Products
							join w in _context.UserWishList.Where(s => s.UserId.Equals(getUser().Result))
							on p.ID equals w.ProductId
							select new { p, w };
				products = query.Select(q => q.p);
				ViewBag.Wishlist = 1;
				return View(products.ToPagedList());
			}
			else
			{
				ViewBag.Wishlist = 0;
			}
			if (basketId == 1)
			{
				var query = from p in _context.Products
							join b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUser().Result))
							on p.ID equals b.ProductId
							select new { p, b };
				products = query.Select(s => s.p);
				foreach (var item in products)
				{
					basePrice.Enqueue(item.ProductBasePrice);
					salePrice.Enqueue(item.ProductSalePrice);
				}
				if (salePrice.Sum() > 1500)
				{
					ViewBag.ShippingCost = "FREE";
				}
				else
				{
					ViewBag.ShippingCost = 300;
					ViewBag.GetFreeShipping = 1000 - salePrice.Sum();
				}
				ViewBag.BasketCount = products.Count();
				ViewBag.Savings = basePrice.Sum() - salePrice.Sum();
				ViewBag.CartTotalSale = salePrice.Sum();
				ViewBag.CartTotalBase = basePrice.Sum();
				basePrice.Clear();
				salePrice.Clear();
				ViewBag.BasketLoad = 1;
				ViewBag.BasketCount = products.Count();
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
        [Authorize(Roles = "User, Manager")]
        [HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int BasketRemovePID, int WishListRemovePID)
		{
			if (WishListRemovePID > 0)
			{
				var wishlist = from w in _context.UserWishList.Where(s => s.UserId.Equals(getUser().Result))
							   select w;
				var wishlistRowsToDelete = wishlist.Where(w => w.ProductId == WishListRemovePID);

				if (wishlistRowsToDelete != null)
				{
					_context.UserWishList.RemoveRange(wishlistRowsToDelete.First());
				}
				await _context.SaveChangesAsync();
				return RedirectToAction("Index", new { wishlistStart = 1 });
			}
			if (BasketRemovePID > 0)
			{
				var basket = from b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUser().Result))
                             select b;
				var basketRowsToDelete = basket.Where(b => b.ProductId == BasketRemovePID);

				if (basketRowsToDelete != null)
				{
					_context.ShopingBasket.RemoveRange(basketRowsToDelete.First());
				}
				await _context.SaveChangesAsync();
				return RedirectToAction("Index", new { BasketID = 1 });
			}
			return RedirectToAction("Index", new { wishlistStart = 0 });
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
				Basket shoppingBasket = new Basket();
				shoppingBasket.ProductId = productId;
				shoppingBasket.BasketId = getUser().Result;
				_context.Attach(shoppingBasket);
				_context.Entry(shoppingBasket).State = EntityState.Added;
				_context.SaveChanges();
				
				return shoppingBasket.BasketId;
			}
			return null;
		}
		[Authorize(Roles = "User, Manager")]
		[HttpPost]
		public string AddToWishList(int productId)
		{
			var prod = from p in _context.Products
					   select p;
			if (productId > 0)
			{
				prod = prod.Where(p => p.ID == productId);
				UserWishList wishList = new UserWishList();
				wishList.ProductId = productId;
				wishList.UserId = getUser().Result;
				_context.Attach(wishList);
				_context.Entry(wishList).State = EntityState.Added;
				_context.SaveChanges();

				return wishList.UserId;
			}
			return null;
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
		public ActionResult UnderConstruction()
		{
			return View();
		}
	}
}