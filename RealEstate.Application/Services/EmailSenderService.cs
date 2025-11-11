using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interface;
using System.Net;
using System.Net.Mail;

namespace RealEstate.Application.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
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
