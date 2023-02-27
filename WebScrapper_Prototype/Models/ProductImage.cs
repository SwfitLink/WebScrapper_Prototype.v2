using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
	public class ProductImage
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public int ProductId { get; set; }
		[Required]
		public string? FileName { get; set; }
		[Required]
		public string? FileType { get; set; }
		[Required]
		public byte[]? Content { get; set; }
	}
}