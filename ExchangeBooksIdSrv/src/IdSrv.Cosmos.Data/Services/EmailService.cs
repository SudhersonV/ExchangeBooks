using System.Threading.Tasks;
using IdSrv.Cosmos.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace IdSrv.Cosmos.Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendRegistrationEmail(string toEmail, string toName, string body)
        {
            await SendEmail(toEmail, toName, null, null, body, "ExchangeBooks Account");
        }

        public async Task SendSupportEmail(string fromEmail, string fromName, string body)
        {
            await SendEmail(null, null, fromEmail, fromName, body, "ExchangeBooks Support");
        }

        private async Task SendEmail(string toEmail, string toName, string fromEmail, string fromName, string body, string subject)
        {
            var apiKey = _configuration.GetSection("SENDGRID_API_KEY").Value;
            var client = new SendGridClient(apiKey);

            var tosE = new List<EmailAddress>();
            var toN = string.Empty;
            var fromE = new EmailAddress();
            var fromN = string.Empty;

            if (string.IsNullOrEmpty(toEmail))
                tosE.Add(new EmailAddress("sudherson.v@gmail.com", "Sudherson V"));
            else
                tosE.Add(new EmailAddress(toEmail, toName));

            if (string.IsNullOrEmpty(fromEmail))
                fromE = new EmailAddress("noreply@exchangebooks.com", "exchangeBooks");
            else
                fromE = new EmailAddress(fromEmail, fromName);

            //var displayRecipients = false; // set this to true if you want recipients to see each others mail id 
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(fromE, tosE, subject, "", body, false);
            var response = await client.SendEmailAsync(msg);
        }
    }
}