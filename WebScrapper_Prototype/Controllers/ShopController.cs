using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using System.Diagnostics;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebScrapper_Prototype.Controllers
{

	public class ShopController : Controller
	{
		private readonly ILogger<ShopController> _logger;
		private readonly WebScrapper_PrototypeContext _context;

		public ShopController(ILogger<ShopController> logger, WebScrapper_PrototypeContext context)
		{
			_logger = logger;
			_context = context;

		}
		public IActionResult Index(int? page)
		{
			var products = from p in _context.Products
			select p;
			products = products.Where(s => s.Visible.Contains("Visible"));
			int pageSize = 25;
			int pageNumber = (page ?? 1);
			var onePageOfProducts = products.ToPagedList(pageNumber, pageSize);
			ViewBag.OnePageOfProducts = onePageOfProducts;
			return View(onePageOfProducts);
		}
	}
}
