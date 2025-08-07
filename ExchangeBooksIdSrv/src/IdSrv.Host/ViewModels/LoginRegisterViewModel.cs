
namespace IdSrv.Host.ViewModels
{
    public class LoginRegisterViewModel : LoginViewModel
    {
        public RegisterViewModel RegisterViewModel { get; set; }
        public string SourceTab { get; set; } = "login";
    }
}