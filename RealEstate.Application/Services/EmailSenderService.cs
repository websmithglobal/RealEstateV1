using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interface;
using System.Net;
using System.Net.Mail;

namespace RealEstate.Application.Services
{
    /// <summary>
    /// Represents an implementation of the email sender service using SMTP configuration from appsettings.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        //private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the EmailSender with the provided configuration.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="config">The configuration instance for retrieving SMTP settings.</param>
        public EmailSender(IConfiguration config)
        {
            _config = config;
            //_mapper = mapper;
        }

        /// <summary>
        /// Sends an email asynchronously to the specified recipient with the given subject and body using SMTP.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            //var appUser = _mapper.Map<ApplicationUser>(dto);
            var smtpClient = new SmtpClient
            {
                Host = _config["SMTP:Host"],
                Port = int.Parse(_config["SMTP:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["SMTP:Username"],
                    _config["SMTP:Password"]
                )
            };
            var message = new MailMessage
            {
                From = new MailAddress(_config["SMTP:SenderEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(email);
            await smtpClient.SendMailAsync(message);
        }
    }
}
