using System.ComponentModel.DataAnnotations;
namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for user registration.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(50)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [Phone]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string? Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }

}