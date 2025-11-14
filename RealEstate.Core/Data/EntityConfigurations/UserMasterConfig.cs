using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Infrastructure.Data.Entities;

namespace RealEstate.Core.Data.EntityConfigurations
{
    public class UserMasterConfig : IEntityTypeConfiguration<UserMasterEntity>
    {
        /// <summary>
        /// Configures the UserMasterEntity using Fluent API.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        public void Configure(EntityTypeBuilder<UserMasterEntity> entity)
        {
            // PostgreSQL defaults applied exactly like your example
            entity.HasKey(e => e.UserIDP);

            entity.Property(e => e.UserIDP)
                  .UseIdentityAlwaysColumn()
                  .HasIdentityOptions(startValue: 1, incrementBy: 1);

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("NOW()");

            entity.Property(e => e.IsDelete)
                  .HasDefaultValue(false);
        }
    }
}
