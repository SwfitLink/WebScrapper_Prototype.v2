using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
    public class OrderProducts
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int ProductKey { get; set; }
    }
}
