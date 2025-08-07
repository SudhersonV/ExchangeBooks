using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models.Response;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class ViewPostViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMyPostsDataService _myPostsDataService;
        private readonly IBooksService _bookService;
        #endregion

        #region Properties
        public PostResponse Post { get; set; }
        public bool IsPostOpen => Post?.Status == PostStatus.Open;
        public BookResponse CurrentBook { get; set; }
        public ICommand PostStatusChanged => new Command<bool>((flag) => RunOnceOnly(() => OnPostStatusChanged(flag)));
        public ICommand BookClicked => new Command<BookResponse>((book) => RunOnceOnly(() => OnBookClicked(book)));
        public ICommand DeletePost => new Command(() => RunOnceOnly(OnDeletePostClicked));
        #endregion

        #region Constructor
        public ViewPostViewModel(IDialogService dialogService, IAuthenticationService authenticationService,
            IMyPostsDataService myPostsDataService, IBooksService bookService) : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _authenticationService = authenticationService;
            _myPostsDataService = myPostsDataService;
            _bookService = bookService;
            Title = "Post View";
        }
        #endregion

        #region Public Methods
        public async Task SetPost(Guid postId)
        {
            Post = _myPostsDataService.Posts.Find(p => p.Id == postId);
            if (Post is null)
            {
                await _dialogService.Alert("Invalid post id", "Invalid post", "Ok");
                await Shell.Current.GoToAsync("..");
            }
            _myPostsDataService.CurrentPost = Post;
            OnPropertyChanged(nameof(Post));
            OnPropertyChanged(nameof(IsPostOpen));
        }

        private async Task OnPostStatusChanged(bool flag)
        {
            var status = flag ? PostStatus.Open : PostStatus.Closed;
            await _bookService.MarkPostStatus(Post.Id, status);
            Post.Status = status;
            OnPropertyChanged(nameof(Post));
            OnPropertyChanged(nameof(IsPostOpen));
        }

        private async Task OnBookClicked(BookResponse book)
        {
            var parentPage = "viewPost";
            if (book != null)
                await Shell.Current.GoToAsync($"viewBook?parentPage={parentPage}&postId={Post.Id}&bookId={book.Id}");
        }

        private async Task OnDeletePostClicked()
        {
            await _bookService.DeletePost(Post.Id);
            await Shell.Current.GoToAsync("..");
        }
        #endregion
    }
}
