using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
	public class Basket
	{
		[Key]
		public int Id { get; set; }
		public DateTime createdAt { get; set; }
		public string BasketId { get; set; }
		public int? ProductId { get; set; }
	}
}
