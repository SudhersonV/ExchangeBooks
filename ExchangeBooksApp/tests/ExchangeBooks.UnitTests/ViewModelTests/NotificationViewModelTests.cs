using System;
using System.Collections.Generic;
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
    public class NotificationViewModelTests
    {
        #region Variables
        private readonly Mock<IDialogService> _dialogService = new Mock<IDialogService>();
        private readonly Mock<IMessagesService> _messageService = new Mock<IMessagesService>();
        private readonly Mock<IFcmUtility> _fcmUtility = new Mock<IFcmUtility>();
        private readonly Mock<IAuthenticationService> _authenticationService = new Mock<IAuthenticationService>();
        private readonly Mock<IMessagingCenterService> _messagingCenterService = new Mock<IMessagingCenterService>();
        private readonly NotificationViewModel _notificationViewModel;
        private List<BookSearchResponse> _recentBookResponse = new List<BookSearchResponse> {
            new BookSearchResponse{ PostId = Guid.Empty, PostName = "Physics", Book = new BookResponse { Class = BookClass.II, Condition = BookCondition.Good } } };
        private List<BookSearchResponse> _bookSearchResponse = new List<BookSearchResponse> {
            new BookSearchResponse{ PostId = Guid.Empty, PostName = "Physics", Book = new BookResponse { Class = BookClass.II, Condition = BookCondition.Good } } };
        private List<PushMessage> _pushMessages = new List<PushMessage>{ new PushMessage { Title="Sample notification", Content = "Book is avalable!" } };
        #endregion

        public NotificationViewModelTests()
        {
            _dialogService.Setup(m => m.Alert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _dialogService.Setup(m => m.Confirm(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _messageService.Setup(m => m.GetNotifications()).ReturnsAsync(_pushMessages);
            _authenticationService.Setup(m => m.GetUserEmail()).ReturnsAsync("sudherson.v@gmail.com"); 
            _authenticationService.Setup(m => m.IsUserAuthenticated()).ReturnsAsync(true);
            _messagingCenterService.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Action<object, object>>()));
            _messagingCenterService.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<object>()));
            _notificationViewModel = new NotificationViewModel(_messageService.Object, _dialogService.Object, _authenticationService.Object
                , _messagingCenterService.Object);
        }

        /// <summary>
        /// Check start up
        /// </summary>
        [Fact]
        public void NotificationViewModel_Start()
        {
            var vm = _notificationViewModel;

            Assert.NotNull(vm);
        }
    }
}
