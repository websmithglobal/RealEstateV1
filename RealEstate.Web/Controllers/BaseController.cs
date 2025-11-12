using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstate.Web.Controllers
{
    /// <summary>
    /// Represents a base controller class providing common functionality, such as retrieving the current user's ID from authentication claims.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// Gets the current authenticated user's ID as a Guid from the authentication claims.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        public Guid GetCurrentUserId
        {
            get
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId);
                    return userId;
                }
                return Guid.Empty;
            }
        }
    }
}
