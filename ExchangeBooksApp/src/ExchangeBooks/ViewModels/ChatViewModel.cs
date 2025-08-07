using System;
using System.Collections.Generic;
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
    public class ChatViewModel : BaseViewModel
    {
        #region Variables
        private readonly IMessagesService _messageService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IDialogService _dialogService;
        private readonly IMessagingCenterService _messagingCenterService;
        private bool _lastMessageVisible = true;
        private Guid _topicId;
        #endregion

        #region Properties
        public string TextToSend { get; set; }
        public bool ShowScrollTap { get; set; } = false;
        public int PendingMessageCount { get; set; } = 0;
        public bool HasPendingMessage => PendingMessageCount > 0;
        public Queue<PushMessage> DelayedMessages { get; set; } = new Queue<PushMessage>();
        public ObservableCollection<PushMessage> Messages { get; set; } = new ObservableCollection<PushMessage>();
        public ICommand OnSendCommand => new Command(() => RunOnceOnly(() => SendMessage()));
        public ICommand OnScrollCommand => new Command(() => ScrollTap());
        #endregion

        #region Constructor
        public ChatViewModel(IAuthenticationService authenticationService, IMessagesService messageService
            , IDialogService dialogService, IMessagingCenterService messagingCenterService) : base(authenticationService, dialogService)
        {
            _authenticationService = authenticationService;
            _messageService = messageService;
            _dialogService = dialogService;
            _messagingCenterService = messagingCenterService;
            _messagingCenterService.Subscribe<Application, PushMessage>(Application.Current, Messaging.ChatMessage,
                (app, msg) => OnMessageReceived(msg));
            #region Mock data
            //Messages.Insert(0, new PushMessage() { Content = "Hi" });
            //Messages.Insert(0, new PushMessage() { Content = "How are you?", CreatedBy = "sudherson.v@gmail.com" });
            //Messages.Insert(0, new PushMessage() { Content = "What's new?" });
            //Messages.Insert(0, new PushMessage() { Content = "How is your family", CreatedBy = "sudherson.v@gmail.com" });
            //Messages.Insert(0, new PushMessage() { Content = "How is your dog?", CreatedBy = "sudherson.v@gmail.com" });
            //Messages.Insert(0, new PushMessage() { Content = "How is your cat?", CreatedBy = "sudherson.v@gmail.com" });
            //Messages.Insert(0, new PushMessage() { Content = "How is your sister?" });
            //Messages.Insert(0, new PushMessage() { Content = "When we are going to meet?" });
            //Messages.Insert(0, new PushMessage() { Content = "I want to buy a laptop" });
            //Messages.Insert(0, new PushMessage() { Content = "Where I can find a good one?" });
            //Messages.Insert(0, new PushMessage() { Content = "Also I'm testing this chat" });
            //Messages.Insert(0, new PushMessage() { Content = "Oh My God!" });
            //Messages.Insert(0, new PushMessage() { Content = " No Problem", CreatedBy = "sudherson.v@gmail.com" });
            //Messages.Insert(0, new PushMessage() { Content = "Hugs and Kisses", CreatedBy = "sudherson.v@gmail.com" });
            //Messages.Insert(0, new PushMessage() { Content = "When we are going to meet?" });
            //Messages.Insert(0, new PushMessage() { Content = "I want to buy a laptop" });
            //Messages.Insert(0, new PushMessage() { Content = "Where I can find a good one?" });
            //Messages.Insert(0, new PushMessage() { Content = "Also I'm testing this chat" });
            //Messages.Insert(0, new PushMessage() { Content = "Oh My God!" });
            //Messages.Insert(0, new PushMessage() { Content = " No Problem" });
            //Messages.Insert(0, new PushMessage() { Content = "Hugs and Kisses" });

            //int count = 1;
            ////Code to simulate reveing a new message procces
            //Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            //{
            //    EnqueueMessage(new PushMessage() { Content = $"New message test-{count++}", CreatedBy = "Mario" });
            //    return true;
            //});
            #endregion
        }
        #endregion

        #region Public Methods

        public void OnAppearing()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                RunOnceOnly(() => GetMessages());
                return false;
            });
        }

        public void SetTopicId(Guid topicId)
        {
            _topicId = topicId;
        }
        public void SetPostName(string postName)
        {
            Title = postName;
        }
        public async Task GetMessages()
        {
            Messages.Clear();
            _dialogService.ShowLoading();
            var pushMessages = await _messageService.GetMessages(_topicId);
            pushMessages.ForEach(cm =>
            {
                EnqueueMessage(new PushMessage { Content = cm.Content, CreatedBy = cm.CreatedBy });
            });
            _dialogService.HideLoading();
        }
        public async Task SendMessage()
        {
            if (string.IsNullOrEmpty(TextToSend)) return;
            await _messageService.SendMessage(_topicId, TextToSend);
            EnqueueMessage(new PushMessage { Content = TextToSend, CreatedBy = await _authenticationService.GetUserEmail() });
            TextToSend = string.Empty;
            OnPropertyChanged(nameof(TextToSend));
        }
        public void ScrollTap()
        {
            while (DelayedMessages.Any())
            {
                InsertMessage(DelayedMessages.Dequeue());
            }
            ShowScrollTap = false;
            _lastMessageVisible = true;
            PendingMessageCount = 0;
            RefreshScrollBtn();
        }
        public void OnMessageAppearing(ItemVisibilityEventArgs message)
        {
            var idx = message.ItemIndex;
            if (idx <= Messaging.ScrollThreshold)
            {
                while (DelayedMessages.Count > 0)
                {
                    InsertMessage(DelayedMessages.Dequeue());
                }
                ShowScrollTap = false;
                _lastMessageVisible = true;
                PendingMessageCount = 0;
                RefreshScrollBtn();
            }
        }
        public void OnMessageDisappearing(ItemVisibilityEventArgs message)
        {
            var idx = message.ItemIndex;
            if (idx <= Messaging.ScrollThreshold)
            {
                ShowScrollTap = true;
                _lastMessageVisible = false;
                RefreshScrollBtn();
            }
        }
        public void OnDisappearing()
        {
            _messagingCenterService.Send(this, Messaging.PendingMessage, new PendingMessage { TopicId = _topicId, Count = PendingMessageCount });
        }
        #endregion

        #region Private Methods
        private void OnMessageReceived(PushMessage message)
        {
            if (_topicId == message.TopicId)
            {
                EnqueueMessage(message);
            }
        }
        private void EnqueueMessage(PushMessage message)
        {
            if (_lastMessageVisible)
            {
                InsertMessage(message);
            }
            else
            {
                DelayedMessages.Enqueue(message);
                PendingMessageCount++;
                RefreshScrollBtn();
            }
        }
        private void InsertMessage(PushMessage message)
        {
            Messages.Insert(0, message);
            Console.WriteLine($"ThreadId: {Task.CurrentId}, message: {message.Content}");
            OnPropertyChanged(nameof(Messages));
        }
        private void RefreshScrollBtn()
        {
            OnPropertyChanged(nameof(ShowScrollTap));
            OnPropertyChanged(nameof(PendingMessageCount));
            OnPropertyChanged(nameof(HasPendingMessage));
        }
        #endregion
    }
}
