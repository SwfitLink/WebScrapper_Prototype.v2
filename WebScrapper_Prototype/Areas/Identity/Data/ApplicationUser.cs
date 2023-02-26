using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
	[Required]
	public Boolean IsSubscribed { get; set; }
	[Required]
	[StringLength(255, ErrorMessage = "First Name is to long")]
	public string? FirstName { get; set; }
	[Required]
	[StringLength(255, ErrorMessage = "Last Name is to long")]
	public string? LastName { get; set; }
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

