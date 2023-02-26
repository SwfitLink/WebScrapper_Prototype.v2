using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Models
{
	public class CheckoutAccount
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public Boolean IsSubscribed { get; set; }
		[Required]
		[StringLength(255, ErrorMessage = "First Name is to long")]
		public string? FirstName { get; set; }
		[Required]
		[StringLength(255, ErrorMessage = "Last Name is to long")]
		public string? LastName { get; set; }
		[Required]
		[EmailAddress]
		public string? ContactEmail { get; set; }
		[Required]
		[Phone]
		public string? PhoneNumber { get; set; }
		[Required]
		public string? CountryCode { get; set; }
		[Required]
		public string? LocationProvince { get; set; }
		[Required]
		public string? LocationCity { get; set; }
		[Required]
		[StringLength(255, ErrorMessage = "Address Line 1 is to long")]
		public string? AddressLine1 { get; set; }
		[Required]
		[StringLength(255, ErrorMessage = "Address Line 2 is to long")]
		public string? AddressLine2 { get; set; }
		[Required]
		public string? PostalCode { get; set; }
	}
}
