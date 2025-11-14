using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Core.Data.Entities
{
    /// <summary>
    /// Represents the master table for property types.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class PropertyTypeMasterEntity
    {
        [Key]
        public int? PropertyTypeIDP { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string PropertyTypeName { get; set; }

        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation List of Building Types
        public ICollection<BuildingTypeMasterEntity>? BuildingTypeMasters { get; set; }
    }

}
