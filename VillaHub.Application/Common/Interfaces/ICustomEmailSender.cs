using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace VillaHub.Application.Common.Interfaces
{
    public interface ICustomEmailSender : IEmailSender
    {
        Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] pdfAttachment, string fileName = "attachment");
    }
}
