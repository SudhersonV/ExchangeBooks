using System;
using System.ComponentModel;
using Acr.UserDialogs;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class BookDetailPage : ContentPage
    {
        private readonly BookDetailViewModel _viewModel;

        public BookDetailPage()
        {
            InitializeComponent();
            _viewModel = (BookDetailViewModel)BindingContext;
        }

        void Picker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var picker = ((Picker)sender);
            if (picker == null) return;
            if (picker.SelectedIndex >= 0)
                picker.TitleColor = Color.Black;
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
                UserDialogs.Instance.AlertAsync("Name min length 5", "Input error", "Ok");
                return;
            }
            if (Condition.SelectedIndex < 0)
            {
                Condition.TitleColor = Color.Maroon;
                UserDialogs.Instance.AlertAsync("Condition is required", "Input error", "Ok");
                return;
            }
            if (Class.SelectedIndex < 0)
            {
                Class.TitleColor = Color.Maroon;
                UserDialogs.Instance.AlertAsync("Class is required", "Input error", "Ok");
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
