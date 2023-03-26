using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
	public class LoginViewModel
	{
		[EmailAddress]
		public string? Email { get; set; }
		public string? Password { get; set; }
	}
}
