using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class HidePostViewModel : PopupViewModel<HidePostEnum>
    {
        #region Variables
        private readonly IAuthenticationService _authenticationService;
        private readonly IDialogService _dialogService;
        private int _selectedOption = 0;
        #endregion

        #region Properties
        public int SelectedOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                OnPropertyChanged(nameof(IsSubmitEnabled));
            }
        }
        public ICommand CancelCmd => new Command(OnCancelCmd);
        public ICommand SubmitCmd => new Command(OnSubmitCmd);
        public bool IsSubmitEnabled => SelectedOption > 0;
        #endregion

        #region Constructor
        public HidePostViewModel(IAuthenticationService authenticationService, IDialogService dialogService)
            : base(authenticationService, dialogService)
        {
            Title = "Select hide options";
        }
        #endregion

        #region Private Methods
        private void OnSubmitCmd()
        {
            switch (SelectedOption)
            {
                case (int)HidePostEnum.Post:
                    Result = HidePostEnum.Post;
                    break;
                case (int)HidePostEnum.All:
                    Result = HidePostEnum.All;
                    break;
                default:
                    Result = HidePostEnum.None;
                    break;
            }
            Close();
        }

        private void OnCancelCmd()
        {
            SelectedOption = (int)HidePostEnum.None;
            Result = HidePostEnum.None;
            Close();
        }
        #endregion
    }
}
