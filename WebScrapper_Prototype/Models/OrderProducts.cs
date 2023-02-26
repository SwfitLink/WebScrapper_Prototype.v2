using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
    public class OrderProducts
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? UserId { get; set; }
        [Required]
        public int ProductId { get; set; }
    }
}
