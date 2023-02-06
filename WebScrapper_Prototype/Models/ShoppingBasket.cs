using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
	public class ShoppingBasket
	{
		[Key]
		public int Id { get; set; }
		public string BasketId { get; set; }
		public int? ProductId { get; set; }
	}
}
