using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using System.Diagnostics;
using X.PagedList;
using X.PagedList.Web.Common;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WebScrapper_Prototype.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using System.Drawing.Printing;
using System.Linq;

namespace WebScrapper_Prototype.Controllers
{

	public class ShopController : Controller
	{
		private readonly ILogger<ShopController> _logger;
		private readonly WebScrapper_PrototypeContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public ShopController(ILogger<ShopController> logger, WebScrapper_PrototypeContext context, UserManager<ApplicationUser> userManager)
		{
			_logger = logger;
			_context = context;
			_userManager = userManager;

		}
		[Authorize(Roles = "User, Manager")]
		[HttpGet]
		public IActionResult Cart(int basketId)
		{
			Queue<decimal?> basePrice = new Queue<decimal?>();
			Queue<decimal?> salePrice = new Queue<decimal?>();
			Queue<decimal?> cartTotal = new Queue<decimal?>();
			decimal? total = 0;
			var products = from p in _context.Products
						   select p;
			var query = from p in _context.Products
						join b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUser().Result))
						on p.ID equals b.ProductId
						select new { p, b };
			products = query.Select(q => q.p);
			foreach (var item in products)
			{
				basePrice.Enqueue(item.ProductBasePrice);
				salePrice.Enqueue(item.ProductSalePrice);
			}
			if(salePrice.Sum() > 1500)
			{
				ViewBag.ShippingCost = "FREE";				
			}
			else
			{
				total = 1;
				ViewBag.ShippingCost = 300;
				ViewBag.GetFreeShipping = 1000 - salePrice.Sum();
			}
			if(total == 1)
			{
				ViewBag.CartTotalSale = salePrice.Sum() + 300;

			}
			else
			{
				ViewBag.CartTotalSale = salePrice.Sum();

			}
			ViewBag.BasketCount = products.Count();
			ViewBag.Savings = basePrice.Sum() - salePrice.Sum();
			ViewBag.CartTotalBase = basePrice.Sum();
			basePrice.Clear();
			salePrice.Clear();
			return View(products.ToPagedList(1, 100));
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
		public IActionResult Index(int basketId, int productIdW, int wishlistStart, string priceRange, int productId, string manufacturer, string category, string searchString, string currentFilter, int? page, int pageSize)
		{
			Queue<decimal?> basePrice = new Queue<decimal?>();
			Queue<decimal?> salePrice = new Queue<decimal?>();
			Queue<decimal?> cartTotal = new Queue<decimal?>();
			var products = from p in _context.Products
						   select p;
			var basket = from b in _context.ShopingBasket
						 select b;
			var wishlist = from w in _context.UserWishList
						   select w;
			products = products.Where(p => p.Visible.Equals("Visible"));
			if (!String.IsNullOrEmpty(searchString))
			{
				var words = searchString.Split(' ');
				foreach (var word in words)
				{
					products = products.Where(s => s.ProductName.Contains(word));

				}
				ViewBag.CurrentFilter = searchString;
			}
			if (productId > 0)
			{
			var a = AddToCart(productId);
			}
			if(productIdW > 0)
			{
				var v = AddToWishList(productIdW);
				wishlist = wishlist.Where(w => w.ProductId.Equals(v));
			}
			if (!String.IsNullOrEmpty(priceRange))
			{
				ViewBag.PriceRange = "Price Range: " + priceRange;
				switch (priceRange)
				{
					case "All":
						products = products.OrderBy(s => s.ProductBasePrice);
						ViewBag.PriceRange = " ";

						break;
					case "R0 - R 1000":
						products = products.OrderBy(s => 
						s.ProductSalePrice).Where(s => 
						s.ProductSalePrice <= 1000);
						break;
					case "R1 000 - R 5000":
						products = products.OrderBy(s =>
						s.ProductSalePrice).Where(s =>
						s.ProductSalePrice >= 1000 && s.ProductSalePrice <= 5000);
						break;
					case "R5 000 - R10 000":
						products = products.OrderBy(s =>
						s.ProductSalePrice).Where(s =>
						s.ProductSalePrice >= 5000 && s.ProductSalePrice <= 10000);
						break;
					case "R10 000 - R20 000":
						products = products.OrderBy(s =>
						s.ProductSalePrice).Where(s =>
						s.ProductSalePrice >= 10000 && s.ProductSalePrice <= 20000);
						break;
				}
			}
			if (searchString != null)
			{
				page = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			if (wishlistStart == 1)
			{
				var query = from p in _context.Products
							join w in _context.UserWishList.Where(s => s.UserId.Equals(getUser().Result))
							on p.ID equals w.ProductId
							select new { p, w };
				products = query.Select(q => q.p);
				ViewBag.Wishlist = 1;
				return View(products.ToPagedList());
			}
			if (basketId == 1)
			{
				var query = from p in _context.Products
							join b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUser().Result))
								on p.ID equals b.ProductId
							select new { p, b };
				products = query.Select(q => q.p);			
				foreach (var p in products)
				{
					basePrice.Enqueue(p.ProductBasePrice);
					salePrice.Enqueue(p.ProductBasePrice);
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
				return View(products.ToPagedList(1, 100));
			}
			else
			{
				ViewBag.BasketLoad = 0;
			}
			if(!String.IsNullOrEmpty(manufacturer)) 
			{
				products = products.Where(s => s.ProductName.Contains(manufacturer));
				ViewBag.Manufacturer = "Manufacturer: " + manufacturer;
			}
			if (!String.IsNullOrEmpty(category))
			{
				if (category.Equals("All"))
				{
					products = products.OrderBy(s => s.ProductCategory);
				}
				else
				{
					products = products.Where(s => s.ProductCategory.Equals(category));
					ViewBag.Category = "Category: " + category;
				}
				var product = from p in _context.Products
							   select p;
				var p1 = product;
				p1 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice <= 1000 && s.ProductCategory.Equals(category));
				ViewBag.PriceRange1 = p1.Count();

				var p2 = product;
				p2 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice >= 1000 && s.ProductSalePrice <= 5000 && s.ProductCategory.Equals(category));
				ViewBag.PriceRange2 = p2.Count();

				var p3 = product;
				p3 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice >= 5000 && s.ProductSalePrice <= 10000 && s.ProductCategory.Equals(category));
				ViewBag.PriceRange3 = p3.Count();

				var p4 = product;
				p4 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice >= 10000 && s.ProductSalePrice <= 20000 && s.ProductCategory.Equals(category));
				ViewBag.PriceRange4 = p4.Count();
			}
			else
			{
				products = products.Where(s => s.ProductCategory.Equals("GPUs") || s.ProductCategory.Equals("CPUs") || s.ProductCategory.Equals("Notebooks") || s.ProductCategory.Equals("Monitors") || s.ProductCategory.Equals("Peripherals") || s.ProductCategory.Equals("Chassis"));
			}
			if (pageSize == 0)
			{
				pageSize = 24;
			}
			else
			{
				Console.WriteLine(pageSize + "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
			}
			int pageNumber = (page ?? 1);
			var onePageOfProducts = products.ToPagedList(pageNumber, pageSize);
			ViewBag.OnePageOfProducts = onePageOfProducts;
			ViewBag.ProductCount = onePageOfProducts.Count;
			return View(onePageOfProducts);
		}
        [HttpGet]
        public IActionResult Product(int id)
        {
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.ID == id);

			}
			return View(products.ToPagedList());
		}
        public async Task<string> getUser()
		{
			var user = await _userManager.GetUserAsync(User);
			var email = user.Email;
			return email;
		} 
		[Authorize(Roles = "User, Manager")]
		[HttpPost]
		public int AddToCart(int productId)
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

				return shoppingBasket.Id;
			}
			return 0;
		}
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
		[HttpGet]
		public IActionResult GetProductsInRange(decimal? minValue, decimal? maxVaue)
		{
			var products = from p in _context.Products
						   select p;
			products = products.Where(s => s.ProductSalePrice > minValue && s.ProductSalePrice < maxVaue);
			int pageSize = 25;
			var onePageOfProducts = products.ToPagedList(0, pageSize);
			return View(nameof(Index), onePageOfProducts);
		}
		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int BasketRemovePID, int WishListRemovePID, int IDB)
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
	}
}
