using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using ExchangeBooks.Interfaces.Framework;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using ExchangeBooks.Navigation;
using ExchangeBooks.Views;

namespace ExchangeBooks.Core.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Variables
        private readonly IAuthenticationService _authenticationService;
        private readonly IDialogService _dialogService;
        private bool isBusy = false;
        private bool isRunning = false;
        private string title = string.Empty;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public BaseViewModel(IAuthenticationService authenticationService, IDialogService dialogService)
        {
            _authenticationService = authenticationService;
            _dialogService = dialogService;
        }
        #endregion

        #region Properties
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }

        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(nameof(Title)); }
        }
        #endregion

        #region Protected
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected async void RunOnceOnly(Func<Task> func)
        {
            if (isRunning)
            {
                Debug.WriteLine("Stopped multiple actions");
                return;
            }

            isRunning = true;
            await Task.Yield();

            try
            {
                await func();
                await Task.Yield();
            }
            finally
            {
                isRunning = false;
            }
        }

        protected async Task CheckAuthenticate(Func<Task> func = null)
        {
            func ??= (async () => await Task.FromResult(0));
            if (await _authenticationService.IsUserAuthenticated())
            {
                await func();
                return;
            }
            var response = await _dialogService.Confirm("Login required to access this tab", "Login in", "Login", "Go to Search");
            if (response)
                await Shell.Current.GoToAsync("//profile/profile");
            else
                await Shell.Current.GoToAsync("//search/search");
        }

        protected async Task BiometricAuthenticate()
        {
            bool isFingerprintAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
            if (!isFingerprintAvailable)
            {
                await _dialogService.Alert("Error", "Biometric authentication is not available or is not configured.", "OK");
                return;
            }

            var conf = new AuthenticationRequestConfiguration("Authentication", "Authenticate access to your personal data");
            var authResult = await CrossFingerprint.Current.AuthenticateAsync(conf);
            if (authResult.Authenticated)
            {
                _dialogService.ShowLoading();
                await _authenticationService.RefreshAccessToken();
                _dialogService.HideLoading();
                await CheckEulaAcceptance();
            }
            else
            {
                await _dialogService.Alert("Error", "Authentication failed", "OK");
            }
        }

        protected async Task CheckEulaAcceptance()
        {
            var accepted = await _authenticationService.GetEulaAcceptance();
            if (accepted) return;
            await PageNavigation.DisplayPopup<EulaPage, bool>();
        }
        #endregion
    }
}
