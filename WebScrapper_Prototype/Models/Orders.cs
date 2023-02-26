using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
	public class Orders
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string? UserId { get; set; }

		[Required]
		public decimal OrderSubTotal { get; set; }

		[Required]
		public decimal ShippingTotal { get; set; }

		[Required]
		public decimal Fee { get; set; }

		[Required]
		public decimal OrderGrandTotal { get; set; }
		
	}
}
