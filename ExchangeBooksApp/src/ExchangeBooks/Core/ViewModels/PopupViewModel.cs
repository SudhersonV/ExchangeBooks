using System.Windows.Input;
using ExchangeBooks.Interfaces.Framework;
using Xamarin.Forms;

namespace ExchangeBooks.Core.ViewModels
{
    public class PopupViewModel<R> : BaseViewModel
    {
        public PopupViewModel(IAuthenticationService authenticationService, IDialogService dialogService)
            : base(authenticationService, dialogService)
        { }

        public bool IsShowing { get; set; }
        public bool BackgroundDismiss { get; set; }
        public ICommand CloseCommand => new Command(ClosePopup);
        public R Result { get; protected set; }

        public bool HandleBackPressed(Page page)
        {
            Close();
            return false;
        }

        public void HandleClose()
        {
            Close();
        }

        public void Close()
        {
            IsShowing = false;
            OnPropertyChanged(nameof(IsShowing));
        }

        private void ClosePopup()
        {
            Close();
        }

    }
}
