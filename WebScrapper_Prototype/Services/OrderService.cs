using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Services
{
	public class OrderService
	{
		private readonly WebScrapper_PrototypeContext _context;

		public OrderService(WebScrapper_PrototypeContext context)
		{
			_context = context;
		}
		public async Task PlaceOrder(Orders order)
		{
			_context.Attach(order);
			_context.Entry(order).State = EntityState.Added;
			_context.SaveChanges();
		}
	}
}
