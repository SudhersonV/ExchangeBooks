using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class ProfilePage : ContentPage
    {
        private ProfileViewModel _profileViewModel;

        public ProfilePage()
        {
            InitializeComponent();
            _profileViewModel = BindingContext as ProfileViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _profileViewModel?.OnAppearing();
        }
    }
}
