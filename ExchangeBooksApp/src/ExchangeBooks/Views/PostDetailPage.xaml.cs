using System;
using System.ComponentModel;
using Acr.UserDialogs;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class PostDetailPage : ContentPage
    {
        private readonly PostDetailViewModel _viewModel;

        public PostDetailPage()
        {
            InitializeComponent();
            _viewModel = (PostDetailViewModel)BindingContext;
        }

        void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entry = ((Entry)sender);
            if (entry == null) return;
            if (!string.IsNullOrWhiteSpace(entry.Text))
                entry.PlaceholderColor = Color.Black;
        }

        public void OnNextButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text) || Name.Text.Trim().Length < 5)
            {
                Name.PlaceholderColor = Color.Maroon;
                UserDialogs.Instance.AlertAsync("Title min length 5", "Input error", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                Price.Text = "0";
            }
            _viewModel?.OnNext();
        }
    }
}
