using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using System.Security.Claims;

namespace RealEstate.Web.Controllers
{
    public class AccountController : Controller
    {
        #region Variable

        // Changed to private readonly as per best practices for injected dependencies
        private readonly ILogger<AccountController> _logger;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        #endregion Variable

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for managing user accounts.</param>
        /// <param name="signInManager">The sign-in manager for handling user logins and logouts.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public AccountController(UserManager<IdentityUser> userManager
            , SignInManager<IdentityUser> signInManager
            , ILogger<AccountController> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                Guid UpdatedBy = Guid.Empty; // Default if user is not authenticated yet
                if (User.Identity.IsAuthenticated)
                {
                    Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out UpdatedBy);
                }

                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login. Please try again.");
            }
            return View(model);
        }

        #endregion Login

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