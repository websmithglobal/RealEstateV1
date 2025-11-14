using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Core.Data.Entities;

namespace RealEstate.Core.Data.EntityConfigurations
{
    public class TenantMasterConfig : IEntityTypeConfiguration<TenantMasterEntity>
    {
        /// <summary>
        /// Configures the TenantMasterEntity using Fluent API.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        public void Configure(EntityTypeBuilder<TenantMasterEntity> entity)
        {
            entity.HasKey(e => e.TenantIDP);

            entity.Property(e => e.TenantIDP)
                  .UseIdentityAlwaysColumn()
                  .HasIdentityOptions(startValue: 1, incrementBy: 1);

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.IsDelete)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("NOW()");
        }
    }
}
