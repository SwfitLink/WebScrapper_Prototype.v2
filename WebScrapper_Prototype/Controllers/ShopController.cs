using Microsoft.AspNetCore.Mvc;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using System.Diagnostics;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WebScrapper_Prototype.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace WebScrapper_Prototype.Controllers
{

	public class ShopController : Controller
	{
		private readonly ILogger<ShopController> _logger;
		private readonly WebScrapper_PrototypeContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public ShopController(ILogger<ShopController> logger, WebScrapper_PrototypeContext context, UserManager<ApplicationUser> userManager)
		{
			_logger = logger;
			_context = context;
			_userManager = userManager;

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
		public IActionResult Index(int basketId, int productId, string brand, string searchString, string currentFilter, int? page)
		{

			ViewBag.CurrentFilter = currentFilter;		
			var products = from p in _context.Products
						   select p;
			var basket = from b in _context.ShopingBasket
						 select b;
			products = products.Where(p => p.Visible.Equals("Visible"));
			var a = AddToCart(productId);
			basket = basket.Where(b => b.BasketId.Equals(a));
			if (searchString != null)
			{
				page = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			ViewBag.CurrentFilter = searchString;
			if (!String.IsNullOrEmpty(searchString))
			{
				var words = searchString.Split(' ');
				foreach(var word in words)
				{
					products = products.Where(s => s.ProductName.Contains(word));

				}
			}
			if (basketId == 1)
			{
				var query = from p in _context.Products
							join b in _context.ShopingBasket
							on p.ID equals b.ProductId
							select new { p, b };
				products = query.Select(q => q.p);
				ViewBag.BasketLoad = 1;
				return View(products.ToPagedList(1, 10));
			}
			else
			{
				ViewBag.BasketLoad = 0;
			}

			int pageSize = 25;
			int pageNumber = (page ?? 1);
			var onePageOfProducts = products.ToPagedList(pageNumber, pageSize);
			ViewBag.OnePageOfProducts = onePageOfProducts;
			return View(onePageOfProducts);
		}
		public async Task<string> getUser()
		{
			var user = await _userManager.GetUserAsync(User);
			var email = user.Email;
			return email;
		} 
		[Authorize(Roles = "User, Manager")]
		[HttpPost]
		public string AddToCart(int productId)
		{
			var prod = from p in _context.Products
					   select p;
			if (productId > 0)
			{
				prod = prod.Where(p => p.ID == productId);
				ShoppingBasket shoppingBasket = new ShoppingBasket();
				shoppingBasket.ProductId = productId;
				shoppingBasket.BasketId = getUser().Result;
				_context.Attach(shoppingBasket);
				_context.Entry(shoppingBasket).State = EntityState.Added;
				_context.SaveChanges();

				return shoppingBasket.BasketId;
			}
			return null;
		}
		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int BasketRemovePID)
		{
			var basket = from b in _context.ShopingBasket
						 select b;
			var basketRowsToDelete = basket.Where(b => b.ProductId == BasketRemovePID);

			if (basketRowsToDelete != null)
			{
				_context.ShopingBasket.RemoveRange(basketRowsToDelete);
			}
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", new { BasketID = 1 });
		}
	}
}
