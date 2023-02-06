using Microsoft.EntityFrameworkCore;
using System.Web.Helpers;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Data
{
	public class WebScrapper_PrototypeContext : DbContext
	{
		public WebScrapper_PrototypeContext(DbContextOptions<WebScrapper_PrototypeContext> options)
			: base(options)
		{

		}
		public DbSet<ProductModel> Products { get; set; }
		public DbSet<ShoppingBasket> ShopingBasket { get; set; }
	}
}
