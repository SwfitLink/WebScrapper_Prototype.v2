using Microsoft.EntityFrameworkCore;
using WebScrapper_Prototype.Models;


namespace WebScrapper_Prototype.Data
{
	public class WebScrapper_PrototypeContext : DbContext
	{

		public WebScrapper_PrototypeContext(DbContextOptions<WebScrapper_PrototypeContext> options)
			: base(options)
		{

		}
		public DbSet<Product> Products { get; set; }
		public DbSet<UserWishList> UserWishList { get; set; }
		public DbSet<Basket> ShopingBasket { get; set; }
	}
}
