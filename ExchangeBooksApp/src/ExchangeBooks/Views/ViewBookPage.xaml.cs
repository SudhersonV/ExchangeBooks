using System;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    [QueryProperty("ParentPage", "parentPage")]
    [QueryProperty("PostId", "postId")]
    [QueryProperty("BookId", "bookId")]
    public partial class ViewBookPage : ContentPage
    {
        private ViewBookViewModel _viewBookViewModel;
        private string _parentPage = string.Empty;
        private Guid _postId;
        public ViewBookPage()
        {
            InitializeComponent();
            _viewBookViewModel = BindingContext as ViewBookViewModel;
        }

        public string BookId
        {
            set
            {
                SetBook(Guid.Parse(Uri.UnescapeDataString(value)));
            }
        }

        public string PostId
        {
            set
            {
                _postId = Guid.Parse(Uri.UnescapeDataString(value));
            }
        }

        public string ParentPage
        {
            set
            {
                _parentPage = Uri.UnescapeDataString(value);
            }
        }

        public void SetBook(Guid bookId)
        {
            _viewBookViewModel?.SetBook(_parentPage, _postId, bookId);
        }
    }
}
