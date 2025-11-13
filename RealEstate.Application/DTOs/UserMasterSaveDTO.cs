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
        public int? UserIDP { get; set; }

        [Required(ErrorMessage = "Please enter Full Name.")]
        [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
        [MinLength(5, ErrorMessage = "Full name must be at least 5 characters long.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Full name can contain only alphabets and spaces.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter Email.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters long.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter phone Number.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 digits.")]
        [MinLength(5, ErrorMessage = "Phone number must be at least 5 digits long.")]
        [RegularExpression(@"^[1-9]\d{4,14}$", ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Please enter Password.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{6,20}$",
    ErrorMessage = "Password must contain at least one lowercase, one uppercase, one digit, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter confirm Password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; }

        public Guid UserIDF { get; set; }
    }
}
