using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using WebScrapper_Prototype.Areas.Identity.Data;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using WebScrapper_Prototype.Services;

namespace WebScrapper_Prototype.Controllers
{
	public class ShopController : Controller
	{
		private readonly ILogger<ShopController> _logger;
		private readonly WebScrapper_PrototypeContext _context;
		private readonly ApplicationDbContext _app;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public ShopController(ILogger<ShopController> logger, WebScrapper_PrototypeContext context, ApplicationDbContext app, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_logger = logger;
			_app = app;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}
		/// <summary>
		/// Handles Product View
		/// </summary>
		[HttpGet]
		public IActionResult Product(int id)
		{
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.Id == id);
			}
			return View(products.ToPagedList());
		}
		/// <summary>
		/// Handles User Shopping Cart ViewBags and other Informative Elements
		/// (This should be moved to a Service in the Future!)
		/// </summary>
		[HttpGet]
		public IActionResult Cart()
		{
			Queue<decimal?> basePrice = new Queue<decimal?>();
			Queue<decimal?> salePrice = new Queue<decimal?>();
			Queue<decimal?> cartTotal = new Queue<decimal?>();
			decimal? total = 0;
			var products = from p in _context.Products
						   select p;
			var query = from p in _context.Products
						join b in _context.ShopingBasket.Where(s => s.UserId.Equals(getUserEmail()))
						on p.Id equals b.ProductId
						select new { p, b };
			products = query.Select(q => q.p);
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
				total = 1;
				ViewBag.ShippingCost = 300;
				ViewBag.GetFreeShipping = 1000 - salePrice.Sum();
			}
			if (total == 1)
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
		/// <summary>
		/// Handles Shop Grid & Related Functions
		/// </summary>
		[HttpPost]
		public IActionResult Index(int id)
		{
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.Id == id);
			}
			return View(products.ToPagedList());
		}
		/// <summary>
		/// Handles Shop Grid & Related Functions
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Index(int basketStart, int productIdW, int wishlistStart, string priceRange, int productId, string manufacturer, string category, string searchString, int? page, int pageSize)
		{
			// User Details Cookie
			await CheckUserCookie();
			// Search for Product
			if (!String.IsNullOrEmpty(searchString))
				SearchProducts(searchString);
			ViewBag.CurrentFilter = searchString;
			// Load User Shopping Cart
			if (basketStart > 0)
			{
				// Set ViewBag for basket load status
				ViewBag.BasketLoad = 1;
				return View(LoadShoppingBasket());
			}			
			// Adding Product to Basket
			if (productId > 0)
				AddToCart(productId);

			// Load User Wish List
			if (wishlistStart > 0)
				return View(LoadWishList());
			// Add Product to Wish List
			if (productIdW > 0)
				AddToWishList(productIdW);

			// Index() Functions:
			var products = from p in _context.Products
						   select p;
			var basket = from b in _context.ShopingBasket
						 select b;
			var wishlist = from w in _context.UserWishList
						   select w;
			products = products.Where(p => p.Visible != null && p.Visible.Equals("Visible"));

			// Function 1: Product Filter = Price
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

			// Function 2: Product Filter = Manufacturer (Not in use!)
			if (!String.IsNullOrEmpty(manufacturer))
			{
				products = products.Where(s => s.ProductName != null && s.ProductName.Contains(manufacturer));
				ViewBag.manufacturer = manufacturer;
			}

			// Function 3: Product Filter = Category
			if (!String.IsNullOrEmpty(category))
			{
				if (category.Equals("All"))
				{
					products = products.OrderBy(s => s.ProductCategory);
				}
				else
				{
					products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Equals(category));
					ViewBag.Category = category;
				}
				var product = from p in _context.Products
							  select p;
				var p1 = product;
				p1 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice <= 1000 && s.ProductCategory != null && s.ProductCategory.Equals(category));
				ViewBag.PriceRange1 = p1.Count();

				var p2 = product;
				p2 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice >= 1000 && s.ProductSalePrice <= 5000 && s.ProductCategory != null && s.ProductCategory.Equals(category));
				ViewBag.PriceRange2 = p2.Count();

				var p3 = product;
				p3 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice >= 5000 && s.ProductSalePrice <= 10000 && s.ProductCategory != null && s.ProductCategory.Equals(category));
				ViewBag.PriceRange3 = p3.Count();

				var p4 = product;
				p4 = product.OrderBy(s =>
				s.ProductSalePrice).Where(s =>
				s.ProductSalePrice >= 10000 && s.ProductSalePrice <= 20000 && s.ProductCategory != null && s.ProductCategory.Equals(category));
				ViewBag.PriceRange4 = p4.Count();
			}
			else
			{
				products = products.Where(s => 
				s.ProductCategory != null && s.ProductCategory.Equals("GPUs") || 
				s.ProductCategory != null && s.ProductCategory.Equals("CPUs") || 
				s.ProductCategory != null && s.ProductCategory.Equals("Notebooks") || 
				s.ProductCategory != null && s.ProductCategory.Equals("Monitors") || 
				s.ProductCategory != null && s.ProductCategory.Equals("Peripherals") || 
				s.ProductCategory != null && s.ProductCategory.Equals("Chassis"));
			}

			// Populates Default Shop Grid & ViewBags
			if (pageSize == 0)
			{
				pageSize = 24;
			}
			int pageNumber = (page ?? 1);
			var onePageOfProducts = products.ToPagedList(pageNumber, pageSize);
			ViewBag.OnePageOfProducts = onePageOfProducts;
			ViewBag.ProductCount = onePageOfProducts.Count;
			return View(onePageOfProducts);
		}
		/// <summary>
		/// Search for Products
		/// </summary>
		[HttpGet]
		public IPagedList SearchProducts(string searchString)
		{
			var products = from p in _context.Products
						   select p;
			products = products.Where(p => p.Visible != null && p.Visible.Equals("Visible"));
			if (!String.IsNullOrEmpty(searchString))
			{
				// Split search string into individual words and search for each word
				var words = searchString.Split(' ');
				foreach (var word in words)
				{
					// Search for products that contain each word
					products = products.Where(s => s.ProductName != null && s.ProductName.Contains(word));
				}
			}
			// Return Products
			return products.ToPagedList(1, 100);
		}
		/// <summary>
		/// Loads Shopping Basket for User
		/// </summary>
		[HttpGet]
		public IPagedList LoadShoppingBasket()
		{
			Queue<decimal?> basePrice = new Queue<decimal?>();
			Queue<decimal?> salePrice = new Queue<decimal?>();
			Queue<decimal?> cartTotal = new Queue<decimal?>();
			// Get user's shopping basket
			var basket = from b in _context.ShopingBasket.Where(b => b.UserId.Equals(getUserEmail().ToString()))
						 select b.ProductId;
			var products = from p in _context.Products
						   where basket.Contains(p.Id)
						   select p;

			// Check if products are not null
			if (products != null)
			{
				// Get base and sale prices for each product
				foreach (var p in products)
				{
					if (p.ProductBasePrice != null)
					{
						basePrice.Enqueue(p.ProductBasePrice.Value);
					}
					if (p.ProductSalePrice != null)
					{
						salePrice.Enqueue(p.ProductSalePrice.Value);
					}
				}

				// Calculate shipping cost and display information in ViewBag
				if (salePrice.Sum() > 1500)
				{
					ViewBag.ShippingCost = "FREE";
				}
				else
				{
					ViewBag.ShippingCost = 300;
					ViewBag.GetFreeShipping = 1000 - salePrice.Sum();
				}

				// Display basket count, savings, cart total sale, and cart total base in ViewBag
				ViewBag.BasketCount = products.Count();
				ViewBag.Savings = basePrice.Sum() - salePrice.Sum();
				ViewBag.CartTotalSale = salePrice.Sum();
				ViewBag.CartTotalBase = basePrice.Sum();

				// Display handling fee in ViewBag
				if (salePrice.Sum() > 5000)
				{
					ViewBag.HandlingFee = "R250";
				}
				else
				{
					ViewBag.HandlingFee = "R50";
				}

				// Clear basePrice and salePrice queues
				basePrice.Clear();
				salePrice.Clear();


			}
			// Return the view for the user's shopping basket
			return products.ToPagedList(1, 100);
		}
		/// <summary>
		/// Loads Wish List for User
		/// </summary>
		[HttpGet]
		public IPagedList LoadWishList()
		{
			// Get user's wish list
			var wishlist = from w in _context.UserWishList.Where(b => b.UserId.Equals(getUserEmail().ToString()))
						 select w;
			var productID = wishlist.Select(w => w.ProductId).FirstOrDefault();
			var products = from p in _context.Products.Where(p => p.Id == productID)
						   select p;
			ViewBag.Wishlist = 1;
			return products.ToPagedList();


		}
		/// <summary>
		/// Checks if there is a cookie present for the user, if not, creates one and signs in the user.
		/// </summary>
		private async Task CheckUserCookie()
		{
			UserDetailsService detailsService = new UserDetailsService(_context, _httpContextAccessor, _app, _userManager, _signInManager);
			// If user is not authenticated
			if (User?.Identity?.IsAuthenticated == false)
			{
				const string cookieName = "SwiftLinkUserCookieVer5.5A";
				var requestCookies = HttpContext.Request.Cookies;
				// Checks for User Cookie
				if (!requestCookies.ContainsKey(cookieName))
				{
					// Create cookie options
					var cookieOptions = new CookieOptions
					{
						Expires = DateTimeOffset.Now.AddDays(7),
						IsEssential = true
					};
					// Create new User Cookie
					HttpContext.Response.Cookies.Append(cookieName, await detailsService.ManageUser(), cookieOptions);
				}
				else
				{
					// Get User Cookie
					var firstRequest = requestCookies[cookieName];
					if (firstRequest == null)
					{
						// If the cookie cannot be found, return error message
						ViewData["ErrorMessage"] = "Cookie Failed";
					}
					// Sign in user using cookie
					if(firstRequest != null)
						await detailsService.SignIn(firstRequest);
				}
			}
		}
		/// <summary>
		/// Add Product to Shopping Basket
		/// </summary>
		[HttpPost]
		public void AddToCart(int productId)
		{
			var prod = from p in _context.Products
					   select p;
			if (productId > 0)
			{
				prod = prod.Where(p => p.Id == productId);
				Basket shoppingBasket = new Basket();
				shoppingBasket.ProductId = productId;
				shoppingBasket.UserId = getUserEmail();
				_context.Attach(shoppingBasket);
				_context.Entry(shoppingBasket).State = EntityState.Added;
				_context.SaveChanges();
			}
		}
		/// <summary>
		/// Add Product to Wish List
		/// </summary>
		[HttpPost]
		public void AddToWishList(int productId)
		{
			var prod = from p in _context.Products
					   select p;
			if (productId > 0)
			{
				prod = prod.Where(p => p.Id == productId);
				UserWishList wishList = new UserWishList();
				wishList.ProductId = productId;
				wishList.UserId = getUserEmail();
				_context.Attach(wishList);
				_context.Entry(wishList).State = EntityState.Added;
				_context.SaveChanges();
			}
		}
		/// <summary>
		/// Handles Product Removal from both User Wish List and Basket
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int BasketRemovePID, int WishListRemovePID)
		{
			if (WishListRemovePID > 0)
			{
				var wishlist = from w in _context.UserWishList.Where(s => s.UserId.Equals(getUserEmail()))
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
				var basket = from b in _context.ShopingBasket.Where(s => s.UserId.Equals(getUserEmail()))
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
		/// <summary>
		/// Gets User Email
		/// </summary>
		private string getUserEmail()
		{
			UserDetailsService userDetailsService = new UserDetailsService(_httpContextAccessor);
			string? email = userDetailsService.GetSignedInUserEmail();
			if (email != null)
				return email;
			else return string.Empty;
		}
	}
}