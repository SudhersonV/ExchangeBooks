using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models.Response;
using Xamarin.Forms;
using ExchangeBooks.Core.ViewModels;

namespace ExchangeBooks.ViewModels
{
    public class MyPostsViewModel : BaseViewModel
    {
        #region Variables
        private readonly IBooksService _bookService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDialogService _dialogService;
        #endregion

        #region Properties
        public ObservableCollection<PostResponse> UserPosts { get; set; } = new ObservableCollection<PostResponse>();
        public ICommand RefreshCommand => new Command(() => RunOnceOnly(GetUserPosts));
        public ICommand ViewCommand => new Command<PostResponse>((postResponse) => RunOnceOnly(() => OnViewCommand(postResponse)));
        public ICommand DeleteCommand => new Command<PostResponse>((postResponse) => RunOnceOnly(() => OnDeleteCommand(postResponse)));
        public bool DataFound { get; set; } = true;
        #endregion

        #region Constructor
        public MyPostsViewModel(IAuthenticationService authenticationService, IBooksService bookService
            , IDialogService dialogService) : base(authenticationService, dialogService)
        {
            _authenticationService = authenticationService;
            _bookService = bookService;
            _dialogService = dialogService;
            Title = "My Posts";
        }
        #endregion

        #region Public Methods
        public async Task OnAppearing()
        {
            await CheckAuthenticate(async () => await GetUserPosts());
        }

        private async Task GetUserPosts()
        {
            _dialogService.ShowLoading();
            var userPosts = await _bookService.GetUserPosts();
            if (userPosts.Any())
                DataFound = true;
            else
                DataFound = false;
            OnPropertyChanged(nameof(DataFound));

            UserPosts.Clear();
            userPosts.ForEach(up => {
                UserPosts.Add(up);
            });
            _dialogService.HideLoading();
        }

        private async Task OnViewCommand(PostResponse postResponse)
        {
            await Shell.Current.GoToAsync($"viewPost?postId={postResponse.Id}");
        }

        private async Task OnDeleteCommand(PostResponse postResponse)
        {
            await _bookService.DeletePost(postResponse.Id);
        }
        #endregion
    }
}
