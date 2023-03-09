using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Web.Helpers;
using WebScrapper_Prototype.Areas.Identity.Data;
using WebScrapper_Prototype.Data;
using OpenAI.API;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<WebScrapper_PrototypeContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("WebScrapper_PrototypeContext") ?? throw new InvalidOperationException("Connection string 'WebScrapper_PrototypeContext' not found.")));
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllersWithViews();
AddAuthorizationPolicies(builder.Services);

AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void AddAuthorizationPolicies(IServiceCollection services)
{
	services.AddAuthorization(options =>
	{
		options.AddPolicy("OwnerOnly", policy => policy.RequireClaim("OwnerId"));
	});

	services.AddAuthorization(options =>
	{
		options.AddPolicy("RequireAdmin", policy => policy.RequireClaim("Administrator"));
		options.AddPolicy("RequireManager", policy => policy.RequireClaim("Manager"));
	});
}
