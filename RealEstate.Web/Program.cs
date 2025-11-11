using Microsoft.AspNetCore.Identity;
using RealEstate.Core.Identity;
using RealEstate.Infrastructure.DatabaseSeeder;
using RealEstate.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Adds MVC controllers and views to the application.
/// </summary>
builder.Services.AddControllersWithViews();

/// <summary>
/// Adds Razor Pages support (required for Identity UI).
/// </summary>
builder.Services.AddRazorPages();

/// <summary>
/// Registers infrastructure services such as DbContext, Identity configuration,
/// repository services, and other application-level dependencies.
/// </summary>
builder.Services.AddInfrastructureServices(builder.Configuration);

/// <summary>
/// Enables authorization services for the application.
/// </summary>
builder.Services.AddAuthorization();

var app = builder.Build();

/// <summary>
/// Handles environment-specific configuration such as migration error pages
/// during development and exception handling/HSTS in production.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

/// <summary>
/// Redirects HTTP requests to HTTPS.
/// </summary>
app.UseHttpsRedirection();

/// <summary>
/// Enables serving static files from the wwwroot folder.
/// </summary>
app.UseStaticFiles();

/// <summary>
/// Adds routing capabilities to the middleware pipeline.
/// </summary>
app.UseRouting();

/// <summary>
/// Enables authentication using ASP.NET Core Identity.
/// </summary>
app.UseAuthentication();

/// <summary>
/// Enables authorization middleware for role and policy checks.
/// </summary>
app.UseAuthorization();

/// <summary>
/// Seeds default roles and admin user into the Identity system on startup.
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DatabaseSeeder.SeedDefaultDataAsync(userManager, roleManager);
}

/// <summary>
/// Configures default MVC routing for controllers.
/// Sets Account/Login as default route.
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

/// <summary>
/// Maps Razor Pages routing for Identity and page-based endpoints.
/// </summary>
app.MapRazorPages();

/// <summary>
/// Starts the web application.
/// </summary>
app.Run();
