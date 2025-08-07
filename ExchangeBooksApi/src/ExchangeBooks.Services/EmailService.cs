using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Infra.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ExchangeBooks.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmail(string toEmail, string name, string body, string subject)
        {
            var apiKey = _configuration.GetSection("SENDGRID_API_KEY").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreply@exchangebooks.com", "exchangeBooks");
            List<EmailAddress> tos = new List<EmailAddress>
            {
              new EmailAddress(toEmail, name)
            };
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", body, false);
            var response = await client.SendEmailAsync(msg);
        }
    }
}