using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models.Request;
using ExchangeBooks.Models.Response;
using ExchangeBooks.Navigation;
using ExchangeBooks.Views;
using Xamarin.Forms;
using static ExchangeBooks.Constants.Constants;

namespace ExchangeBooks.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IBooksService _bookService;
        private readonly IEventTracker _eventTracker;
        private readonly IMessagesService _messageService;
        private readonly IFcmUtility _fcmUtility;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHidePostsDataService _hidePostsDataService;
        #endregion

        #region Properties
        public int TopicType { get; private set; }
        public ObservableCollection<BookSearchResponse> BooksResponse { get; set; } = new ObservableCollection<BookSearchResponse>();
        public string SearchText { get; set; }
        public ICommand SearchClick => new Command(() => RunOnceOnly(SearchBooks));
        public ICommand ChatClick => new Command<BookSearchResponse>((bookResponse) => RunOnceOnly(() => OnChatClick(bookResponse)));
        public ICommand BookClick => new Command<BookSearchResponse>((bookResponse) => RunOnceOnly(() => OnBookClick(bookResponse)));
        public ICommand HideClick => new Command<BookSearchResponse>((bookResponse) => RunOnceOnly(() => OnHideClick(bookResponse)));
        public ICommand FlagClick => new Command<BookSearchResponse>((bookResponse) => RunOnceOnly(() => OnFlagClick(bookResponse)));
        public ICommand RefreshCommand => new Command(() => RunOnceOnly(GetRecentBooks));
        public bool IsRefreshing { get; set; }
        public bool DataFound { get; set; } = true;
        #endregion

        public SearchViewModel(IDialogService dialogService, IBooksService bookService
            , IEventTracker eventTracker, IMessagesService messageService, IFcmUtility fcmUtility
            , IAuthenticationService authenticationService, IHidePostsDataService hidePostsDataService) : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _bookService = bookService;
            _eventTracker = eventTracker;
            _messageService = messageService;
            _fcmUtility = fcmUtility;
            _authenticationService = authenticationService;
            _hidePostsDataService = hidePostsDataService;
            Title = "Search";
        }

        public async Task GetRecentBooks()
        {
            if (!IsRefreshing)
                _dialogService.ShowLoading();

            var books = await _bookService.GetRecentBooks(10);
            if (books.Any())
                DataFound = true;
            else
                DataFound = false;
            OnPropertyChanged(nameof(DataFound));

            await RefreshSearchView(books);

            if (!IsRefreshing)
                _dialogService.HideLoading();

            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        }

        public async Task SearchBooks()
        {
            var userName = await _authenticationService.GetUserEmail();
            if (string.IsNullOrWhiteSpace(SearchText))
                return;
            var tags = SearchText.Trim().Split(SearchSeparator);
            _dialogService.ShowLoading();

            var books = await _bookService.SearchBooks(tags);
            if (books.Any())
                DataFound = true;
            else
                DataFound = false;
            OnPropertyChanged(nameof(DataFound));

            await RefreshSearchView(books);

            _dialogService.HideLoading();

            if (!BooksResponse.Any() && await _dialogService.Confirm("Do you wish to be alerted when such a book is available?", "Subscribe", "Ok", "No"))
            {
                await _messageService.SubscribeToTopic(Enums.TopicType.Notify, tags);
                await Shell.Current.GoToAsync("//search/filter");
            }
        }

        public async Task OnChatClick(BookSearchResponse bookResponse)
        {
            await CheckAuthenticate(async () => await StartChat(bookResponse));
        }

        public async Task OnHideClick(BookSearchResponse bookResponse)
        {
            await CheckAuthenticate(async () => await HidePost(bookResponse));
        }

        public async Task OnFlagClick(BookSearchResponse bookResponse)
        {
            await CheckAuthenticate(async () => await FlagPost(bookResponse));
        }

        private async Task OnBookClick(BookSearchResponse bookResponse)
        {
            var parentPage = "searchBook";
            if (bookResponse?.Book != null)
                await Shell.Current.GoToAsync($"viewBook?parentPage={parentPage}&postId={bookResponse.PostId}&bookId={bookResponse.Book.Id}");
        }

        private async Task StartChat(BookSearchResponse bookResponse)
        {
            var userEmail = await _authenticationService.GetUserEmail();
            if (userEmail == bookResponse.CreatedBy)
            {
                await _dialogService.Alert("You posted this book :-)", "Your book", "Ok");
                return;
            }
            var tags = new string[] { userEmail, bookResponse.PostId.ToString() };
            _dialogService.ShowLoading();
            var topic = await _messageService.SubscribeToTopic(Enums.TopicType.Chat, tags);
            _dialogService.HideLoading();
            OnPropertyChanged(nameof(BooksResponse));
            if (topic != null)
                await Shell.Current.GoToAsync($"chat?topicId={topic.Id}&postName={bookResponse.PostName}");
        }

        private async Task HidePost(BookSearchResponse bookResponse)
        {
            var hideResponse = await PageNavigation.DisplayPopup<HidePostPage, HidePostEnum>();

            var request = new HidePostRequest();
            switch (hideResponse)
            {
                case HidePostEnum.Post:
                    _hidePostsDataService.HidePost(bookResponse.PostId);
                    request.PostId = bookResponse.PostId;
                    await _bookService.HidePost(request);
                    break;
                case HidePostEnum.All:
                    _hidePostsDataService.HideUser(bookResponse.CreatedBy);
                    request.UserEmail = bookResponse.CreatedBy;
                    await _bookService.HidePost(request);
                    break;
                default:
                    break;
            }

            var books = BooksResponse.ToList();
            await RefreshSearchView(books);
        }

        private async Task FlagPost(BookSearchResponse bookResponse)
        {
            var flagResponse = await PageNavigation.DisplayPopup<FlagPostPage, FlagPostEnum>();

            switch (flagResponse)
            {
                case FlagPostEnum.None:
                    break;
                default:
                    await _bookService.FlagPost(
                        new FlagPostRequest
                        {
                            PostId = bookResponse.PostId,
                            Reason = flagResponse
                        });
                    break;
            }
        }

        private async Task RefreshSearchView(List<BookSearchResponse> unfilteredBooks)
        {
            var userName = await _authenticationService.GetUserEmail();
            BooksResponse.Clear();
            var books = FilterBooks(unfilteredBooks);
            books.ForEach(b =>
            {
                b.IsChatVisible = !b.CreatedBy.Equals(userName, StringComparison.OrdinalIgnoreCase);
                BooksResponse.Add(b);
            });

            OnPropertyChanged(nameof(BooksResponse));
        }

        private List<BookSearchResponse> FilterBooks(List<BookSearchResponse> books)
        {
            var returnValue = books.Except(books.Where(b => _hidePostsDataService.HiddenPosts.PostIds.Contains(b.PostId)))
                .Except(books.Where(eb => _hidePostsDataService.HiddenPosts.UserEmailIds.Contains(eb.CreatedBy))).ToList();
            return returnValue;
        }
    }
}
