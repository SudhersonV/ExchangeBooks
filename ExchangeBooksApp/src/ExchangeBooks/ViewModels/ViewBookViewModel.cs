using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ViewBookViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMyPostsDataService _myPostsDataService;
        private readonly ISearchBooksDataService _searchBooksDataService;
        private readonly IBooksService _bookService;
        private Guid _postId = Guid.Empty;
        private string _parentPage = string.Empty;
        #endregion

        #region Properties
        public BookResponse Book { get; set; }
        public Image CurrentImage { get; set; }
        public List<string> BookStatuses { get; set; }
        public string BookStatus { get; set; }
        public bool CanEditStatus { get; set; }
        public bool CanDelete { get; set; }
        public ICommand BookStatusChanged => new Command<string>((flag) => RunOnceOnly(() => OnBookStatusChanged(flag)));
        public ICommand DeleteBook => new Command(() => RunOnceOnly(OnDeleteBookClicked));
        #endregion

        #region Constructor
        public ViewBookViewModel(IDialogService dialogService, IAuthenticationService authenticationService,
            IMyPostsDataService myPostsDataService, ISearchBooksDataService searchBooksDataService, IBooksService bookService)
            : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _authenticationService = authenticationService;
            _myPostsDataService = myPostsDataService;
            _searchBooksDataService = searchBooksDataService;
            _bookService = bookService;
            Title = "Book View";
            Init();
        }
        #endregion

        #region Public Methods
        public async Task SetBook(string parentPage, Guid postId, Guid bookId)
        {
            _parentPage = parentPage;
            _postId = postId;
            switch (_parentPage)
            {
                case "viewPost":
                    Book = _myPostsDataService.CurrentPost.Books.ToList().Find(b => b.Id == bookId);
                    CanEditStatus = true;
                    CanDelete = _myPostsDataService.CurrentPost.Books.Count > 1;
                    break;
                case "searchBook":
                    Book = _searchBooksDataService.Books.Find(b => b.Book.Id == bookId)?.Book;
                    CanEditStatus = CanDelete = false;
                    break;
            }
            if (Book is null)
            {
                await _dialogService.Alert("Invalid book id", "Invalid book", "Ok");
                await Shell.Current.GoToAsync("..");
            }

            BookStatus = Book.Status.ToString();
            OnPropertyChanged(nameof(CanEditStatus));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(Book));
            OnPropertyChanged(nameof(BookStatus));
        }
        #endregion

        #region Private Methods
        private void Init()
        {
            BookStatuses = Enum.GetNames(typeof(BookStatus)).ToList();
        }

        private async Task OnBookStatusChanged(string status)
        {
            switch (_parentPage)
            {
                case "viewPost":
                    var bookStatus = (BookStatus)Enum.Parse(typeof(BookStatus), status);
                    await _bookService.MarkBookStatus(_postId, Book.Id, bookStatus);
                    Book.Status = bookStatus;
                    var bookIndex = _myPostsDataService.CurrentPost.Books.IndexOf(Book);
                    _myPostsDataService.CurrentPost.Books[bookIndex] = Book;
                    return;
                default:
                    return;
            }
        }

        private async Task OnDeleteBookClicked()
        {
            await _bookService.DeleteBook(_postId, Book.Id);
            switch (_parentPage)
            {
                case "viewPost":
                    if (_myPostsDataService.CurrentPost.Books.Count > 1)
                        _myPostsDataService.CurrentPost.Books.Remove(Book);
                    break;
                default:
                    return;
            }
            await Shell.Current.GoToAsync($"..?postId={_postId}");
        }
        #endregion
    }
}
