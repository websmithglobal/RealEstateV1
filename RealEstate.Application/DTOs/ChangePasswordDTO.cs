using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for changing user password.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
