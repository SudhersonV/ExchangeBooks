using System.Threading.Tasks;

namespace ExchangeBooks.Infra.Interfaces
{
    public interface IEmailService
    {
         Task SendEmail(string toEmail, string name, string body, string subject);
    }
}