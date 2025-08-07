using System.Threading.Tasks;

namespace IdSrv.Cosmos.Data.Interfaces
{
    public interface IEmailService
    {
         Task SendRegistrationEmail(string toEmail, string toName, string body);
         Task SendSupportEmail(string fromEmail, string fromName, string body);
    }
}