using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Core.Data.Entities;

namespace RealEstate.Core.Data.EntityConfigurations
{
    public class BuildingTypeMasterConfig : IEntityTypeConfiguration<BuildingTypeMasterEntity>
    {
        /// <summary>
        /// Configures the BuildingTypeEntity using Fluent API.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        public void Configure(EntityTypeBuilder<BuildingTypeMasterEntity> entity)
        {
            entity.HasKey(e => e.BuildingTypeIDP);

            entity.Property(e => e.BuildingTypeIDP)
                  .UseIdentityAlwaysColumn()
                  .HasIdentityOptions(startValue: 1, incrementBy: 1);

            entity.Property(e => e.BuildingTypeName)
                  .IsRequired()
                  .HasColumnType("varchar(150)");

            entity.Property(e => e.PropertyTypeIDF)
                  .IsRequired();

            entity.HasOne(e => e.PropertyTypeMaster)
                  .WithMany(p => p.BuildingTypeMasters)
                  .HasForeignKey(e => e.PropertyTypeIDF);

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            entity.Property(e => e.IsDelete)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedDate)
                  .HasDefaultValueSql("NOW()");
        }
    }
}
