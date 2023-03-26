using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebScrapper_Prototype.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
	[Required]
	public string? FirstName { get; set; }
	[Required]
	public string? LastName { get; set; }
	[Required]
	public DateTime Joined { get; set; }


}

