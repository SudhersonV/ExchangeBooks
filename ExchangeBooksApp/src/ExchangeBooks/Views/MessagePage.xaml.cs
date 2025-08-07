using System;
using System.Collections.Generic;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class MessagePage : ContentPage
    {
        private readonly MessageViewModel _messageViewModel;

        public MessagePage()
        {
            InitializeComponent();
            _messageViewModel = BindingContext as MessageViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _messageViewModel?.OnAppearing();
        }
    }
}
