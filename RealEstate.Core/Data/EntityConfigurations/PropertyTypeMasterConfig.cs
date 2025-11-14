using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Core.Data.Entities;

namespace RealEstate.Core.Data.EntityConfigurations
{
    public class PropertyTypeMasterConfig : IEntityTypeConfiguration<PropertyTypeMasterEntity>
    {
        /// <summary>
        /// Configures the PropertyTypeEntity using Fluent API.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        public void Configure(EntityTypeBuilder<PropertyTypeMasterEntity> entity)
        {
            entity.HasKey(e => e.PropertyTypeIDP);

            entity.Property(e => e.PropertyTypeIDP)
                  .UseIdentityAlwaysColumn()
                  .HasIdentityOptions(startValue: 1, incrementBy: 1);

            entity.Property(e => e.PropertyTypeName)
                  .IsRequired()
                  .HasColumnType("varchar(150)");

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.IsDelete)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("NOW()");
        }
    }
}
