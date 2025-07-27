using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using VillaHub.Application.Common.Interfaces;

namespace VillaHub.Application.Common.Utility
{
    public class EmailSender : ICustomEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return SendEmailInternalAsync(email, subject, htmlMessage, null, null);
        }

        public Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] pdfAttachment, string fileName = "attachment.pdf")
        {
            return SendEmailInternalAsync(email, subject, htmlMessage, pdfAttachment, fileName);
        }

        private Task SendEmailInternalAsync(string email, string subject, string htmlMessage, byte[]? pdfAttachment, string? fileName)
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
                htmlMessage)
            {
                IsBodyHtml = true
            };

            if (pdfAttachment != null)
            {
                var stream = new MemoryStream(pdfAttachment);
                var attachment = new Attachment(stream, fileName, MediaTypeNames.Application.Pdf);
                mailMessage.Attachments.Add(attachment);
            }

            return smtpClient.SendMailAsync(mailMessage);
        }
    }


}
