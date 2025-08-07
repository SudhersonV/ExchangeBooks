using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Constants;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventTracker _eventTracker;
        private readonly IBooksService _booksService;
        private bool IsLoggedIn;
        #endregion

        #region Properties
        public bool EnabledBiometrics { get; set; }
        public bool IsBiometricsOn { get; set; }
        public string LoginTxt { get; set; }
        public bool ShowBiometrics { get; set; }
        public bool ShowUserInfo { get; set; }
        public bool ShowUserName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string SupportUrl { get; set; }
        public ICommand BiometricsChanged => new Command<bool>((flag) => RunOnceOnly(() => OnBiometricsChanged(flag)));
        public ICommand LoginClicked => new Command(() => RunOnceOnly(OnLoginClick));
        public ICommand BiometricClicked => new Command(() => RunOnceOnly(OnBiometricClicked));
        public ICommand SupportClicked => new Command<string>(async (url) => await Launcher.OpenAsync(url));
        #endregion

        #region Constructor
        public ProfileViewModel(IDialogService dialogService
            , IEventTracker eventTracker, IAuthenticationService authenticationService
            , IBooksService booksService) : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _authenticationService = authenticationService;
            _eventTracker = eventTracker;
            _booksService = booksService;
            Title = "Profile";
            Init();
        }
        #endregion

        #region Private Methods
        private async Task OnLoginClick()
        {
            if (IsLoggedIn)
            {
                _eventTracker.SendEvent("UserProfile", "Logout", "Click");
                _authenticationService.Logout();
            }
            else
            {
                _eventTracker.SendEvent("UserProfile", "Login", "Click");
                await _authenticationService.Login();
                await CheckEulaAcceptance();
                await _booksService.GetHiddenPosts();
            }
            await OnAppearing();
        }

        private async Task OnBiometricsChanged(bool flag)
        {
            await _authenticationService.SetBiometricsFlag(flag);
            EnabledBiometrics = flag;
            await OnAppearing();
        }

        public async Task OnBiometricClicked()
        {
            await BiometricAuthenticate();
            await _booksService.GetHiddenPosts();
            await OnAppearing();
        }

        private void Init()
        {
            _eventTracker.SetCurrentScreen("UserProfile", nameof(ProfileViewModel));
            SupportUrl = $"{Constants.Constants.IdentityServer.Url}/{Constants.Constants.IdentityServer.Paths.Support}";
        }

        public async Task OnAppearing()
        {
            EnabledBiometrics = IsLoggedIn = await _authenticationService.IsUserAuthenticated();
            LoginTxt = IsLoggedIn ? LabelConstants.Logout : LabelConstants.Login;
            IsBiometricsOn = await _authenticationService.IsBiometricsEnabled();
            ShowBiometrics = IsBiometricsOn && !IsLoggedIn && await _authenticationService.HasRefreshToken();
            ShowUserInfo = await _authenticationService.HasRefreshToken();
            UserName = await _authenticationService.GetUserName();
            Email = await _authenticationService.GetUserEmail();
            var identityProvider = await _authenticationService.GetIdentityProvider();
            ShowUserName = identityProvider.ToLower() != "apple";
            OnPropertyChanged(nameof(LoginTxt));
            OnPropertyChanged(nameof(EnabledBiometrics));
            OnPropertyChanged(nameof(IsBiometricsOn));
            OnPropertyChanged(nameof(ShowBiometrics));
            OnPropertyChanged(nameof(ShowUserInfo));
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(ShowUserName));
        }
        #endregion
    }
}