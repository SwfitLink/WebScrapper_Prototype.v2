using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
	public class Basket
	{
		[Key]
		public int Id { get; set; }
		public string BasketId { get; set; }
		public int? ProductId { get; set; }
	}
}
