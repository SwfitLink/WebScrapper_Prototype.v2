using Microsoft.AspNetCore.Identity;
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
		private readonly IUserStore<ApplicationUser> _userStore;
		private readonly IUserEmailStore<ApplicationUser> _emailStore;
		private string emailPersis = string.Empty;

		public UserDetailsService(WebScrapper_PrototypeContext context, IHttpContextAccessor httpContextAccessor, ApplicationDbContext app, UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = usermanager;
			_signInManager = signInManager;
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_app = app;
		}
		public async Task<int> Login(LoginViewModel model)
		{
			var user = await _signInManager!.UserManager.FindByEmailAsync(model.Email);
			if (user != null)
			{
				var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
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
					return 200;
				}
				else
					return 500;
			}
			else
				return 404;
		}
		public async Task<string> Register(UserModel model, string token)
		{
			var user = CreateUser();
			user.FirstName = model.FirstName;
			user.LastName = model.LastName;
			user.Email = model.Email;
			user.PhoneNumber = model.PhoneNumber;
			model.Joined = DateTime.Now;
			user.Joined = DateTime.Now;
			user.UserName = model.Email;
			IdentityUser identity = user;

			var existingUser = _app!.Users.FirstOrDefault(u => u.Email == model.Email);
			if (existingUser != null && !existingUser.Email.Contains("cookie"))
				return "100";
			else
			{
				var resultCreate = await _userManager!.CreateAsync(user, model.Password);
				Console.WriteLine(resultCreate.Errors);
				if (resultCreate.Succeeded)
				{
					Console.WriteLine("USER CREATED!!");
					_context!.Attach(model);
					_context.Entry(model).State = EntityState.Added;
					await _context.SaveChangesAsync();

					if (token != null)
					{
						var basketProducts = _context!.ShopingBasket.Where(s => s.UserId.Equals(token));/// Start-->
						if (basketProducts != null)
						{
							foreach (var product in basketProducts)
								product.UserId = model.Email!;
							/// Save Changes
							_context.SaveChanges();

						}/// -->END

						 /// Update: UserWishList
						 /// UserId: cookieEmail -> UserId: contactEmail			 
						var wishListProducts = _context.UserWishList.Where(s => s.UserId.Equals(token));/// Start-->
						if (wishListProducts != null)
						{
							foreach (var product in wishListProducts)
								product.UserId = model.Email!;
							/// Save Changes
							_context.SaveChanges();
							/// END
						}/// -->END

						 /// Remove: oldUser
						var oldUser = await _userManager!.FindByEmailAsync(token);/// Start-->
						if (oldUser != null)
						{
							_app.Attach(oldUser);
							_app.Remove(oldUser);
							/// Save Changes
							_app.SaveChanges();
							/// -->END
						}
						/// SignIn -> NewUser
						user = await _signInManager!.UserManager.FindByEmailAsync(model.Email);
					}
				}
			}
			var result = await _signInManager!.CheckPasswordSignInAsync(user, model.Password, false);
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
				return "200";
			}
			else
				return "Password was Invalid";
		}
		public async Task<int> EntireUser(UserViewModel model, string token)
		{			
			var existingUser = _app!.Users.FirstOrDefault(u => u.Email == model.Email);
			var userShipping = _context!.UserShippings.FirstOrDefault(u => u.UserId == model.Email);

			if (existingUser != null)
			{
				if (userShipping != null)
					return 500;
				else
				{
					userShipping = new UserShipping
					{
						UserId = model.Email,
						Province = model.Province,
						City = model.City,
						PostalCode = model.PostalCode,
						Unit = model.Unit,
						Street = model.Street,
						Area = model.Area,
					};
					_context.Attach(userShipping);
					_context.Entry(userShipping).State = EntityState.Added;
					_context.SaveChanges();
					return 250;
				}
			}
			else
			{
				var user = new UserModel
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					PhoneNumber = model.PhoneNumber,
					Password = model.Password
				};
				string result = await Register(user, token);
				if (result.Equals("200"))
				{
					userShipping = new UserShipping
					{
						UserId = model.Email,
						Province = model.Province,
						City = model.City,
						PostalCode = model.PostalCode,
						Unit = model.Unit,
						Street = model.Street,
						Area = model.Area,
					};
					_context.Attach(userShipping);
					_context.Entry(userShipping).State = EntityState.Added;
					_context.SaveChanges();
					return 200;
				}
				else if (result.Equals("100"))
				{
					return 500;
				}
				else
				{
					return 404;
				}				
			}
		}
		public async Task ShippingUser(UserShipping model, string token)
		{
			var existingUser = _app!.Users.FirstOrDefault(u => u.Email == token);
			var userShipping = _context!.UserShippings.FirstOrDefault(u => u.UserId == token);

			if (existingUser != null)
			{
				if (userShipping == null)
				{
					userShipping = new UserShipping
					{
						UserId = token,
						Province = model.Province,
						City = model.City,
						PostalCode = model.PostalCode,
						Unit = model.Unit,
						Street = model.Street,
						Area = model.Area,
					};
					_context.Attach(userShipping);
					_context.Entry(userShipping).State = EntityState.Added;
					await _context.SaveChangesAsync();
				}
			}
		}
		public async Task<string> Cookie()
		{
			string generatedName = "cookieUserSix";
			string generatedPass = "3%@D8Iy2?Kt7*ceK";
			string generatedEmail = "cookie" + Guid.NewGuid().ToString() + "@swiftlink.com";
			var cookie = CreateUser();
			cookie.Email = generatedEmail;
			cookie.FirstName = generatedName;
			cookie.LastName = generatedName;
			cookie.PhoneNumber = generatedName;
			cookie.Joined = DateTime.Now;
			cookie.UserName = generatedEmail;
			IdentityUser identity = cookie;
			var resultCreate = await _userManager!.CreateAsync(cookie, generatedPass);
			Console.WriteLine(resultCreate.Errors);
			if (resultCreate.Succeeded)
			{
				Console.WriteLine("USER CREATED!! Creating UserModel...");
				var model = new UserModel
				{
					FirstName = cookie.FirstName,
					LastName = cookie.LastName,
					Email = cookie.Email,
					PhoneNumber = cookie.PhoneNumber,
					Joined = DateTime.Now
				};
				_context!.Attach(model);
				_context.Entry(model).State = EntityState.Added;			
				await _context.SaveChangesAsync();
				cookie = await _signInManager!.UserManager.FindByEmailAsync(generatedEmail);
				var result = await _signInManager!.CheckPasswordSignInAsync(cookie, generatedPass, false);
				if (result.Succeeded)
				{
					var claims = new List<Claim>
					{
						new Claim("arr", "pwd"),
					};
					var roles = await _signInManager.UserManager.GetRolesAsync(cookie);
					if (roles.Any())
					{
						//Gives Cookie [USER] Role
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					else
					{
						await _signInManager.UserManager.AddToRoleAsync(cookie, "User");
						var roleClaim = string.Join(",", roles);
						claims.Add(new Claim("Roles", roleClaim));
					}
					await _signInManager.SignInWithClaimsAsync(cookie, true, claims);
				}
			}
			return generatedEmail;
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
				var result = await _signInManager.CheckPasswordSignInAsync(userA, "3%@D8Iy2?Kt7*ceK", false);
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
				await Cookie();
			return "Successfully SignedIn!";
		}
	}
}
