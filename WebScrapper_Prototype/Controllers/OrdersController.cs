using System;
using System.Collections.Generic;
using System.Linq;
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

		// GET: Orders
		public async Task<IActionResult> Index()
        {
			AutoUserService userService = new AutoUserService(_app, _signInManager, _userManager, _context);
			if (!User?.Identity.IsAuthenticated == false)
			{
                if (!getUserEmail().Equals("404"))
                {
                    Console.WriteLine("COOKIE: WORKING...");
                }
                else
                {
                    Console.WriteLine("ERROR: UserEmail is NULL!!!" + getUserEmail());
                }
            }
			else
			{
				if (!HttpContext.Request.Cookies.ContainsKey("cookie2"))
				{
					CookieOptions cookieOptions = new CookieOptions();
					cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));
					cookieOptions.IsEssential = true;
					HttpContext.Response.Cookies.Append("cookie2", userService.ManageUser().Result, cookieOptions);

				}
				else
				{
					var firstRequest = HttpContext.Request.Cookies["cookie2"];
					await userService.signInUser(firstRequest.ToString());
				}
                if (!getUserEmail().Equals("404"))
                {
                    Console.WriteLine("COOKIE: WORKING...");
                }
                else
                {
                    Console.WriteLine("ERROR: UserEmail is NULL!!!" + getUserEmail());
                }
            }

            var products = _context.Products
                .Join(_context.ShopingBasket.Where(s => s.BasketId.Equals(getUserEmail())), p => p.ID, b => b.ProductId, (p, b) => p)
                .ToList();

            var order = _context.Orders.Where(o => o.UserId.Equals(getUserEmail()));
            decimal orderSubTotal = 0;
            decimal shippingTotal = 0;
            decimal fee = 0;
            decimal orderGrandTotal = 0;
            foreach (var item in order)
            {
                orderSubTotal = item.OrderSubTotal;
                shippingTotal = item.ShippingTotal;
                fee = item.Fee;
                orderGrandTotal = item.OrderGrandTotal;
            }
            ViewBag.SubTotal = orderSubTotal;
            ViewBag.ShippingTotal = shippingTotal;
            ViewBag.Fee = fee;
            ViewBag.OrderGrandTotal = orderGrandTotal;
            return View(products);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderSubTotal,ShippingTotal,Fee,OrderGrandTotal")] Orders orders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,OrderSubTotal,ShippingTotal,Fee,OrderGrandTotal")] Orders orders)
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'WebScrapper_PrototypeContext.Orders'  is null.");
            }
            var orders = await _context.Orders.FindAsync(id);
            if (orders != null)
            {
                _context.Orders.Remove(orders);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(int id)
        {
          return _context.Orders.Any(e => e.Id == id);
        }
        public string getUserEmail()
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;
                var email = user.Email;
                return email;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "404";
            }
        }
    }
}
