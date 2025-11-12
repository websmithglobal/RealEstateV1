using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for saving or updating user master details.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class UserMasterSaveDTO
    {
        // If UserIDP > 0 → Update existing user; otherwise → Create new user
        public int? UserIDP { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(50)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; }

        [StringLength(20, MinimumLength = 6)]
        public string? Password { get; set; } // optional during update

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        public Guid UserIDF { get; set; }
    }
}
