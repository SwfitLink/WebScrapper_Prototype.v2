using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
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
		public async Task<IActionResult> Index(int showOrder)
		{
			// User Details Cookie
			await CheckUserCookie();
			// Check if user account is cookie
			var userEmail = getUserEmail();
			if (userEmail.Contains("cookie"))
				ViewBag.userIsCookie = true;
			else		
				ViewBag.userIsCookie = false;
			// Place Order
			decimal orderSubTotal = 0;
			decimal shippingTotal = 0;
			decimal fee = 0;
			decimal orderGrandTotal = 0;
			var products = from p in _context.Products
						   join b in _context.ShopingBasket
						   on p.Id equals b.ProductId
						   where b.UserId.Equals(getUserEmail())
						   select p;
			var salePriceSum = products.Sum(p => p.ProductSalePrice);
			ViewBag.BasketCount = products.Count();
			ViewBag.Savings = products.Sum(p => p.ProductBasePrice - p.ProductSalePrice);
			ViewBag.CartTotalBase = products.Sum(p => p.ProductBasePrice);
			if (salePriceSum > 1500 && salePriceSum != null)
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
			decimal handlingFee = 0;
			decimal cartAverage = 0;
			int productCount = products.Count();
			if(salePriceSum > 0)
				cartAverage = (decimal)(salePriceSum / productCount);
			/// -------HandlingFee-----------
			/// ↓SUMMARY↓: First Iteration
			/// ↓----------------------------
			if(productCount > 0)
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
			var order = new Orders
			{
				UserId = getUserEmail(),
				OrderSubTotal = orderSubTotal,
				ShippingTotal = shippingTotal,
				Fee = fee,
				OrderGrandTotal = orderGrandTotal,
			};
			await _context.Orders.AddAsync(order);
			await _context.SaveChangesAsync();
			ViewBag.ShowOrder = 0;
			if (showOrder > 0)
			{
				ViewBag.ShowOrder = 1;
			}
			return View();
		}
		[HttpPost]
		public async Task<ActionResult> CheckoutAccount(CheckoutAccount updatedUser)
		{			
			UserDetailsService userDetailsService = new(_httpContextAccessor);
			AutoUserService autoUser = new(_app, _signInManager, _userManager, _context);
			var order = from o in _context.Orders.Where(o => o.UserId != null && o.UserId.Equals(getUserEmail()))
						select o;
			decimal orderSubTotal = 0;
			decimal shippingTotal = 0;
			decimal fee = 0;
			decimal orderGrandTotal = 0;
			int orderId = 0;
			foreach (var detail in order)
			{
				orderSubTotal = detail.OrderSubTotal;
				shippingTotal = detail.ShippingTotal;
				fee = detail.Fee;
				orderGrandTotal = detail.OrderGrandTotal;
				orderId = detail.Id;
			}
			await PlaceOrder(orderSubTotal, shippingTotal, fee, orderGrandTotal, orderId);
			emailPersis = await autoUser.ManageUser(getUserEmail(), updatedUser);
			try
			{
				await userDetailsService.SignIn(emailPersis);
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}
			return RedirectToAction("Index", new RouteValueDictionary(new { showOrder = 1 }));
		}
		/// -------PlaceOrder------------
		/// ↓SUMMARY↓: (HttpPost) First Iteration
		/// ↓----------------------------
		[HttpPost]
		public async Task PlaceOrder(decimal orderSubTotal, decimal shippingTotal, decimal fee, decimal orderGrandTotal, int orderId)
		{
			/// -------Table[Orders]-------
			/// ↓SUMMARY↓: GET Products from User ShoppingCart -> Create OrderProducts -> SaveChanges()
			/// ↓--------------------------		
			var basketItems = from b in _context.ShopingBasket.Where(s => s.UserId.Equals(getUserEmail())).ToList()
							  select b.ProductId;
			foreach (var basketItem in basketItems)
			{
				var orderProducts = new OrderProducts
				{
					OrderId = orderId,
					ProductId = basketItem
				};
				await _context.OrderProducts.AddAsync(orderProducts);
			}
			await _context.SaveChangesAsync();
			await RemoveBasketItems();
			try
			{
				SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
				SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
				MailMessage email = new MailMessage();
				// START
				email.From = new MailAddress("brandenconnected@gmail.com");
				email.To.Add(getUserEmail());
				email.CC.Add("brandenconnected@gmail.com");
				email.Subject = "SwiftLink: Order Placed (" + orderId + ")";
				email.Body = "<html><body><h1>Congratulations!</h1><h2>Your Order has been placed and you will receive a tracking code shortly.</h2><br>" +
					"<h3>Look how much you have Saved!</h3>" +
					"<br>" +
					"<p>How much you would've paid if everything was Full Price: <span style='color:red;'>R" + orderSubTotal.ToString("#,##0.00") + "</span>" +
					"<br>" + 
					"With our incredible discounted prices, driven by AI - You only paid: <span style='color:forestgreen'>R" + orderGrandTotal.ToString("#,##0.00") + "</span></p>" + 
					"</body></html>";
				email.IsBodyHtml = true;
				//END
				SmtpServer.Timeout = 5000;
				SmtpServer.EnableSsl = true;
				SmtpServer.UseDefaultCredentials = false;
				SmtpServer.Credentials = new NetworkCredential("brandenconnected@gmail.com", "mueadqbombixceuk");
				SmtpServer.Send(email);
				Console.WriteLine("Email Successfully Sent");				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());	
			}		
		}
		/// <summary>
		/// Checks if there is a cookie present for the user, if not, creates one and signs in the user.
		/// </summary>
		private async Task CheckUserCookie()
		{
			UserDetailsService detailsService = new(_context, _httpContextAccessor, _app, _userManager, _signInManager);
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
					if (firstRequest != null)
						await detailsService.SignIn(firstRequest);
				}
			}
		}
		/// <summary>
		/// Gets User Email
		/// </summary>
		public async Task RemoveBasketItems()
		{
			var BasketItems = _context.ShopingBasket.Where(s => s.UserId.Equals(getUserEmail()));
			_context.ShopingBasket.AttachRange(BasketItems);
			_context.ShopingBasket.RemoveRange(BasketItems);
			/// Save Changes
			await _context.SaveChangesAsync();
			/// -->END
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
