using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models;
using Xamarin.Forms;
using static ExchangeBooks.Constants.Constants;

namespace ExchangeBooks.ViewModels
{
    public class NotificationViewModel : BaseViewModel
    {
        #region Variables
        private readonly IMessagesService _messageService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDialogService _dialogService;
        private readonly IMessagingCenterService _messagingCenterService;
        #endregion

        #region Properties
        public ObservableCollection<PushMessage> Notifications { get; set; }
        public ICommand ViewCommand => new Command<PushMessage>((notification) => RunOnceOnly(() => OnViewCommand(notification)));
        #endregion

        #region Constructor
        public NotificationViewModel(IMessagesService messageService
            , IDialogService dialogService, IAuthenticationService authenticationService
            , IMessagingCenterService messagingCenterService) : base(authenticationService, dialogService)
        {
            _authenticationService = authenticationService;
            _messageService = messageService;
            _dialogService = dialogService;
            _messagingCenterService = messagingCenterService;
            _messagingCenterService.Subscribe<Application, PushMessage>(Application.Current, Messaging.PushMessage,
                (app, msg) => OnMessageReceived(msg));
        }
        #endregion

        #region Public Methods
        public async Task OnAppearing()
        {
            await CheckAuthenticate(async () => await GetTopics());
        }

        public async Task GetTopics()
        {
            if (Notifications != null) return;
            _dialogService.ShowLoading();
            var topics = await _messageService.GetNotifications();
            Notifications = new ObservableCollection<PushMessage>();
            topics.ForEach(t =>
            {
                Notifications.Add(t);
            });
            _dialogService.HideLoading();
            OnPropertyChanged(nameof(Notifications));
        }

        public async Task OnViewCommand(PushMessage notification)
        {
            await _dialogService.Alert(notification.Content, notification.Title, "Ok");
        }

        private void OnMessageReceived(PushMessage message)
        {
            if (Notifications is null) return;
            Notifications.Add(message);
            OnPropertyChanged(nameof(Notifications));
        }
        #endregion
    }
}
