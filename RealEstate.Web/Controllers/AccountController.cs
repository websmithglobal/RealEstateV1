using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interface;
using RealEstate.Core.Identity;

namespace RealEstate.Web.Controllers
{
    /// <summary>
    /// Represents a controller for handling account-related operations including login, registration, password management, and logout.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class AccountController : BaseController
    {
        #region Variable
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        #endregion Variable

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userManager">The user manager for managing user accounts.</param>
        /// <param name="signInManager">The sign-in manager for handling user logins and logouts.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        /// <param name="webHostEnvironment">The web hosting environment for accessing files and paths.</param>
        /// <param name="emailSender">The email sender service for sending notifications.</param>
        public AccountController(UserManager<ApplicationUser> userManager
            , SignInManager<ApplicationUser> signInManager
            , ILogger<AccountController> logger,
                IWebHostEnvironment webHostEnvironment,
                IEmailSender emailSender)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }
        #endregion Constructor

        #region Login
        /// <summary>
        /// GET: /Account/Login
        /// Displays the login page.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after successful login.</param>
        /// <returns>The login view.</returns>
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// Handles user login.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="model">The login view model containing user credentials.</param>
        /// <param name="returnUrl">The URL to redirect to after successful login.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> for redirection or view display.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model, string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogWarning("Two-factor authentication required for user.");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Your account has been locked. You cannot login.");
                    return View(model);
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login in AccountController/Login.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login. Please try again.");
            }
            return View(model);
        }
        #endregion Login

        #region ForgotPassword
        /// <summary>
        /// GET: Displays the Forgot Password form.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// POST: Sends a password-reset email to the admin user.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="model">The forgot password view model containing the user's email.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> for redirection or view display.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Account not found.";
                return View(model);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { userId = user.Id, token = token },
                protocol: HttpContext.Request.Scheme);
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Email", "ForgotPassword.html");
            string template = System.IO.File.ReadAllText(filePath);
            string username = model.Email.Split('@')[0];
            string emailBody = template
                .Replace("{username}", username)
                .Replace("{callbackUrl}", callbackUrl);
            await _emailSender.SendEmailAsync(model.Email, "Reset Your Password", emailBody);
            TempData["SuccessMessage"] = "Password reset link has been sent.";
            return RedirectToAction("ForgotPassword");
        }
        #endregion ForgotPassword

        #region ResetPassword
        /// <summary>
        /// GET: Shows the reset password page with token.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userId">The ID of the user to reset the password for.</param>
        /// <param name="token">The password reset token.</param>
        /// <returns>The reset password view or a bad request if invalid.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid reset request.");
            return View(new ResetPasswordDTO
            {
                UserId = userId,
                Token = token
            });
        }

        /// <summary>
        /// POST: Resets the admin user's password using the provided token.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="model">The reset password view model containing user ID, token, and new password.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> for redirection or view display.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return View(model);
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                TempData["ErrorMessage"] = "Unable to reset password.";
                return View(model);
            }
            TempData["SuccessMessage"] = "Password reset successfully.";
            return RedirectToAction("Login");
        }
        #endregion ResetPassword

        #region ChangePassword
        /// <summary>
        /// GET: Shows the Change Password page for logged-in admin.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <returns>The change password view.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// POST: Changes the password for the logged-in admin.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="model">The change password view model containing current and new passwords.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> for redirection or view display.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return View(model);
            }
            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                TempData["ErrorMessage"] = "Unable to change password.";
                return View(model);
            }
            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction("ChangePassword");
        }
        #endregion ChangePassword

        #region Logout
        /// <summary>
        /// POST: /Account/LogOff
        /// Logs out the current user and redirects to the Login page.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> for redirection.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login", "Account");
        }
        #endregion Logout

        /// <summary>
        /// GET: Displays the registration page with assignable roles based on the current user's permissions.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the registration view with assignable roles.</returns>
        [HttpGet]
        public async Task<IActionResult> RegisterAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                throw new Exception("User not found");
            // Get roles (normalized role names may be returned)
            var roles = await _userManager.GetRolesAsync(user);
            // Determine which roles can be assigned
            var assignableRoles = GetAssignableRoles(roles);
            return View(assignableRoles);
        }

        /// <summary>
        /// Determines the roles that the current user is allowed to assign during registration.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userRoles">The roles of the current user.</param>
        /// <returns>A list of selectable role items based on user permissions.</returns>
        private List<SelectListItem> GetAssignableRoles(IList<string> userRoles)
        {
            var roles = new List<SelectListItem>();
            var normalizedRoles = userRoles.Select(r => r.ToUpper()).ToList();
            if (normalizedRoles.Contains("SUPERADMIN") || normalizedRoles.Contains("ADMIN"))
            {
                // SuperAdmin can assign all roles
                roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "SuperAdmin", Text = "SuperAdmin" },
                    new SelectListItem { Value = "Broker", Text = "Broker" },
                    new SelectListItem { Value = "Landlord", Text = "Landlord" },
                    new SelectListItem { Value = "Tenant", Text = "Tenant" }
                };
            }
            else if (normalizedRoles.Contains("BROKER"))
            {
                // Broker can assign only Landlord and Tenant
                roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Landlord", Text = "Landlord" },
                    new SelectListItem { Value = "Tenant", Text = "Tenant" }
                };
            }
            return roles;
        }

        #region Helpers
        /// <summary>
        /// Redirects to the specified return URL if it's a local URL, otherwise redirects to the Home page.
        /// Prevents open redirection vulnerabilities.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to.</param>
        /// <returns>An <see cref="IActionResult"/> for redirection.</returns>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion Helpers
    }
}