using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using System.Diagnostics;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using WebScrapper_Prototype.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Cors;

namespace WebScrapper_Prototype.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly WebScrapper_PrototypeContext _context;
		private readonly ApplicationDbContext _app;
		private readonly UserManager<ApplicationUser> _userManager;
		public HomeController(ILogger<HomeController> logger, WebScrapper_PrototypeContext context, UserManager<ApplicationUser> userManager, ApplicationDbContext app)
		{
			_logger = logger;
			_context = context;
			_app = app;
			_userManager = userManager;
		}
		[HttpGet]
		public IActionResult Index()
		{		
			return Redirect("/Shop");
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult ReleaseNotes()
		{
			return View();
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		public ActionResult UnderConstruction()
		{
			return View();
		}
	}
}