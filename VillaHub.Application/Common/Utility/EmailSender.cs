using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace VillaHub.Application.Common.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient(_configuration["EmailSettingsGmail:SmtpServer"], int.Parse(_configuration["EmailSettingsGmail:Port"]!))
            {
                Credentials = new NetworkCredential(
                    _configuration["EmailSettingsGmail:SenderEmail"],
                    _configuration["EmailSettingsGmail:SenderPassword"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(
                from: _configuration["EmailSettingsGmail:SenderEmail"]!,
                to: email,
                subject,
                htmlMessage
            )
            {
                IsBodyHtml = true
            };

            return smtpClient.SendMailAsync(mailMessage);
        }
    }
}
