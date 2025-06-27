using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace VillaHub.Application.Common.Utility
{
    public class TwilioService
    {
        private readonly IConfiguration _config;

        public TwilioService(IConfiguration config)
        {
            _config = config;
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task SendWhatsAppMessage(string toNumber, string message)
        {
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];

            TwilioClient.Init(accountSid, authToken);

            var fromNumber = new PhoneNumber("whatsapp:+14155238886"); // Twilio Sandbox
            var to = new PhoneNumber($"whatsapp:{toNumber}");

            await MessageResource.CreateAsync(
                to: to,
                from: fromNumber,
                body: message
            );
        }

    }
}
