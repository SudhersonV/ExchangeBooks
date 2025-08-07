using System;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;
using System.Linq;

namespace ExchangeBooks.Views
{
    [QueryProperty("TopicId", "topicId")]
    [QueryProperty("PostName", "postName")]
    public partial class ChatPage : ContentPage
    {
        private ChatViewModel _chatViewModel;

        public string TopicId
        {
            set
            {
                var topicId = Uri.UnescapeDataString(value);
                _chatViewModel?.SetTopicId(new Guid(topicId));
            }
        }

        public string PostName
        {
            set
            {
                var postName = Uri.UnescapeDataString(value);
                _chatViewModel?.SetPostName(postName);
            }
        }

        public ChatPage()
        {
            InitializeComponent();
            _chatViewModel = BindingContext as ChatViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _chatViewModel?.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _chatViewModel?.OnDisappearing();
        }

        public void ScrollTap(object sender, System.EventArgs e)
        {
                ScrollToFirst();
        }

        public void OnListTapped(object sender, ItemTappedEventArgs e)
        {
            chatInput.UnFocusEntry();
        }

        public void OnMessageAppearing(object sender, ItemVisibilityEventArgs args)
        {
            _chatViewModel?.OnMessageAppearing(args);
        }

        public void OnMessageDisappearing(object sender, ItemVisibilityEventArgs args)
        {
            _chatViewModel?.OnMessageDisappearing(args);
        }

        private void ScrollToFirst()
        {
            try
            {
                if (ChatList.ItemsSource != null && ChatList.ItemsSource.Cast<object>().Any())
                {
                    var msg = ChatList.ItemsSource.Cast<object>().FirstOrDefault();
                    ChatList.ScrollTo(msg, ScrollToPosition.Start, true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void ScrollToLast()
        {
            try
            {
                if (ChatList.ItemsSource != null && ChatList.ItemsSource.Cast<object>().Any())
                {
                    var msg = ChatList.ItemsSource.Cast<object>().LastOrDefault();
                    ChatList.ScrollTo(msg, ScrollToPosition.End, false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}
