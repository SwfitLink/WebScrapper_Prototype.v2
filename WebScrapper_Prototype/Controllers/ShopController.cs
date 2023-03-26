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
using Microsoft.AspNetCore.Http;

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
		private static string userEmail;
		private static string userFirstName;
		private static int basketCounter;

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
		/// Responsible for the Shop RazorView (Deprecated)
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
		/// Responsible for the Shop RazorView
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Index(int basketStart, int productIdW, int wishlistStart, string priceRange,int removeProduct, int addProduct, string category, string searchString, int page)
		{
			/// Cookies ///
			userEmail = await CheckUserCookie();
			if (userEmail.Contains("cookie"))
			{
				ViewBag.IsCookie = true;
				ViewBag.FirstName = 
					"Please Login or\n" +
					"Register a New Account";
			}
			else
			{
				ViewBag.IsCookie = false;
				userFirstName = _context.UserModels.FirstOrDefault(s => s.Email.Equals(userEmail)).FirstName;
				ViewBag.FirstName = userFirstName;
			}
			if (basketCounter == 0)
				ViewBag.ItemCount = "*";
			else
				ViewBag.ItemCount = basketCounter;

			/// External Functions ///
			if (basketStart > 0)
			{	
				ViewBag.BasketLoad = 1; 
				var basket = LoadShoppingBasket();
				basketCounter = basket.TotalItemCount;
				ViewBag.ItemCount = basketCounter;
				return View(basket);	
			}
			if (addProduct > 0)
				return View(await AddToCart(addProduct));
			if (removeProduct > 0)
				return View(await RemoveFromCart(removeProduct));
			if (wishlistStart > 0)
				return View(LoadWishList());
			if (productIdW > 0)
				AddToWishList(productIdW);			

			/// Internal Functions ///			
			var products = _context.Products.Where(s => s.Visible!.Equals("Visible"));
			// Function : Search Products
			if (!string.IsNullOrEmpty(searchString))
			{
				Console.WriteLine("------------------\n" + $"Selecting Products: {searchString}");
				products = products.Where(p => p.ProductName!.Contains(searchString));
				Console.WriteLine("------------------\n");
			}
			// Function : Product Filter = Price
			if (!string.IsNullOrEmpty(priceRange))
			{
				// We can avoid repeating the switch cases and simplify the code by defining 
				// the minimum and maximum prices for each range and filtering based on that.
				int minPrice = 0;
				int maxPrice = int.MaxValue;
				switch (priceRange)
				{
					case "R0 - R 1000":
						maxPrice = 1000;
						break;
					case "R1 000 - R 5000":
						minPrice = 1000;
						maxPrice = 5000;
						break;
					case "R5 000 - R10 000":
						minPrice = 5000;
						maxPrice = 10000;
						break;
					case "R10 000 - R20 000":
						minPrice = 10000;
						maxPrice = 20000;
						break;
				}
				Console.WriteLine("------------------\n" + $"Selecting Products: {priceRange}");
				products = products.Where(p => p.ProductSalePrice >= minPrice && p.ProductSalePrice <= maxPrice);
				ViewBag.PriceRange = "Price Range: " + priceRange;
				Console.WriteLine("------------------\n");
			}
			// Function : Product Filter = Category
			if (!String.IsNullOrEmpty(category))
			{
				Console.WriteLine("------------------\n" + $"Selecting Products: {category}");
				if (category.Equals("All"))
				{					
					ViewBag.Category = null;
					return View(products);
				}
				else
				{
					products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Equals(category));
					ViewBag.Category = category;					
				}
				return View(products);

			}
			else
			{
				Console.WriteLine("------------------\n" + $"Selecting Products: Default");
				products = products.Where(s => s.ProductCategory != null && (s.ProductCategory.Equals("GPUs") ||
				s.ProductCategory.Equals("CPUs") ||
				s.ProductCategory.Equals("Notebooks") ||
				s.ProductCategory.Equals("Monitors") ||
				s.ProductCategory.Equals("Peripherals") ||
				s.ProductCategory.Equals("Chassis")));
				Console.WriteLine("------------------\n");
			}
			// Places products into groups depending on their price.
			var priceRanges = products
				.GroupBy(p => p.ProductSalePrice <= 1000 ? "PriceRange1" :
				  p.ProductSalePrice >= 1000 && p.ProductSalePrice <= 5000 ? "PriceRange2" :
				  p.ProductSalePrice >= 5000 && p.ProductSalePrice <= 10000 ? "PriceRange3" :
				  p.ProductSalePrice >= 10000 && p.ProductSalePrice <= 20000 ? "PriceRange4" : "")
				.Select(g => new { PriceRange = g.Key, Count = g.Count() })
				.ToList();
			// Displays the number of products in each price range
			ViewBag.PriceRange1 = priceRanges.FirstOrDefault(p => p.PriceRange == "PriceRange1")?.Count;
			ViewBag.PriceRange2 = priceRanges.FirstOrDefault(p => p.PriceRange == "PriceRange2")?.Count;
			ViewBag.PriceRange3 = priceRanges.FirstOrDefault(p => p.PriceRange == "PriceRange3")?.Count;
			ViewBag.PriceRange4 = priceRanges.FirstOrDefault(p => p.PriceRange == "PriceRange4")?.Count;
			// PagedList : Enables pagination of products within RazorView!
			int pageNumber = 1;
			int pageSize = 8;
			ViewBag.ItemCount = "*";
			IPagedList<Product> pagedList = products.ToPagedList();
			if (page > 0)
			{
				pageNumber = page;
				pagedList = products.ToPagedList(pageNumber, pageSize);
				return View(products);
			}			
			return View(pagedList);
		}
		/// <summary>
		/// Responsible for the Cart RazorView
		/// </summary>
		[HttpGet]
		public IActionResult Cart()
		{
			if (userEmail.Contains("cookie"))
			{
				ViewBag.IsCookie = true;
				ViewBag.FirstName =
					"Please Login Into Your Account or\n" +
					"Register a New Account";
			}
			else
			{
				ViewBag.IsCookie = false;
				userFirstName = _context.UserModels.FirstOrDefault(s => s.Email.Equals(userEmail)).FirstName;
				ViewBag.FirstName = userFirstName;
			}
			Queue<decimal?> basePrice = new();
			Queue<decimal?> salePrice = new();
			Queue<decimal?> cartTotal = new();
			decimal? total = 0;
			var products = from p in _context.Products
						   select p;
			var query = from p in _context.Products
						join b in _context.ShopingBasket.Where(s => s.UserId.Equals(userEmail))
						on p.Id equals b.ProductKey
						select new { p, b };
			products = query.Select(q => q.p);
			foreach (var item in products)
			{
				basePrice.Enqueue(item.ProductBasePrice);
				salePrice.Enqueue(item.ProductSalePrice);
			}
			if (salePrice.Sum() > 1500)
			ViewBag.ShippingCost = "FREE";
			else
			{
				total = 1;
				ViewBag.ShippingCost = 300;
				ViewBag.GetFreeShipping = 1000 - salePrice.Sum();
			}
			if (total == 1)
			ViewBag.CartTotalSale = salePrice.Sum() + 300;
			else
			ViewBag.CartTotalSale = salePrice.Sum();
			ViewBag.BasketCount = products.Count();
			ViewBag.Savings = basePrice.Sum() - salePrice.Sum();
			ViewBag.CartTotalBase = basePrice.Sum();
			basePrice.Clear();
			salePrice.Clear();
			return View(products.ToPagedList(1, 100));
		}
		/// <summary>
		/// Responsible for the Product RazorView
		/// </summary>
		[HttpGet]
		public IActionResult Product(int id)
		{
			if (userEmail.Contains("cookie"))
			{
				ViewBag.IsCookie = true;
				ViewBag.FirstName =
					"Please Login Into Your Account or\n" +
					"Register a New Account";
			}
			else
			{
				ViewBag.IsCookie = false;
				userFirstName = _context.UserModels.FirstOrDefault(s => s.Email.Equals(userEmail)).FirstName;
				ViewBag.FirstName = userFirstName;
			}
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.Id == id);
			}
			return View(products.ToPagedList());
		}
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
		/// Responsible for Shopping Basket Overlay 
		/// </summary>
		[HttpGet]
		public IPagedList LoadShoppingBasket()
		{			
			Queue<decimal?> basePrice = new();
			Queue<decimal?> salePrice = new();
			Queue<decimal?> cartTotal = new();
			// Get user's shopping basket
			var basket = _context.ShopingBasket
				.Where(b => b.UserId.Equals(userEmail))
				.Select(s => s.ProductKey);

			var products = _context.Products
				.Where(p => basket.Contains(p.Id));

			// Check if products are not null
			if (products != null)
			{
				// Get base and sale prices for each product
				foreach (var p in products)
				{
					if (p.ProductBasePrice != null)
						basePrice.Enqueue(p.ProductBasePrice.Value);

					if (p.ProductSalePrice != null)
						salePrice.Enqueue(p.ProductSalePrice.Value);					
				}

				// Calculate shipping cost and display information in ViewBag
				if (salePrice.Sum() > 1500)
					ViewBag.ShippingCost = "FREE";
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
				ViewBag.HandlingFee = "R250";
				else
					ViewBag.HandlingFee = "R50";

				// Clear basePrice and salePrice queues
				basePrice.Clear();
				salePrice.Clear();
			}
			// Return the view for the user's shopping basket
			return products.ToPagedList();
		}
		/// <summary>
		/// Responsible for Wish List Overlay (Deprecated)
		/// </summary>
		[HttpGet]
		public IPagedList LoadWishList()
		{
			// Get user's wish list
			var wishlist = from w in _context.UserWishList.Where(b => b.UserId.Equals(userEmail))
						 select w;
			var productID = wishlist.Select(w => w.ProductKey).FirstOrDefault();
			var products = from p in _context.Products.Where(p => p.Id == productID)
						   select p;
			ViewBag.Wishlist = 1;
			return products.ToPagedList();


		}
		/// <summary>
		/// Responsible for Cookies
		/// </summary>
		private async Task<string> CheckUserCookie()
		{
			UserDetailsService detailsService = new(_context, _httpContextAccessor, _app, _userManager, _signInManager);
			const string cookieName = "wazawarecookie6";
			var requestCookies = HttpContext.Request.Cookies;
			var firstRequest = requestCookies[cookieName];
			// If user is not authenticated
			if (User?.Identity?.IsAuthenticated == false)
			{
				var cookieOptions = new CookieOptions
				{
					Expires = DateTimeOffset.Now.AddDays(7),
					IsEssential = true
				};
				// Checks for User Cookie
				if (!requestCookies.ContainsKey(cookieName))
				{
					userEmail = await detailsService.Cookie();
					// Create new User Cookie					
					HttpContext.Response.Cookies.Append(cookieName, userEmail, cookieOptions);
				}
				else
				{
					userEmail = await detailsService.Cookie();
					HttpContext.Response.Cookies.Append(cookieName, userEmail, cookieOptions);
				}
			}
			if (!String.IsNullOrEmpty(firstRequest))
				return firstRequest;
			else
				userEmail = await detailsService.Cookie();
			return userEmail;
		}
		/// <summary>
		/// External Function Responsible for Adding Products to User Shopping Basket & Cart
		/// </summary>
		[HttpPost]
		public async Task<IPagedList> AddToCart(int productId)
		{
			var products = from p in _context.Products.Where(p => p.Id == productId)
						   select p;
			//var product = _context.Products.FirstOrDefault(s => s.Id == addProduct);
			var basket = new Basket
			{
				ProductKey = productId,
				UserId = userEmail,
				createdAt = DateTime.Now
			};
			_context.Attach(basket);
			_context.Entry(basket).State = EntityState.Added;
			await _context.SaveChangesAsync();
		
			return products.ToPagedList();
		}
		/// <summary>
		/// External Function Responsible for Adding Products to User Shopping Basket & Cart
		/// </summary>
		[HttpPost]
		public async Task<IPagedList> RemoveFromCart(int productId)
		{
			var basket = from b in _context.ShopingBasket
						 select b;
			var products = from p in _context.Products.Where(p => p.Id == productId)
						   select p;
			var productToRemove = basket.Where(s => s.ProductKey == productId).FirstOrDefault();
			if(basket != null && productToRemove != null)
			{
				_context.AttachRange(productToRemove);
				_context.RemoveRange(productToRemove);
				await _context.SaveChangesAsync();
			}
			return products.ToPagedList();
		}
		/// <summary>
		/// External Function Responsible for Adding Products to User Wish List (Deprecated)
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
				wishList.ProductKey = productId;
				wishList.UserId = userEmail;
				_context.Attach(wishList);
				_context.Entry(wishList).State = EntityState.Added;
				_context.SaveChanges();
			}
		}
		/// <summary>
		/// External Function Responsible for Removing Products from User Shopping Basket & Cart (Deprecated -> Still removes for Wish List)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int BasketRemovePID, int WishListRemovePID)
		{
			if (WishListRemovePID > 0)
			{
				var wishlist = from w in _context.UserWishList.Where(s => s.UserId.Equals(userEmail))
							   select w;
				var wishlistRowsToDelete = wishlist.Where(w => w.ProductKey == WishListRemovePID);

				if (wishlistRowsToDelete != null)
				{
					_context.UserWishList.RemoveRange(wishlistRowsToDelete.First());
				}
				await _context.SaveChangesAsync();
				return RedirectToAction("Index", new { wishlistStart = 1 });
			}
			if (BasketRemovePID > 0)
			{
				var basket = from b in _context.ShopingBasket.Where(s => s.UserId.Equals(userEmail))
							 select b;
				var basketRowsToDelete = basket.Where(b => b.ProductKey == BasketRemovePID);
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