using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Enums;
using ExchangeBooks.Interfaces.Data;
using ExchangeBooks.Interfaces.Framework;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class BookDetailViewModel : BaseViewModel
    {
        #region Private Variables
        private readonly IDialogService _dialogService;
        private readonly IPostDataService _postDataService;
        private readonly IEventTracker _eventTracker;
        #endregion
        #region Properties
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string BookCondition { get; set; }
        public string BookClass { get; set; }

        public List<string> BookConditions { get; set; }
        public List<string> BookClasses { get; set; }
        public List<string> Boards { get; set; }
        #endregion

        #region Constructor
        public BookDetailViewModel(IDialogService dialogService, IPostDataService postDataService
            , IEventTracker eventTracker, IAuthenticationService authenticationService) : base(authenticationService, dialogService)
        {
            _dialogService = dialogService;
            _postDataService = postDataService;
            _eventTracker = eventTracker;
            Title = "Detail";
            Init();
        }
        #endregion

        #region Public Methods
        public async void OnNext()
        {
            _eventTracker.SendEvent("BookDetails", "OnNext", "Click");
            _postDataService.CurrentBook.Name = Name.Trim();
            _postDataService.CurrentBook.Description = Description;
            _postDataService.CurrentBook.Class = (BookClass)Enum.Parse(typeof(BookClass), BookClass);
            _postDataService.CurrentBook.Condition = (BookCondition)Enum.Parse(typeof(BookCondition), BookCondition);
            _postDataService.CurrentBook.Price = Price;
            var paramDictionary = new Dictionary<string, string> {
                { "bookName", Name },
                { "bookClass", BookClass },
                { "bookCondition", BookCondition } };
            _eventTracker.SendEvent("BookDetails", paramDictionary);
            await Shell.Current.GoToAsync("//post/images");
        }
        #endregion

        #region Private Methods
        private void Init()
        {
            _eventTracker.SetCurrentScreen("BookDetails", nameof(BookDetailViewModel));
            BookConditions = Enum.GetNames(typeof(BookCondition)).ToList();
            BookClasses = Enum.GetNames(typeof(BookClass)).ToList();
            Boards = Enum.GetNames(typeof(BookBoard)).ToList();
            #if DEBUG
            Name = "Yaazhini Book";
            BookClass = Enums.BookClass.III.ToString();
            BookCondition = Enums.BookCondition.Good.ToString();
            #endif    
        }
        #endregion
    }
}
