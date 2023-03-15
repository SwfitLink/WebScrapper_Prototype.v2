using CsvHelper.Configuration;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Mappers
{
    public sealed class ProductMapSingle : ClassMap<Product>
    {
        public ProductMapSingle()
        {
            Map(x => x.VendorSite).Name("web-scraper-start-url");
			Map(x => x.VendorProductURL).Name("ScapperProdcutId-href");
            Map(x => x.ProductKey).Name("ScapperProdcutId");
            Map(x => x.ProductCategory).Name("Cat");
			Map(x => x.ProductName).Name("ProductName");
            Map(x => x.ProductStock).Name("ProductStock");
            Map(x => x.ProductBasePrice).Name("ProductBasePrice");
            Map(x => x.ProductSalePrice).Name("ProductSalePrice");
            Map(x => x.ProductDescription).Name("ProductShortDescription");
            Map(x => x.ProductImageURL).Name("ProductImage-src");

        }
    }
}
