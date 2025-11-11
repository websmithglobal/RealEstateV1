using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interface;
using RealEstate.Core.Identity;

namespace RealEstate.Web.Controllers
{
    public class AccountController : Controller
    {
        #region Variable

        // Changed to private readonly as per best practices for injected dependencies
        private readonly ILogger<AccountController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        #endregion Variable

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for managing user accounts.</param>
        /// <param name="signInManager">The sign-in manager for handling user logins and logouts.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
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
                    // Handle two-factor authentication if implemented
                    _logger.LogWarning("Two-factor authentication required for user.");
                    // return RedirectToAction(nameof(VerifyCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return View("Lockout"); // Redirect to a lockout page
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login in AccountController/Login.");

                // Log to custom error reporting system if ERRORREPORTING is available
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();

                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login. Please try again.");
            }
            return View(model);
        }

        #endregion Login

        #region ForgotPassword

        /// <summary>
        /// GET: Displays the Forgot Password form.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }

        /// <summary>
        /// POST: Sends a password-reset email to the admin user.
        /// </summary>
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

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "email", "ForgotPassword.html");
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
        /// </summary>
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
        /// </summary>
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
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDTO());
        }

        /// <summary>
        /// POST: Changes the password for the logged-in admin.
        /// </summary>
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

        #region Helpers

        /// <summary>
        /// Redirects to the specified return URL if it's a local URL, otherwise redirects to the Home page.
        /// Prevents open redirection vulnerabilities.
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