using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models;
using ExchangeBooks.Models.Response;
using ExchangeBooks.ViewModels;
using Moq;
using Xunit;

namespace ExchangeBooks.UnitTests.ViewModelTests
{
    public class SearchViewModelTests
    {
        #region Variables
        private readonly Mock<IDialogService> _dialogService = new Mock<IDialogService>();
        private readonly Mock<IBooksService> _bookService = new Mock<IBooksService>();
        private readonly Mock<IEventTracker> _eventTracker = new Mock<IEventTracker>();
        private readonly Mock<IMessagesService> _messageService = new Mock<IMessagesService>();
        private readonly Mock<IFcmUtility> _fcmUtility = new Mock<IFcmUtility>();
        private readonly Mock<IAuthenticationService> _authenticationService = new Mock<IAuthenticationService>();
        private readonly SearchViewModel _searchViewModel;
        private List<BookSearchResponse> _recentBookResponse = new List<BookSearchResponse> {
            new BookSearchResponse{ PostId = Guid.Empty, PostName = "Physics", CreatedBy = "amala.rajan1987@gmail.com" ,Book = new BookResponse { Class = BookClass.II, Condition = BookCondition.Good } } };
        private List<BookSearchResponse> _bookSearchResponse = new List<BookSearchResponse> {
            new BookSearchResponse{ PostId = Guid.Empty, PostName = "Physics", CreatedBy = "sudherson.v@gmail.com", Book = new BookResponse { Class = BookClass.II, Condition = BookCondition.Good } } };
        private Topic _topic = new Topic();
        #endregion

        public SearchViewModelTests()
        {
            _dialogService.Setup(m => m.Alert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _dialogService.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _bookService.Setup(m => m.GetRecentBooks(It.IsAny<int>())).ReturnsAsync(_recentBookResponse);
            _bookService.Setup(m => m.SearchBooks(It.IsAny<string[]>())).ReturnsAsync(_bookSearchResponse);
            _messageService.Setup(m => m.SubscribeToTopic(It.IsAny<TopicType>(), It.IsAny<string[]>())).ReturnsAsync(_topic);
            _authenticationService.Setup(m => m.GetUserEmail()).ReturnsAsync("sudherson.v@gmail.com");
            _authenticationService.Setup(m => m.GetFcmToken()).ReturnsAsync("cEtus50sIY8:APA91bHnNIzNJXgdx7EygcrASeRxtxyRF-");
            _searchViewModel = new SearchViewModel(_dialogService.Object, _bookService.Object, _eventTracker.Object, _messageService.Object, _fcmUtility.Object, _authenticationService.Object);
            _searchViewModel.SearchText = "physics II";
        }

        /// <summary>
        /// Search Books when searchText is empty
        /// </summary>
        [Fact]
        public async Task SearchBooks_When_SearchText_Empty()
        {
            _searchViewModel.SearchText = string.Empty;

            await _searchViewModel.SearchBooks();

            Assert.Empty(_searchViewModel.BooksResponse);
        }

        /// <summary>
        /// Search Books returns books
        /// </summary>
        [Fact]
        public async Task SearchBooks_Returns_Books()
        {
            await _searchViewModel.SearchBooks();

            Assert.NotEmpty(_searchViewModel.BooksResponse);
        }
    }
}
