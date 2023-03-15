using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
	public class ProductImageURLs
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string? VendorSiteOrigin { get; set; }
		[Required]
		public string? VendorSiteProduct { get; set; }
		[Required]
		public string? ProductKey { get; set; }
		[Required]
		public string? ImageURL { get; set; }
	}
}
