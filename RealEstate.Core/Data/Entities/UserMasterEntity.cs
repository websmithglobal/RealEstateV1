using System.ComponentModel.DataAnnotations;

namespace RealEstate.Infrastructure.Data.Entities
{
    /// <summary>
    /// Represents the entity for user master data in the database, linked to ASP.NET Identity.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class UserMasterEntity
    {
        [Key]
        public int UserIDP { get; set; }
        public Guid AspNetUserIDF { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDelete { get; set; }
    }
}
