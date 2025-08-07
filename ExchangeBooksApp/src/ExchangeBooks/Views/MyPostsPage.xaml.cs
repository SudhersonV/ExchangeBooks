using System;
using System.Collections.Generic;
using ExchangeBooks.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class MyPostsPage : ContentPage
    {
        private readonly MyPostsViewModel _myPostsViewModel;

        public MyPostsPage()
        {
            InitializeComponent();
            _myPostsViewModel = BindingContext as MyPostsViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _myPostsViewModel?.OnAppearing();
        }
    }
}
