using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using WebScrapper_Prototype.Services;
using X.PagedList;

namespace WebScrapper_Prototype.Controllers
{
    public class OrdersController : Controller
    {
        private readonly WebScrapper_PrototypeContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _app;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IHttpContextAccessor _httpContextAccessor;

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
			UserDetailsService userDetailsService = new(_httpContextAccessor);
			// User Details Cookie
			await CheckUserCookie();
			var userEmail = getUserEmail();
			var products = from o in _context.Orders
						 join op in _context.OrderProducts on o.Id equals op.OrderId
						 join p in _context.Products on op.ProductKey equals p.Id
						 select p;
			//var order = from o in _context.Orders
			//			select o.Id;
			//var products = from p in _context.Products
			//			   join op in _context.OrderProducts						   
			//			   on p.Id equals op.ProductKey
			//			   where op.OrderId.Equals(order)
			//			   select p;

			var orderDetails = _context.Orders.Where(o => o.UserId != null && o.UserId.Equals(getUserEmail()));
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
					if (firstRequest != null)
						await detailsService.SignIn(firstRequest);
				}
			}
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
