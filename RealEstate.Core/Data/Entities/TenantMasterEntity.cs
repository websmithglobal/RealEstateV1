using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Core.Data.Entities
{
    /// <summary>
    /// Represents the entity for tenant master data in the database.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMasterEntity
    {
        [Key]
        public int? TenantIDP { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string TenantName { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? ContactNo { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string? EmailAddress { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
