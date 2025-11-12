using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for user login credentials and options.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class LoginDTO
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
