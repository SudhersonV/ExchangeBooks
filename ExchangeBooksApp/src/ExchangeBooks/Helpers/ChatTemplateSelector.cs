using ExchangeBooks.Bootstrap;
using ExchangeBooks.Interfaces.Framework;
using ExchangeBooks.Models;
using ExchangeBooks.Views.Chat;
using Xamarin.Forms;

namespace ExchangeBooks.Helpers
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _incomingDataTemplate;
        private readonly DataTemplate _outgoingDataTemplate;
        private readonly IAuthenticationService _authenticationService;

        public ChatTemplateSelector()
        {   
            _incomingDataTemplate = new DataTemplate(typeof(IncomingViewCell));
            _outgoingDataTemplate = new DataTemplate(typeof(OutgoingViewCell));
            _authenticationService = AppContainer.Resolve<IAuthenticationService>();
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as PushMessage;
            return messageVm == null ? null :
                messageVm.CreatedBy == _authenticationService.GetUserEmail().GetAwaiter().GetResult() ? _outgoingDataTemplate : _incomingDataTemplate;
        }
    }
}
