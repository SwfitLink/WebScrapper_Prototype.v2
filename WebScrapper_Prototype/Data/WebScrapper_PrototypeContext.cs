using Microsoft.EntityFrameworkCore;

namespace WebScrapper_Prototype.Data
{
    public class WebScrapper_PrototypeContext : DbContext
    {
        public WebScrapper_PrototypeContext(DbContextOptions<WebScrapper_PrototypeContext> options)
            : base(options)
        {

        }
        public DbSet<WebScrapper_Prototype.Models.ProductModel> Products { get; set; } = default!;
    }
}
