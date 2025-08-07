using System;
using System.Collections.Generic;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class NotificationPage : ContentPage
    {
        private readonly NotificationViewModel _notificationViewModel;

        public NotificationPage()
        {
            InitializeComponent();
            _notificationViewModel = BindingContext as NotificationViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _notificationViewModel?.OnAppearing();
        }
    }
}
