using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Services
{
	public class UserDetailsService
	{
		private readonly WebScrapper_PrototypeContext? _context;
		private readonly UserManager<ApplicationUser>? _userManager;
		private readonly ApplicationDbContext? _app;
		private readonly SignInManager<ApplicationUser>? _signInManager;
		private readonly IHttpContextAccessor? _httpContextAccessor;
		private string emailPersis = string.Empty;

		public UserDetailsService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}
		public UserDetailsService(WebScrapper_PrototypeContext context, IHttpContextAccessor httpContextAccessor, ApplicationDbContext app,UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_app = app;

		}
		public async Task<string> ManageUser()
		{
			var user = CreateUser();
			string generatedEmail = "cookie" + Guid.NewGuid().ToString() + "@swiftlink.com";
			user.FirstName = "cookieUser_FirstName";
			user.LastName = "cookieUser_LastName";
			user.CountryCode = "cookieUser_CountryCode";
			user.LocationProvince = "cookieUser_LocationProvince";
			user.LocationCity = "cookieUser_LocationCity";
			user.AddressLine1 = "cookieUser_AddressLine1";
			user.AddressLine2 = "cookieUser_AddressLine2";
			user.PostalCode = "cookieUser_PostalCode";

			IdentityUser identity = user;
			identity.UserName = generatedEmail;
			identity.Email = generatedEmail;
			identity.NormalizedEmail = generatedEmail;
			identity.NormalizedUserName = generatedEmail;
			identity.PasswordHash = "AQAAAAEAACcQAAAAEH+MdVIf9JyT0rq9Q/+YY8LEzJtSkL1kCdWQPPQTo06tmiiuDbpWMfaDthgRlIMfXg==";
			identity.PhoneNumber = "1234567890";
			identity.PhoneNumberConfirmed = false;
			identity.EmailConfirmed = false;

			_app.Attach(user);
			_app.Entry(user).State = EntityState.Added;
			_app.SaveChanges();

			user = await _signInManager.UserManager.FindByEmailAsync(user.Email);
			emailPersis = user.Email;
			var result = await _signInManager.CheckPasswordSignInAsync(user, "Markaway86!", false);

			if (result.Succeeded)
			{
				var claims = new List<Claim>
					{
						new Claim("arr", "pwd"),
					};
				var roles = await _signInManager.UserManager.GetRolesAsync(user);
				if (roles.Any())
				{
					//Gives Cookie [USER] Role
					var roleClaim = string.Join(",", roles);
					claims.Add(new Claim("Roles", roleClaim));
				}
				else
				{
					await _signInManager.UserManager.AddToRoleAsync(user, "User");
					var roleClaim = string.Join(",", roles);
					claims.Add(new Claim("Roles", roleClaim));
				}
				await _signInManager.SignInWithClaimsAsync(user, true, claims);
			}
			return emailPersis;
		}
		public async Task<string> ManageUser(string token, CheckoutAccount updatedUser)
		{
			if (token.Contains("cookie"))
			{
				var user = CreateUser();/// Start-->
				user.FirstName = updatedUser.FirstName;
				user.LastName = updatedUser.LastName;
				user.CountryCode = updatedUser.CountryCode;
				user.LocationProvince = updatedUser.LocationProvince;
				user.LocationCity = updatedUser.LocationCity;
				user.AddressLine1 = updatedUser.AddressLine1;
				user.AddressLine2 = updatedUser.AddressLine2;
				user.PostalCode = updatedUser.PostalCode;

				IdentityUser identity = user;
				identity.UserName = updatedUser.ContactEmail;
				identity.Email = updatedUser.ContactEmail;
				identity.NormalizedEmail = updatedUser.ContactEmail;
				identity.NormalizedUserName = updatedUser.ContactEmail;
				identity.PasswordHash = "AQAAAAEAACcQAAAAEH+MdVIf9JyT0rq9Q/+YY8LEzJtSkL1kCdWQPPQTo06tmiiuDbpWMfaDthgRlIMfXg==";
				identity.PhoneNumber = updatedUser.PhoneNumber;
				identity.PhoneNumberConfirmed = false;
				identity.EmailConfirmed = false;
				var existingUser = _app.Users.FirstOrDefault(u => u.Email == updatedUser.ContactEmail);
				if (existingUser != null)
					emailPersis = existingUser.Email;
				else
				{
					/// Create: user
					_app.Attach(user);/// Start-->
					_app.Entry(user).State = EntityState.Added;
					/// Save Changes
					_app.SaveChanges();
					/// -->END
					emailPersis = user.Email;
				}
				/// Update: ShoppingCart
				/// UserId: cookieEmail -> UserId: contactEmail			 
				var basketProducts = _context.ShopingBasket.Where(s => s.UserId.Equals(token));/// Start-->
				if (basketProducts != null)
				{
					foreach (var product in basketProducts)
						product.UserId = emailPersis;
					/// Save Changes
					_context.SaveChanges();

				}/// -->END

				 /// Update: UserWishList
				 /// UserId: cookieEmail -> UserId: contactEmail			 
				var wishListProducts = _context.UserWishList.Where(s => s.UserId.Equals(token));/// Start-->
				if (wishListProducts != null)
				{
					foreach (var product in wishListProducts)
						product.UserId = emailPersis;
					/// Save Changes
					_context.SaveChanges();
					/// END
				}/// -->END

				 /// Remove: oldUser
				var oldUser = await _userManager.FindByEmailAsync(token);/// Start-->
				if (oldUser != null)
				{
					_app.Attach(oldUser);
					_app.Remove(oldUser);
					/// Save Changes
					_app.SaveChanges();
					/// -->END
				}
				/// SignIn -> NewUser
				user = await _signInManager.UserManager.FindByEmailAsync(emailPersis);/// Start-->
				emailPersis = user.Email;
				var result = await _signInManager.CheckPasswordSignInAsync(user, "Markaway86!", false);
				if (result.Succeeded)
				{
					var claims = new List<Claim>
					{
						new Claim("arr", "pwd"),
					};
					var roles = await _signInManager.UserManager.GetRolesAsync(user);
					if (roles.Any())
					{
						//Gives Cookie [USER] Role
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					else
					{
						await _signInManager.UserManager.AddToRoleAsync(user, "User");
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					await _signInManager.SignInWithClaimsAsync(user, true, claims);
				}
				return emailPersis;/// --> Stop
			}
			else/// Continue -->
			{
				return null;/// --> END 
			}
		}
		private ApplicationUser CreateUser()
		{
			try
			{
				return Activator.CreateInstance<ApplicationUser>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
					$"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
					$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
			}
		}
		public async Task<CheckoutAccount> CheckoutAccount(string token)
		{
			if(_userManager != null)
			{
				var userA = await _userManager.FindByEmailAsync(token);
				var user = new CheckoutAccount
				{
					IsSubscribed = true,
					FirstName = userA.FirstName,
					LastName = userA.LastName,
					ContactEmail = userA.Email,
					PhoneNumber = userA.PhoneNumber,
					CountryCode = userA.CountryCode,
					LocationProvince = userA.LocationProvince,
					LocationCity = userA.LocationCity,
					AddressLine1 = userA.AddressLine1,
					AddressLine2 = userA.AddressLine2,
					PostalCode = userA.PostalCode
				};
				return user;
			}
			return null;
		}
		public async Task<string> SignIn(string token)
		{
			/// Handle Null Exceptions
			if (_signInManager == null || _userManager == null || _signInManager.UserManager == null)
			{				
				return "--------------------------------------------------ERROR: httpContextAccessor is NULL!!";
			}
			/// Handle Null Exceptions
			var userA = await _signInManager.UserManager.FindByEmailAsync(token);
			if (userA != null)
			{
				var result = await _signInManager.CheckPasswordSignInAsync(userA, "Markaway86!", false);
				if (result.Succeeded)
				{
					var claims = new List<Claim>
						{
							new Claim("arr", "pwd"),
						};
					var roles = await _signInManager.UserManager.GetRolesAsync(userA);
					if (roles.Any())
					{
						//Gives Cookie [USER] Role
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					else
					{
						await _signInManager.UserManager.AddToRoleAsync(userA, "User");
						await _userManager.UpdateAsync(userA);
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					await _signInManager.SignInWithClaimsAsync(userA, true, claims);
				}
			}
			else
			{
				await ManageUser();
			}
			return "Successfully SignedIn!";
		}		
		public string? GetSignedInUserEmail()
		{
			/// Handle Null Exceptions
			if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
			{
				// return a default value or throw an exception, depending on your use case
				return "--------------------------------------------------ERROR: httpContextAccessor is NULL!!";
			}
			/// Handle Null Exceptions
			var emailClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
			return emailClaim?.Value;
		}
	}
}
