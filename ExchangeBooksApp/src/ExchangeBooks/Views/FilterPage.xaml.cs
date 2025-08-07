using System.ComponentModel;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class FilterPage : ContentPage
    {
        public FilterPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((FilterViewModel)BindingContext)?.OnAppearing();
        }

        void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {   
        }
    }
}
