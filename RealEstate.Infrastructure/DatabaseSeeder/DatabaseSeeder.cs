using Microsoft.AspNetCore.Identity;
using RealEstate.Core.Identity;

namespace RealEstate.Infrastructure.DatabaseSeeder
{
    /// <summary>
    /// Represents a static utility class for seeding the database with default roles and users.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeds the database asynchronously with default roles (SuperAdmin, Broker, Landlord, Tenant) and a default admin user.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userManager">The user manager for creating and managing users.</param>
        /// <param name="roleManager">The role manager for creating and managing roles.</param>
        /// <returns>A task representing the asynchronous seeding operation.</returns>
        public static async Task SeedDefaultDataAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Default roles
            string[] roles = { "SuperAdmin", "Broker", "Landlord", "Tenant" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            // Default admin user
            string adminEmail = "admin@admin.com";
            string adminPassword = "Admin@123";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
            }
        }
    }
}
