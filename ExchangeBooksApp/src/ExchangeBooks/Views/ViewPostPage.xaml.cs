using System;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    [QueryProperty("PostId", "postId")]
    public partial class ViewPostPage : ContentPage
    {
        private ViewPostViewModel _viewPostViewModel;

        public string PostId
        {
            set
            {
                SetPost(Guid.Parse(Uri.UnescapeDataString(value)));
            }
        }

        public ViewPostPage()
        {
            InitializeComponent();
            _viewPostViewModel = BindingContext as ViewPostViewModel;
        }

        public void SetPost(Guid postId)
        {
            _viewPostViewModel?.SetPost(postId);
        }
        
    }
}
