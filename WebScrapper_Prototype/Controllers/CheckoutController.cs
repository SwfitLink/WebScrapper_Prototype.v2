using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using WebScrapper_Prototype.Services;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Microsoft.EntityFrameworkCore;

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
		private string userEmail = string.Empty;
		private string userFirstName = string.Empty;

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
		public async Task<IActionResult> Index(int startPlaceOrder)
		{
			/// Cookies ///
			userEmail = await CheckUserCookie();
			if (userEmail.Contains("cookie"))
			{
				ViewBag.IsCookie = true;
				ViewBag.FirstName = "Please Login or Register";
			}
			else
			{
				ViewBag.IsCookie = false;
				userFirstName = _context.UserModels.FirstOrDefault(s => s.Email.Equals(userEmail)).FirstName;
				ViewBag.FirstName = userFirstName;
			}
			var view = new UserViewModel
			{
				FirstName = string.Empty,
				LastName = string.Empty,
				Email = string.Empty,
				PhoneNumber = string.Empty,
				Province = string.Empty,
				City = string.Empty,
				PostalCode = string.Empty,
				Unit = string.Empty,
				Street = string.Empty,
				Area = string.Empty
			};
			var user = _context!.UserModels.FirstOrDefault(u => u.Email == userEmail);
			// Place Order
			decimal orderSubTotal = 0;
			decimal shippingTotal = 0;
			decimal fee = 0;
			decimal orderGrandTotal = 0;
			var products = from p in _context.Products
						   join b in _context.ShopingBasket
						   on p.Id equals b.ProductKey
						   where b.UserId.Equals(userEmail)
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
			if (salePriceSum > 0)
				cartAverage = (decimal)(salePriceSum / productCount);
			/// -------HandlingFee-----------
			/// ↓SUMMARY↓: First Iteration
			/// ↓----------------------------
			if (productCount > 0)
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
			ViewBag.Items = products;
			if (userEmail.Contains("cookie"))
				ViewBag.userIsCookie = true;
			else
			{
				ViewBag.userIsCookie = false;
				var userShipping = _context.UserShippings.FirstOrDefault(u => u.UserId == userEmail);
				if (userShipping != null)
				{
					view = new UserViewModel
					{
						FirstName = user.FirstName,
						LastName = user.LastName,
						Email = user.Email,
						PhoneNumber = user.PhoneNumber,
						Province = userShipping.Province,
						City = userShipping.City,
						PostalCode = userShipping.PostalCode,
						Unit = userShipping.Unit,
						Street = userShipping.Street,
						Area = userShipping.Area
					};
				}
				else
				{
					view = new UserViewModel
					{
						FirstName = user.FirstName,
						LastName = user.LastName,
						Email = user.Email,
						PhoneNumber = user.PhoneNumber
					};
				}
			}
			if (startPlaceOrder > 0)
				await Proceed(orderSubTotal, shippingTotal, fee, orderGrandTotal, userEmail, user.FirstName, user.LastName);
			return View(view);
		}
		[HttpPost]
		public async Task<ActionResult> EntireUser(UserViewModel model)
		{
			UserDetailsService service = new(_context, _httpContextAccessor, _app, _userManager, _signInManager);
			string token = await CheckUserCookie();
			var result = await service.EntireUser(model, token);
			Console.WriteLine(result);
			switch (result)
			{
				case 200:
					return RedirectToAction(nameof(Index));
				case 250:
					return RedirectToAction(nameof(Index));
				case 500:
					return RedirectToAction("Index", "User");
			}
			return RedirectToAction(nameof(Index));
		}
		[HttpPost]
		public async Task<ActionResult> ShippingUser(UserShipping model)
		{
			UserDetailsService service = new(_context, _httpContextAccessor, _app, _userManager, _signInManager);
			string token = await CheckUserCookie();
			await service.ShippingUser(model, token);
			return RedirectToAction(nameof(Index));
		}
		public ActionResult Edit()
		{
			return RedirectToAction("Index", "User");
		}
		public async Task Proceed(decimal orderSubTotal, decimal shippingTotal, decimal fee, decimal orderGrandTotal, string userEmail, string firstName, string lastName)
		{
			Console.WriteLine("------------------------------------------------------------------------------------->>\n" +
					"Placing New Order ------------------------------------------>>\n" +
					"Email: " + userEmail);
			int orderId = await PlaceOrder(orderSubTotal, shippingTotal, fee, orderGrandTotal);
			Console.WriteLine("Order Placed" + "<<-------------------------------------------------------------------------------------\n");
			Console.WriteLine("------------------------------------------------------------------------------------->>\n" +
				"Sending New User an Email ------------------------------------------>>\n" +
				"Email: " + userEmail);
			await SendEmail(userEmail, firstName + " " + lastName, orderSubTotal, shippingTotal, fee, orderGrandTotal, orderId);
			Console.WriteLine("Email Sent" + "<<-------------------------------------------------------------------------------------\n");
		}
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
				return await detailsService.Cookie();
		}
		[HttpPost]
		public async Task<int> PlaceOrder(decimal orderSubTotal, decimal shippingTotal, decimal fee, decimal orderGrandTotal)
		{
			var basketItems = _context.ShopingBasket
				.Where(s => s.UserId.Equals(userEmail));
			var order = new Orders
			{
				UserId = userEmail,
				OrderSubTotal = orderSubTotal,
				ShippingTotal = shippingTotal,
				Fee = fee,
				OrderGrandTotal = orderGrandTotal,
			};
			_context.Orders.Attach(order);
			_context.Entry(order).State = EntityState.Added;
			foreach (var basketItem in basketItems)
			{
				var orderProducts = new OrderProducts
				{
					OrderId = order.Id,
					ProductKey = basketItem.ProductKey
				};
				_context.OrderProducts.Attach(orderProducts);
				_context.Entry(orderProducts).State = EntityState.Added;
			}
			_context.ShopingBasket.AttachRange(basketItems);			
			_context.ShopingBasket.RemoveRange(basketItems);
			await _context.SaveChangesAsync();
			return order.Id;
		}
		[HttpPost]
		public async Task SendEmail(string userEmail, string fullName, decimal orderSubTotal, decimal shippingTotal, decimal fee, decimal orderGrandTotal, int orderId)
		{
			try
			{
				SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
				SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
				MailMessage email = new MailMessage();
				// START
				email.From = new MailAddress("brandenconnected@gmail.com");
				email.To.Add(userEmail);
				email.CC.Add("brandenconnected@gmail.com");
				email.Subject = "SwiftLink Order Confirmation";
				email.Body = "" +
					"<!DOCTYPE html>" +
					"<html> " +
					"<head> " +
					"<meta charset=\"utf-8\"/> " +
					"<style>/* Styling for the header */ .header{display: flex; justify-content: space-between; align-items: center; padding: 20px;}.header img{height: 50px;}.header h1{margin: 0; font-size: 24px; color: #333;}/* Styling for the message */ .message{background-color: #f5f5f5; padding: 20px;}.message h2{margin-top: 0; font-size: 20px; color: #333;}.message p{margin: 0; font-size: 16px; line-height: 1.5; color: #666;}/* Styling for the banner */ .banner{text-align: center; padding: 20px;}.banner img{max-width: 100%;}/* Styling for the signature */ .signature{text-align: center; padding: 20px;}.signature p{margin: 0; font-size: 16px; line-height: 1.5; color: #666;}</style> " +
					"</head> " +
					"<body> " +
					"<div class=\"header\"> " +
					"<img src=\"/Media/LogoCropped.png\" alt=\"Logo\"/> " +
					"<h1>SwiftLink Order Confirmation</h1> </div><div class=\"message\"> " +
					"<h2>Thank You for Your Order!</h2> " +
					"<p> Dear <strong>" + fullName + "</strong>, " +
					"<br/> Thank you for placing your order with SwiftLink. We are now processing your order and it can take between 1 - 2 working days. " +
					"You will receive a shipping tracking number once your order is processed and on its way." +
					"</p><p>You can review your order by clicking on the following link: <a href='https://longdogechallenge.com/'>Order Details</a></p>" +
					"</div>" +
					"<div class=\"banner\"> " +
					"<img src=\"https://example.com/banner.png\" alt=\"Banner\"/> " +
					"</div>" +
					"<div class=\"signature\"> " +
					"<p>Kind regards,</p><p>The SwiftLink Team</p>" +
					"</div>" +
					"</body></html>";
				email.IsBodyHtml = true;
				//END
				SmtpServer.Timeout = 5000;
				SmtpServer.EnableSsl = true;
				SmtpServer.UseDefaultCredentials = false;
				SmtpServer.Credentials = new NetworkCredential("brandenconnected@gmail.com", "mueadqbombixceuk");
				await SmtpServer.SendMailAsync(email);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

		}
	}
}
