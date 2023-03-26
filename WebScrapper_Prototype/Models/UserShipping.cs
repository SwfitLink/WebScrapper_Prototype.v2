using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
	public class UserShipping
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string? UserId { get; set;}
		[Required]
		public string? Province { get; set; }
		[Required]
		public string? City { get; set; }
		[Required]
		public string? PostalCode { get; set; }
		[Required]
		public string? Unit { get; set; }
		[Required]
		public string? Street { get; set; }
		[Required]
		public string? Area { get; set; }
	}
}
