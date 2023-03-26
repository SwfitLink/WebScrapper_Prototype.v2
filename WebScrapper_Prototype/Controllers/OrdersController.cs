using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Services;

namespace WebScrapper_Prototype.Controllers
{
    public class OrdersController : Controller
    {
        private readonly WebScrapper_PrototypeContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _app;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private string userEmail = string.Empty;

		public OrdersController(WebScrapper_PrototypeContext context, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, ApplicationDbContext app)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_app = app;			
		}
		/// <summary>
		/// Handles Order Reviewing
		/// </summary>
		public async Task<IActionResult> Index()
        {
			// User Details Cookie
			userEmail = await CheckUserCookie();
			var products = from o in _context.Orders
						 join op in _context.OrderProducts on o.Id equals op.OrderId
						 join p in _context.Products on op.ProductKey equals p.Id
						 select p;
			var orderDetails = _context.Orders.Where(o => o.UserId != null && o.UserId.Equals(userEmail));
            decimal orderSubTotal = 0;
            decimal shippingTotal = 0;
            decimal fee = 0;
            decimal orderGrandTotal = 0;
			int id = 0;
	
            foreach (var detail in orderDetails)
            {
				id = detail.Id;
                orderSubTotal = detail.OrderSubTotal;
                shippingTotal = detail.ShippingTotal;
                fee = detail.Fee;
                orderGrandTotal = detail.OrderGrandTotal;
            }
			
            ViewBag.SubTotal = orderSubTotal;
            ViewBag.ShippingTotal = shippingTotal;
            ViewBag.Fee = fee;
            ViewBag.OrderGrandTotal = orderGrandTotal;
			ViewBag.OrderId = id;
            return View(products);
        }
		/// <summary>
		/// Checks if there is a cookie present for the user, if not, creates one and signs in the user.
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
				return await detailsService.Cookie();
		}
	}
}
