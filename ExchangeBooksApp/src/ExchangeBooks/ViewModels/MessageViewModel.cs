using System.Collections.ObjectModel;
using System.Linq;
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
    public class MessageViewModel : BaseViewModel
    {
        #region Variables
        private readonly IMessagesService _messageService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDialogService _dialogService;
        private readonly IMessagingCenterService _messagingCenterService;
        #endregion

        #region Properties
        public ObservableCollection<Topic> ChatTopics { get; set; }
        public ICommand ChatCommand => new Command<Topic>((topic) => RunOnceOnly(() => OnChatCommand(topic)));
        #endregion

        #region Constructor
        public MessageViewModel(IAuthenticationService authenticationService, IMessagesService messageService
            , IDialogService dialogService, IMessagingCenterService messagingCenterService) : base(authenticationService, dialogService)
        {
            _authenticationService = authenticationService;
            _messageService = messageService;
            _dialogService = dialogService;
            _messagingCenterService = messagingCenterService;
            _messagingCenterService.Subscribe<Xamarin.Forms.Application, PushMessage>(Application.Current, Messaging.ChatMessage,
                async (app, msg) => { await OnMessageReceived(msg); });
            _messagingCenterService.Subscribe<ChatViewModel, PendingMessage>(this, Messaging.PendingMessage,
                (vm, pending) => OnPendingChatMessage(pending));
        }
        #endregion

        #region Public Methods
        public async Task OnAppearing()
        {
            await CheckAuthenticate(async () => await GetTopics());
        }

        public async Task GetTopics()
        {
            if (ChatTopics != null) return;
            _dialogService.ShowLoading();
            var topics = await _messageService.UserTopics(Enums.TopicType.Chat);
            ChatTopics = new ObservableCollection<Topic>();
            topics.ForEach(t =>
            {
                ChatTopics.Add(t);
            });
            _dialogService.HideLoading();
            OnPropertyChanged(nameof(ChatTopics));
        }

        public async Task OnChatCommand(Topic topic)
        {
            if (!topic.PostNames.Any())
                return;

            await Shell.Current.GoToAsync($"chat?topicId={topic.Id}&postName={topic.PostNames.FirstOrDefault()}");
            var chatTopic = ChatTopics.SingleOrDefault(c => c.Id == topic.Id);
            chatTopic.PendingMessageCount = 0;
            var index = ChatTopics.IndexOf(chatTopic);
            ChatTopics[index] = chatTopic;
            OnPropertyChanged(nameof(ChatTopics));
        }

        private async Task OnMessageReceived(PushMessage message)
        {
            if (ChatTopics is null) return;

            var chatTopic = ChatTopics.SingleOrDefault(c => c.Id == message.TopicId);
            if (chatTopic is null)
            {
                var topic = await _messageService.GetTopic(message.TopicId);
                topic.PendingMessageCount++;
                ChatTopics.Add(topic);
            }
            else
            {
                chatTopic.PendingMessageCount++;
                var index = ChatTopics.IndexOf(chatTopic);
                ChatTopics[index] = chatTopic;
            }

            OnPropertyChanged(nameof(ChatTopics));
        }

        private void OnPendingChatMessage(PendingMessage pendingMessage)
        {
            var chatTopic = ChatTopics.SingleOrDefault(c => c.Id == pendingMessage.TopicId);

            if (chatTopic is null) return;

            var index = ChatTopics.IndexOf(chatTopic);
            chatTopic.PendingMessageCount = pendingMessage.Count;
            ChatTopics[index] = chatTopic;

            OnPropertyChanged(nameof(ChatTopics));
        }
        #endregion
    }
}
