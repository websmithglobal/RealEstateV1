namespace RealEstate.Application.Interface
{
    /// <summary>
    /// Represents an interface for sending emails asynchronously.
    /// Created By - Prashant
    /// Created Date - 12.11.2025
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email asynchronously to the specified recipient with the given subject and body.
        /// Created By - Prashant
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string email, string subject, string body);
    }
}
