using Microsoft.VisualBasic;

namespace WebScrapper_Prototype.Models
{
    public class ScrappedProductModel
    {
        public int ID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductType { get; set; }
        public string ProductCategory { get; set; }
        public float ProductPrice { get; set;}
        public float ProductDiscount { get; set;}
        public DateTime ProductCreated { get; set;}

    }
}
