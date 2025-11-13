using System.ComponentModel.DataAnnotations;
namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for tenant master details.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMasterDTO
    {
        public int SrNo { get; set; }
        public int TenantIDP { get; set; }
        public string TenantName { get; set; }
        public string ContactNo { get; set; }
        public string EmailAddress { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a paging response for tenant master entities.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMasterEntityPaging
    {
        public List<TenantMasterDTO> Records { get; set; }
        public int TotalRecord { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object for saving or updating tenant master details.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMasterSaveDTO
    {
        public int? TenantIDP { get; set; }
        [Required(ErrorMessage = "Please enter Full Name.")]
        [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
        [MinLength(5, ErrorMessage = "Full name must be at least 5 characters long.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Full name can contain only alphabets and spaces.")]
        public string TenantName { get; set; }
        [Required(ErrorMessage = "Please enter Email.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters long.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; }
        [Required(ErrorMessage = "Please enter phone Number.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 digits.")]
        [MinLength(5, ErrorMessage = "Phone number must be at least 5 digits long.")]
        [RegularExpression(@"^[1-9]\d{4,14}$", ErrorMessage = "Please enter a valid phone number.")]
        public string ContactNo { get; set; }
        [Required(ErrorMessage = "Please enter address.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters long.")]
        public string Address { get; set; }
        public Guid UserIDF { get; set; }
    }
}