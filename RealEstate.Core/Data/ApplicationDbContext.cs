using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstate.Core.Data.Entities;
using RealEstate.Core.Data.EntityConfigurations;
using RealEstate.Core.Identity;
using RealEstate.Infrastructure.Data.Entities;

namespace RealEstate.Core.Data
{
    /// <summary>
    /// Represents the application database context, inheriting from IdentityDbContext for ASP.NET Identity integration and including custom entities like UserMaster.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext with the specified options.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="options">The database context options for configuration.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserMasterEntity> UserMaster { get; set; }
        public DbSet<TenantMasterEntity> TenantMaster { get; set; }
        public DbSet<PropertyTypeMasterEntity> PropertyTypeMaster { get; set; }
        public DbSet<BuildingTypeMasterEntity> BuildingTypeMaster { get; set; }


        /// <summary>
        /// Configures the model for Entity Framework, applying base Identity configurations and custom entity mappings for UserMasterEntity.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="modelBuilder">The model builder instance for configuration.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserMasterConfig());
            modelBuilder.ApplyConfiguration(new TenantMasterConfig());
            modelBuilder.ApplyConfiguration(new PropertyTypeMasterConfig());
            modelBuilder.ApplyConfiguration(new BuildingTypeMasterConfig());
        }
    }
}
