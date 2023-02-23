using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using WebScrapper_Prototype.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using System.Security.Claims;
using System.Text;

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
		private string emailPersis = string.Empty;

		public ShopController(ILogger<ShopController> logger, WebScrapper_PrototypeContext context, ApplicationDbContext app, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_logger = logger;
			_app = app;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}
		[Authorize(Roles = "User, Manager")]
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
						join b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUserEmail().Result))
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
		public async Task<IActionResult> Index(int basketStart, int productIdW, int wishlistStart, string priceRange, int productId, string manufacturer, string category, string searchString, string currentFilter, int? page, int pageSize)
		{
			CookieOptions cookieOptions = new CookieOptions();
			cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));
			cookieOptions.IsEssential = true;
			if (!HttpContext.Request.Cookies.ContainsKey("user_cookie3"))
			{
				await cookieUser("404");
				HttpContext.Response.Cookies.Append("user_cookie3", emailPersis.ToString(), cookieOptions);
			}
			else
			{
				var firstRequest = HttpContext.Request.Cookies["user_cookie3"];
				emailPersis = firstRequest.ToString();
				await cookieUser(firstRequest.ToString());
			}
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
				AddToCart(productId);
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
							join w in _context.UserWishList.Where(s => s.UserId.Equals(getUserEmail().Result))
							on p.ID equals w.ProductId
							select new { p, w };
				products = query.Select(q => q.p);
				ViewBag.Wishlist = 1;
				return View(products.ToPagedList());
			}
			if (basketStart == 1)
			{
				var query = from p in _context.Products
							join b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUserEmail().Result))
								on p.ID equals b.ProductId
							select new { p, b };
				products = query.Select(q => q.p);			
				foreach (var p in products)
				{
					basePrice.Enqueue(p.ProductBasePrice);
					salePrice.Enqueue(p.ProductSalePrice);
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
				if(salePrice.Sum() > 5000)
				{
					ViewBag.HandlingFee = "R250";
				}
				else
				{
					ViewBag.HandlingFee = "R50";
				}
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
				ViewBag.manufacturer = manufacturer;
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
					ViewBag.Category = category;
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
		public async Task<IActionResult> cookieUser(string token)
		{
			if (token.Equals("404"))
			{
				var user = CreateUser();
				string generatedEmail = "cookie" + Guid.NewGuid().ToString() + "@swiftlink.com";
				user.FirstName = "User";
				user.LastName = "UserlAT";
				user.Email = generatedEmail;
				user.PhoneNumber = "1234567890";
				user.CountryCode = "ZA";
				user.UserName = generatedEmail;
				user.NormalizedUserName = generatedEmail;
				user.NormalizedEmail = generatedEmail;
				user.PasswordHash = "AQAAAAEAACcQAAAAEH+MdVIf9JyT0rq9Q/+YY8LEzJtSkL1kCdWQPPQTo06tmiiuDbpWMfaDthgRlIMfXg==";
				IdentityUser identity = user;
				identity.UserName = user.UserName;
				identity.Email = user.Email;
				identity.NormalizedEmail = user.NormalizedEmail;
				identity.NormalizedUserName = user.NormalizedUserName;
				identity.PasswordHash = user.PasswordHash;
				identity.PhoneNumber = user.PhoneNumber;
				_app.Attach(user);
				_app.Entry(user).State = EntityState.Added;
				_app.SaveChanges();
				user = await _signInManager.UserManager.FindByEmailAsync(user.Email);				
				emailPersis = user.Email;
				var result = await _signInManager.CheckPasswordSignInAsync(user, "Markaway86!", false);
				if (result.Succeeded)
				{
					var claims = new List<Claim>
					{
						new Claim("arr", "pwd"),
					};
					var roles = await _signInManager.UserManager.GetRolesAsync(user);
					if (roles.Any())
					{
						//Gives Cookie [USER] Role
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					else
					{
						await _signInManager.UserManager.AddToRoleAsync(user, "User");
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					await _signInManager.SignInWithClaimsAsync(user, true, claims);				 
					_logger.LogInformation("User logged in.");
				}
				return RedirectToPage("Index", "Shop");
			}
			else
			{
				var userA = await _signInManager.UserManager.FindByEmailAsync(token);
				if(userA != null)//Debug Purpose (So that when switching Conn strings no error)
				{
					if (User?.Identity.IsAuthenticated == false)
					{
						var result = await _signInManager.CheckPasswordSignInAsync(userA, "Markaway86!", false);
						if (result.Succeeded)
						{
							var claims = new List<Claim>
					{
						new Claim("arr", "pwd"),
					};
							var roles = await _signInManager.UserManager.GetRolesAsync(userA);
							if (roles.Any())
							{
								//Gives Cookie [USER] Role
								var roleClaim = string.Join(",", roles);
								claims.Add(new Claim("Roles", roleClaim));
							}
							else
							{
								await _signInManager.UserManager.AddToRoleAsync(userA, "User");
								await _userManager.UpdateAsync(userA);
								var roleClaim = string.Join(",", roles);
								claims.Add(new Claim("Roles", roleClaim));
							}
							await _signInManager.SignInWithClaimsAsync(userA, true, claims);
							_logger.LogInformation("User logged in.");
						}
						return RedirectToPage("Index", "Shop");
					}
				}
				else
				{
					await cookieUser("404");
				}			
				return View();
			}
		}
		private ApplicationUser CreateUser()
		{
			try
			{
				return Activator.CreateInstance<ApplicationUser>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
					$"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
					$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
			}
		}
		public async Task<string> getUserEmail()
		{
			if (!String.IsNullOrEmpty(emailPersis))
			{
				var user = await _userManager.FindByEmailAsync(emailPersis);
				var email = user.Email;
				return email;
			}
			else
			{
				var user = await _userManager.GetUserAsync(User);
				var email = user.Email;
				return email;
			}			
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
				shoppingBasket.BasketId = getUserEmail().Result;
				shoppingBasket.createdAt= DateTime.Now;
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
				wishList.UserId = getUserEmail().Result;
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
				var wishlist = from w in _context.UserWishList.Where(s => s.UserId.Equals(getUserEmail().Result))
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
				var basket = from b in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUserEmail().Result))
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
		//public async Task<bool> CheckForUpdatesAsync()
		//{
		//	// set lastCheckedTime to the current time
		//	var lastCheckedTime = DateTime.UtcNow;
		//	// check if any new items have been added to the shopping basket	
		//	var newItemsAdded = await _context.ShopingBasket.Where(i => i.createdAt > lastCheckedTime).AnyAsync();
			

		//	// check if any items in the shopping basket have been updated or removed
		//	var itemsUpdatedOrRemoved = await _context.ShopingBasket.Where(i => i.createdAt > lastCheckedTime || i.IsDeleted).AnyAsync();



		//	return newItemsAdded || itemsUpdatedOrRemoved;
		//}

		//public async Task<string> GetShoppingBasketAsync()
		//{
		//	// query the database for the current contents of the shopping basket
		//	var shoppingBasketItems = await dbContext.ShoppingBasketItems
		//		.Include(i => i.Item)
		//		.Where(i => i.ShoppingBasketId == basketId)
		//		.OrderByDescending(i => i.DateAdded)
		//		.ToListAsync();

		//	// format the shopping basket items as HTML
		//	var sb = new StringBuilder();
		//	sb.AppendLine("<ul>");
		//	foreach (var item in shoppingBasketItems)
		//	{
		//		sb.AppendLine($"<li>{item.Item.Name} ({item.Quantity})</li>");
		//	}
		//	sb.AppendLine("</ul>");

		//	return sb.ToString();
		//}

		//public async Task<IActionResult> Updates()
		//{
		//	Response.Headers.Add("Content-Type", "text/event-stream");
		//	Response.Headers.Add("Cache-Control", "no-cache");

		//	while (true)
		//	{
		//		// check for updates in the database
		//		var updateAvailable = await CheckForUpdatesAsync();

		//		if (updateAvailable)
		//		{
		//			// send the updated shopping basket to the client
		//			var shoppingBasket = await GetShoppingBasketAsync();
		//			var data = $"data: {shoppingBasket}\n\n";
		//			await Response.WriteAsync(data);
		//			await Response.Body.FlushAsync();
		//		}

		//		// wait for a short time before checking again
		//		await Task.Delay(5000);
		//	}
		//}
	}
}
