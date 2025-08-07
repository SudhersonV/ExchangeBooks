using System.Net;
using IdSrv.Host.Models;

namespace IdSrv.Host.ViewModels
{
    public class RegisterViewModel : RegisterUserModel
    {
        public bool IsAfterRegistrationPost { get; set; }
        public HttpStatusCode RegistrationStatus { get; set; }
    }
}
