using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
	public class UserWishList
	{
		[Key]
		public int Id { get; set; }
		public string UserId { get; set; }	
		public int? ProductId { get; set; }
	}
}
