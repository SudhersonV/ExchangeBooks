using System.ComponentModel;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            SearchBtn.IsEnabled = false;
            SearchText.Text = string.Empty;
            SearchText.PlaceholderColor = Color.Black;
            ((SearchViewModel)BindingContext)?.GetRecentBooks();
        }

        void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entry = ((Entry)sender);
            if (entry == null) return;
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                SearchBtn.IsEnabled = false;
                
            }
            else
            {
                SearchBtn.IsEnabled = true;
            }
        }
    }
}
