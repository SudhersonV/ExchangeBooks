using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Constants;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models;
using ExchangeBooks.Services.Framework;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class AddPostViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IBooksService _bookService;
        private readonly IPostDataService _postDataService;
        private readonly IDialogService _dialogService;
        private readonly IAuthenticationService _authenticationService;
        #endregion

        #region Properties
        public ICommand PostClick => new Command(() => RunOnceOnly(OnPost));
        #endregion

        #region Constructor
        public AddPostViewModel(IBooksService bookService, IPostDataService postDataService,
            IDialogService dialogService, IAuthenticationService authenticationService) : base(authenticationService, dialogService)
        {
            _bookService = bookService;
            _postDataService = postDataService;
            _dialogService = dialogService;
            _authenticationService = authenticationService;
            Title = "Post";
        }
        #endregion

        #region Public Methods
        public void InitializeMessenger()
        {   
        }
        #endregion

        #region Private Methods
        private async Task OnPost()
        {
            var postRequest = _postDataService.Post;

            #region Validate request
            ValidationContext vc = new ValidationContext(postRequest);
            ICollection<ValidationResult> results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(postRequest, vc, results, true);
            if (!isValid)
            {
                await _dialogService.Alert("Validate fields using Next button in each tab.", "Error", "Ok");
                return;
            }
            #endregion

            await CheckAuthenticate(async () => await Post(postRequest));
        }

        private async Task Post(PostRequest postRequest)
        {
            _dialogService.ShowLoading();
            var result = await _bookService.Post(postRequest);
            _dialogService.HideLoading();
            if (result != null)
            {
                _dialogService.Toast("Woot!! Post added");
                await Shell.Current.GoToAsync("//profile/posts");
            }
            else
                await _dialogService.Alert("Please try again later", "Error adding post", "Ok");
        }
        #endregion
    }
}
