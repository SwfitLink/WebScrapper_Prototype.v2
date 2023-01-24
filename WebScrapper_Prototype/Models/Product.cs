using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace WebScrapper_Prototype.Models
{
    public class Product
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(100)]
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
        [Required]
        public string ImageURL { get; set; }
        [Required]
        [NotMapped]
        public IFormFile ProductPic { get; set; }
    }
}
