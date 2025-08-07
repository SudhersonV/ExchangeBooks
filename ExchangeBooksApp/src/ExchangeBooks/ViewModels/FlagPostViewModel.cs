using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Framework;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class FlagPostViewModel: PopupViewModel<FlagPostEnum>
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
        public FlagPostViewModel(IAuthenticationService authenticationService, IDialogService dialogService)
            : base(authenticationService, dialogService)
        {
            Title = "Select flag options";
        }
        #endregion

        #region Private Methods
        private void OnSubmitCmd()
        {
            switch (SelectedOption)
            {
                case (int)FlagPostEnum.Bad:
                    Result = FlagPostEnum.Bad;
                    break;
                case (int)FlagPostEnum.Vulgar:
                    Result = FlagPostEnum.Vulgar;
                    break;
                case (int)FlagPostEnum.Sexual:
                    Result = FlagPostEnum.Sexual;
                    break;
                default:
                    Result = FlagPostEnum.None;
                    break;
            }
            Close();
        }

        private void OnCancelCmd()
        {
            SelectedOption = (int)FlagPostEnum.None;
            Result = FlagPostEnum.None;
            Close();
        }
        #endregion
    }
}
