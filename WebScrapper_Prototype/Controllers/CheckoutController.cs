using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web.Helpers;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using WebScrapper_Prototype.Services;

namespace WebScrapper_Prototype.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly ILogger<ShopController> _logger;
		private readonly WebScrapper_PrototypeContext _context;
		private readonly ApplicationDbContext _app;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private string emailPersis = string.Empty;

		public CheckoutController(ILogger<ShopController> logger, WebScrapper_PrototypeContext context, ApplicationDbContext app, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_logger = logger;
			_app = app;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}
		[HttpGet]
		public async Task<IActionResult> Index(string updateCookie, int showOrder, int startPlaceOrder)
		{
			decimal orderSubTotal = 0;
			decimal shippingTotal = 0;
			decimal fee = 0;
			decimal orderGrandTotal = 0;
			AutoUserService userService = new AutoUserService(_app, _signInManager, _userManager, _context);
			/// -----------Cookies-----------
			/// ↓SUMMARY↓: Checks for Cookie & Signs User
			/// ↓----------------------------
			if (!String.IsNullOrEmpty(updateCookie))
			{
				CookieOptions cookieOptions = new CookieOptions();
				cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));
				cookieOptions.IsEssential = true;

				HttpContext.Response.Cookies.Append("cookieV5.3.1B", updateCookie, cookieOptions);
				ViewBag.UpdateCookie = "You may Proceed to checkout. Thank You!";
			}
			if (!User?.Identity.IsAuthenticated == false)
			{
				if (getUserEmail() != null)
				{
					Console.WriteLine("COOKIE: WORKING...");
				}
				else
				{
					Console.WriteLine("ERRO: COOKIE IS NULL");
				}
			}
			else
			{
				if (!HttpContext.Request.Cookies.ContainsKey("cookieV5.3.1B"))
				{
					CookieOptions cookieOptions = new CookieOptions();
					cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));
					cookieOptions.IsEssential = true;
					HttpContext.Response.Cookies.Append("cookieV5.3.1B", userService.ManageUser().Result, cookieOptions);

				}
				else
				{
					var firstRequest = HttpContext.Request.Cookies["cookieV5.3.1B"];
					await userService.signInUser(firstRequest.ToString());
				}
				if (getUserEmail() != null)
				{
					Console.WriteLine("COOKIE: WORKING...");
				}
				else
				{
					Console.WriteLine("ERRO: COOKIE IS NULL");
				}
			}
			var userEmail = getUserEmail();
			if (userEmail.Contains("cookie"))
			{
				ViewBag.userIsCookie = true;
			}
			else
			{
				ViewBag.userIsCookie = false;
			}
			var products = _context.Products
				.Join(_context.ShopingBasket.Where(s => s.BasketId.Equals(userEmail)), p => p.ID, b => b.ProductId, (p, b) => p)
				.ToList();
			var salePriceSum = products.Sum(p => p.ProductSalePrice);
			ViewBag.BasketCount = products.Count();
			ViewBag.Savings = products.Sum(p => p.ProductBasePrice - p.ProductSalePrice);
			ViewBag.CartTotalBase = products.Sum(p => p.ProductBasePrice);
			if (salePriceSum > 1500)
			{
				ViewBag.ShippingCost = "FREE";
				ViewBag.CartTotalSale = salePriceSum;
				shippingTotal = 0;
			}
			else
			{
				ViewBag.ShippingCost = 300;
				shippingTotal = 300;
				ViewBag.GetFreeShipping = 1500 - salePriceSum;
				ViewBag.CartTotalSale = salePriceSum + 300;
			}
			/// --------HandlingFee----------
			/// ↓SUMMARY↓: First Iteration
			/// ↓----------------------------
			decimal handlingFee;
			int productCount = products.Count;
			decimal cartAverage = (decimal)(salePriceSum / productCount);
			/// -------HandlingFee-----------
			/// ↓SUMMARY↓: First Iteration
			/// ↓----------------------------
			switch (productCount)
			{
				/// -----------------------------
				/// ↓SUMMARY↓:
				/// ShoppingCart has (1) Product,
				/// +R50 Handling Fee
				/// ↓----------------------------
				case 1:
					handlingFee = 50;
					break;
				/// -----------------------------
				/// ↓SUMMARY↓:
				/// ShoppingCart has (2) Products,
				/// ShoppingCart Average is LESS / MORE than R1500?
				/// LESS: +R50 Handling Fee || MORE: +R100 Handling Fee
				/// ↓----------------------------
				case 2:
					handlingFee = cartAverage < 1500 ? 50 : 100;
					break;
				/// -----------------------------
				/// ↓SUMMARY↓:
				/// ShoppingCart has (3) Products,
				/// ShoppingCart Average is LESS / MORE than R1500?
				/// LESS: +R80 Handling Fee || MORE: +R100 Handling Fee
				/// ↓----------------------------
				case 3:
					handlingFee = cartAverage < 1500 ? 80 : 100;
					break;
				/// -----------------------------
				/// ↓SUMMARY↓:
				/// ShoppingCart has (4) Products,
				/// ShoppingCart Average is LESS / MORE than R1500?
				/// LESS: +R100 Handling Fee || MORE: +R120 Handling Fee
				/// ↓----------------------------
				case 4:
					handlingFee = cartAverage < 1500 ? 100 : 120;
					break;
				/// -----------------------------
				/// ↓SUMMARY↓:
				/// ShoppingCart has (5) Products,
				/// ShoppingCart Average is LESS / MORE than R1500?
				/// LESS: +R120 Handling Fee || MORE: +R150 Handling Fee
				/// ↓----------------------------
				case 5:
					handlingFee = cartAverage < 1500 ? 120 : 150;
					break;
				/// -----------------------------
				/// ↓SUMMARY↓:
				/// ShoppingCart has More than (5) Products,
				/// ShoppingCart Average is LESS / MORE than R1500?
				/// LESS: +R150 Handling Fee || MORE: +R200 Handling Fee
				/// ↓----------------------------
				default:
					handlingFee = cartAverage < 1500 ? 150 : 200;
					break;
			}
			/// -------ViewBag---------------
			/// ↓SUMMARY↓: First Iteration
			/// ↓----------------------------
			ViewBag.HandlingFee = handlingFee;
			ViewBag.GrandTotal = handlingFee + ViewBag.CartTotalSale;
			orderSubTotal = ViewBag.CartTotalSale;
			fee = handlingFee;
			orderGrandTotal = ViewBag.GrandTotal;
			/// -------PlaceOrder------------
			/// ↓SUMMARY↓: GET METHOD
			/// ↓----------------------------
			if (startPlaceOrder > 0)
			{
				await PlaceOrder(orderSubTotal, shippingTotal, fee, orderGrandTotal);
				ViewBag.Carry = getUserEmail();
			}
			return View();
		}
		[HttpPost]
		public async Task<ActionResult> CheckoutAccount(CheckoutAccount updatedUser)
		{
			AutoUserService autoUser = new AutoUserService(_app, _signInManager, _userManager, _context);
			emailPersis = await autoUser.ManageUser(getUserEmail(), updatedUser);
			try
			{
				await autoUser.signInUser(emailPersis);
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}

			return RedirectToAction("Index", new RouteValueDictionary(new { updateCookie = emailPersis }));
		}
		/// -------PlaceOrder------------
		/// ↓SUMMARY↓: (HttpPost) First Iteration
		/// ↓----------------------------
		[HttpPost]
		public Task PlaceOrder(decimal orderSubTotal, decimal shippingTotal, decimal fee, decimal orderGrandTotal)
		{
			/// -------Variables-----------
			/// ↓SUMMARY↓: Local Variables
			/// ↓--------------------------
			string email = getUserEmail();
			Queue<int> productId = new Queue<int>();
			var productIdValue = 0;
			/// -------Products------------
			/// ↓SUMMARY↓: GET Products from User ShoppingCart
			/// ↓--------------------------
			foreach (var product in _context.ShopingBasket.Where(s => s.BasketId.Equals(getUserEmail())))
				productId.Enqueue(product.Id);
            /// ---Table[OrderProducts]----
            /// ↓SUMMARY↓: GET Products from User ShoppingCart
            /// ↓--------------------------
            while (productId.Count > 0)
			{
                productIdValue = productId.Dequeue();
				var orderProducts = new OrderProducts
				{
					UserId = email,
					ProductId = productIdValue
                };
                /// ↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-↓-
                _context.OrderProducts.Add(orderProducts);
                _context.SaveChanges();
            }
            /// -------Table[Orders]-------
            /// ↓SUMMARY↓: GET Products from User ShoppingCart
            /// ↓--------------------------
            var order = new Orders
			{
				UserId = getUserEmail(),
				OrderSubTotal = orderSubTotal,
				ShippingTotal = shippingTotal,
				Fee = fee,
				OrderGrandTotal = orderGrandTotal,
			};
            _context.Orders.Add(order);
            _context.SaveChanges();
            return Task.CompletedTask;
        }
		public string getUserEmail()
		{
			var user = _userManager.GetUserAsync(User).Result;
			var email = user.Email;
			return email;
		}
		public async Task<string> getUserToken()
		{
			AutoUserService autoUser = new AutoUserService(_app, _signInManager, _userManager, _context);
			string userEmail;
			if (!User?.Identity.IsAuthenticated == false)
			{
				var user = await _userManager.GetUserAsync(User);
				userEmail = user.Email;
				return userEmail;
			}
			else
			{			
				if (HttpContext.Request.Cookies.ContainsKey("cookieV2"))
				{
					userEmail = HttpContext.Request.Cookies["cookieV2"];
					return userEmail;
				}
				else if (HttpContext.Request.Cookies.ContainsKey("cookieUserV2"))
				{
					userEmail = HttpContext.Request.Cookies["cookieUserV2"];
					return userEmail;
				}
				else
				{
					return await autoUser.ManageUser();
				}
			}
		}
		public IActionResult ReviewOrder()
		{
			return View();
		}
	}
}
