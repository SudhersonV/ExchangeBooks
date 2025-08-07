using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ExchangeBooks.Core.ViewModels;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Interfaces.Http;
using ExchangeBooks.Models;
using Xamarin.Forms;

namespace ExchangeBooks.ViewModels
{
    public class FilterViewModel : BaseViewModel
    {   

        #region Private Variables
        private readonly IMessagesService _messageService;
        private readonly IDialogService _dialogService;
        #endregion

        #region Properties
        public List<Topic> Topics { get; set; }
        public ICommand DeleteCommand => new Command<Topic>((topic) => RunOnceOnly(() => OnDeleteCommand(topic)));
        public bool DataFound { get; set; } = true;
        #endregion

        public FilterViewModel(IMessagesService messageService, IAuthenticationService authenticationService
            , IDialogService dialogService) : base(authenticationService, dialogService)
        {
            _messageService = messageService;
            _dialogService = dialogService;
            Title = "Filters";
        }

        public async Task OnAppearing()
        {
            await CheckAuthenticate(async () => await GetFilters());
        }

        public async Task GetFilters()
        {
            _dialogService.ShowLoading();
            Topics = await _messageService.UserTopics(Enums.TopicType.Notify);

            if (Topics.Any())
                DataFound = true;
            else
                DataFound = false;
            OnPropertyChanged(nameof(DataFound));

            _dialogService.HideLoading();
            OnPropertyChanged(nameof(Topics));
        }

        private async Task OnDeleteCommand(Topic topic)
        {
            await _messageService.UnsubscribeTopic(topic.Id);
            await GetFilters();
        }
    }
}
