using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstate.Application.Interface;
using RealEstate.Application.Mapping;
using RealEstate.Application.Services;
using RealEstate.Core.Data;
using RealEstate.Core.Identity;

namespace RealEstate.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extension methods for IServiceCollection to add infrastructure services including DbContext, Identity, AutoMapper, and custom services.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds infrastructure services to the specified IServiceCollection using the provided configuration.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="configuration">The IConfiguration instance for retrieving connection strings and other settings.</param>
        /// <returns>The IServiceCollection with added infrastructure services.</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext (PostgreSQL)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            // Register Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // add automapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
                cfg.AddProfile<TenantMappingProfile>();
                cfg.AddProfile<PropertyTypeMasterMappingProfile>();
                cfg.AddProfile<BuildingTypeMasterMappingProfile>();
            });
            // Resolve dependencies
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IUserMaster, UserMasterService>();
            services.AddScoped<ITenantMaster, TenantMasterService>();
            services.AddScoped<IPropertyTypeMasterService, PropertyTypeMasterService>();
            services.AddScoped<IBuildingTypeMasterService, BuildingTypeMasterService>();
            return services;
        }
    }
}
