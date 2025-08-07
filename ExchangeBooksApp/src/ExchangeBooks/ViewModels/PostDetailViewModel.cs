using System;
using System.Collections.Generic;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class PostDetailViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IPostDataService _postDataService;
        private readonly IEventTracker _eventTracker;
        #endregion
        #region Properties
        public string Name { get; set; }
        public double Price { get; set; }
        #endregion

        public PostDetailViewModel(IDialogService dialogService, IPostDataService postDataService
            , IEventTracker eventTracker, IAuthenticationService authenticationService) : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _postDataService = postDataService;
            _eventTracker = eventTracker;
            Title = "Detail";
            Init();
        }

        #region Public Methods
        public async void OnNext()
        {
            _eventTracker.SendEvent("PostDetails", "OnNext", "Click");
            _postDataService.Post.Name = Name.Trim();
            _postDataService.Post.Price = Price;
            var paramDictionary = new Dictionary<string, string> {
                { "postName", Name } };
            _eventTracker.SendEvent("PostDetails", paramDictionary);
            await Shell.Current.GoToAsync("//post/books");
        }
        #endregion

        #region Private Methods
        private void Init()
        {
            _eventTracker.SetCurrentScreen("PostDetails", nameof(PostDetailViewModel));
            #if DEBUG
            Name = "Used books for sale";
            #endif
            _postDataService.ResetPost();
        }
        #endregion
    }
}
