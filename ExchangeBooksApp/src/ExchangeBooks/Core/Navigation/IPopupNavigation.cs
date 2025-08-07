using System.Threading.Tasks;
using ExchangeBooks.Core.Navigation;
using ExchangeBooks.Core.Pages;

//[assembly: Xamarin.Forms.Dependency(typeof(PopupNavigation))]
namespace ExchangeBooks.Core.Navigation
{
    public interface IPopupNavigation
    {
        bool IsAnyPopupShowing { get; set; }
        Task PromptPopup(BasePopupPage page);
    }

    public class PopupNavigation : IPopupNavigation
    {
        public bool IsAnyPopupShowing { get; set; }

        public async Task PromptPopup(BasePopupPage page)
        {
            await BasePopupPage.PromptPopup(page);
        }
    }
}
