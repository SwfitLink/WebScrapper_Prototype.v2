using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
    public class ProductView : EditImage
    {
        public string ProductName { get; set; }
        [Required]
        [StringLength(255)]
        public string ProductDescription { get; set; }
        [Required]
        [StringLength(30)]
        public string ProductStatus { get; set; }
        [Required]
        [StringLength(100)]
        public string ProductCategory { get; set; }
        [Required]
        public float ActualPrice { get; set; }
        [Required]
        public float ScrappedPrice { get; set; }

    }
}
