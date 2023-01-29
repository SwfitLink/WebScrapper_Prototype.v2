using CsvHelper.Configuration;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Mappers
{
    public sealed class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Map(x => x.ProductName).Name("ProductName");
            Map(x => x.ProductStock).Name("ProductStock");
            Map(x => x.ProductDescription).Name("ProductDescription");
            Map(x => x.ProductBasePrice).Name("ProductBasePrice");
            Map(x => x.ProductSalePrice).Name("ProductSalePrice");
            Map(x => x.VendorProductURL).Name("ImageURL");
            Map(x => x.ProductCategory).Name("Cat");
            Map(x => x.ImageURL).Name("Image-src");
            Map(x => x.dataBatch).Name("DataBatchNumber");

        }
    }
}
