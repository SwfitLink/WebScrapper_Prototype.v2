using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using WebScrapper_Prototype.Services;

namespace WebScrapper_Prototype.Controllers
{
	public class UserController : Controller
	{
		private readonly WebScrapper_PrototypeContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _app;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private string userEmail = string.Empty;
		public UserController(WebScrapper_PrototypeContext context, IHttpContextAccessor httpContextAccessor, ApplicationDbContext app, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_app = app;
		}
		[HttpGet]
		public async Task<IActionResult> Index(string email, string invalidResult)
		{
			userEmail = await CheckUserCookie();
			if (!userEmail!.Contains("cookie") && email == null)
			{
				ViewBag.UserLayer = 0;
				var userModel = _context.UserModels.FirstOrDefault(u => u.Email!.Equals(userEmail));
				var userShipping = _context.UserShippings.FirstOrDefault(u => u.UserId!.Equals(userEmail));

				if (userShipping != null)
				{
					var view = new UserViewModel
					{
						FirstName = userModel!.FirstName,
						LastName = userModel.LastName,
						Email = userModel.Email,
						PhoneNumber = userModel.PhoneNumber,
						Province = userShipping.Province,
						City = userShipping.City,
						PostalCode = userShipping.PostalCode,
						Unit = userShipping.Unit,
						Street = userShipping.Street,
						Area = userShipping.Area
					};
					return View(view);
				}
				else
				{
					var view = new UserViewModel
					{
						FirstName = userModel!.FirstName,
						LastName = userModel.LastName,
						Email = userModel.Email,
						PhoneNumber = userModel.PhoneNumber,
						Province = "No Information",
						City = "No Information",
						PostalCode = "No Information",
						Unit = "No Information",
						Street = "No Information",
						Area = "No Information"

					};
					return View(view);
				}
			}
			else if (userEmail!.Contains("cookie") && email == null)
			{
				ViewBag.UserLayer = 1;
				return View();
			}				
			else if(email != null)
			{
				ViewBag.UserLayer = 2;
				ViewBag.Result = email;
				return View();
			}
			else
				return View();
		}
		public async Task<ActionResult> Register(UserModel model)
		{
			UserDetailsService service = new(_context, _httpContextAccessor, _app, _userManager, _signInManager);
			string result = string.Empty;
			const string cookieName = "wazawarecookie6";
			var requestCookies = HttpContext.Request.Cookies;
			var cookieOptions = new CookieOptions
			{
				Expires = DateTimeOffset.Now.AddDays(7),
				IsEssential = true
			};
			var firstRequest = requestCookies[cookieName];
			if (ModelState.IsValid)
			{
				result = await service.Register(model, userEmail);
				if (result.Equals("100"))
					return RedirectToAction("Index", new { email = model.Email });

				HttpContext.Response.Cookies.Append(cookieName, result, cookieOptions);
			}
			else
				Console.WriteLine("ERROR!");
			return RedirectToAction(nameof(Index));
		}
		[HttpPost]
		public async Task<ActionResult> UserUpdateContact(UserModel model)
		{
			userEmail = await CheckUserCookie();
			var dbModel = _context.UserModels.FirstOrDefault(s => s.Email!.Equals(userEmail));
			if (dbModel != null)
			{
				// check if the model contains changes
				if (model.FirstName != dbModel.FirstName || model.LastName != dbModel.LastName
					|| model.PhoneNumber != dbModel.PhoneNumber)
				{
					// update only the changed columns
					dbModel.FirstName = model.FirstName ?? dbModel.FirstName;
					dbModel.LastName = model.LastName ?? dbModel.LastName;
					dbModel.PhoneNumber = model.PhoneNumber ?? dbModel.PhoneNumber;

					// save changes to database
					_context.Update(dbModel);
					await _context.SaveChangesAsync();
				}
			}
			return RedirectToAction(nameof(Index));
		}
		[HttpPost]
		public async Task<ActionResult> UserUpdateShipping(UserShipping model)
		{
			userEmail = await CheckUserCookie();
			var dbModel = _context.UserShippings.FirstOrDefault(s => s.UserId!.Equals(userEmail));
			if (dbModel != null)
			{
				// check if the model contains changes
				if (model.Province != dbModel.Province || model.City != dbModel.City
					|| model.PostalCode != dbModel.PostalCode || model.Unit != dbModel.Unit
					|| model.Street != dbModel.Street || model.Area != dbModel.Area)
				{
					// update only the changed columns
					dbModel.Province = model.Province ?? dbModel.Province;
					dbModel.City = model.City ?? dbModel.City;
					dbModel.PostalCode = model.PostalCode ?? dbModel.PostalCode;
					dbModel.Unit = model.Unit ?? dbModel.Unit;
					dbModel.Street = model.Street ?? dbModel.Street;
					dbModel.Area = model.Area ?? dbModel.Area;

					// save changes to database
					_context.Update(dbModel);
					await _context.SaveChangesAsync();
				}
			}
			return RedirectToAction(nameof(Index));
		}		
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			UserDetailsService service = new(_context, _httpContextAccessor, _app, _userManager, _signInManager);
			int result = 0;
			if (!String.IsNullOrEmpty(model.Password))
				result = await service.Login(model);
			switch (result)
			{					
				case 200:
					const string cookieName = "wazawarecookie6";
					var requestCookies = HttpContext.Request.Cookies;
					var firstRequest = requestCookies[cookieName];
					var cookieOptions = new CookieOptions
					{
						Expires = DateTimeOffset.Now.AddDays(7),
						IsEssential = true
					};
					HttpContext.Response.Cookies.Append(cookieName, model.Email, cookieOptions);
					return RedirectToAction(nameof(Index));
				case 300:
					return RedirectToAction(nameof(Index), new { invalidResult = "Invalid Password" });					
				default:
					return RedirectToAction(nameof(Index), new { invalidResult = "Password is to weak" });
			}			
		}
		[HttpGet]
		public async Task Logout()
		{
			await _signInManager.SignOutAsync();
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
	}
}
