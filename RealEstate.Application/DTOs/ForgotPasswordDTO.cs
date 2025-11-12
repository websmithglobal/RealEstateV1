using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for forgot password functionality.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }
    }
}
