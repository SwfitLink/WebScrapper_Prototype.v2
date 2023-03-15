using CsvHelper.Configuration;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Mappers
{
    public sealed class ProductMapImages : ClassMap<ProductImageURLs>
    {
        public ProductMapImages()
        {
            Map(x => x.VendorSiteOrigin).Name("web-scraper-start-url");
			Map(x => x.ProductKey).Name("ScrapperProductId");
			Map(x => x.VendorSiteProduct).Name("ScrapperProductId-href");           
			Map(x => x.ImageURL).Name("Image-src");
        }
    }
}
