using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? ProductName { get; set; }
        [Required]
        public string? ProductStock { get; set; }
        [Required]
        public string? ProductDescription { get; set; }
        [Required]
        public string? ProductStatus { get; set; }
        [Required]
        public string? ProductCategory { get; set; }
        [Required]
        public decimal? ProductBasePrice { get; set; }
        [Required]
        public decimal? ProductSalePrice { get; set; }
        [Required]
        public string? ImageURL { get; set; }
        [Required]
        public string? VendorSite { get; set; }
        [Required]
        public string? VendorProductURL { get; set; }
        [Required]
        public string? Visible { get; set; }
        [Required]
        public string? dataBatch { get; set; }
        [NotMapped]
        public IFormFile ProductPic { get; set; }
    }
}
