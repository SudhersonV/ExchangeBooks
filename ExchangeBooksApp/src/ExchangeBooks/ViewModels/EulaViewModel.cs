using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using Xamarin.Forms;
using static ExchangeBooks.Constants.Constants.IdentityServer;

namespace ExchangeBooks.ViewModels
{
    public class EulaViewModel: PopupViewModel<bool>
    {
        #region Variables
        private readonly IAuthenticationService _authenticationService;
        #endregion

        #region Properties
        public ICommand AcceptCmd => new Command(() => RunOnceOnly(OnAccept));
        public ICommand DeclineCmd => new Command(() => RunOnceOnly(OnDecline));
        public string SourceUrl { get; set; }
        #endregion

        #region Constructor
        public EulaViewModel(IAuthenticationService authenticationService, IDialogService dialogService)
            : base(authenticationService, dialogService)
        {
            _authenticationService = authenticationService;
            Title = "Term Of Use";
            SourceUrl = $"{Url}/{Paths.Eula}";
        }
        #endregion

        #region Private Methods
        private async Task OnAccept()
        {
            await _authenticationService.SetEulaAcceptance();
            Result = true;
            Close();
        }

        private async Task OnDecline()
        {
            _authenticationService.Logout();
            Result = false;
            Close();
        }
        #endregion
    }
}
