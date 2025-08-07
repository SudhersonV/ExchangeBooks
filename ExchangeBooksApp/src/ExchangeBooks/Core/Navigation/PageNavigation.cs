using System;
using System.Threading.Tasks;
using ExchangeBooks.Bootstrap;
using ExchangeBooks.Core.Navigation;
using ExchangeBooks.Core.Pages;
using ExchangeBooks.Core.ViewModels;
using Xamarin.Forms;

namespace ExchangeBooks.Navigation
{
    public static class PageNavigation
    {

        public static async Task<R> DisplayPopup<T, R>(T pg = null) where T : BasePopupPage
        {
            var popupImplementor = AppContainer.Resolve<IPopupNavigation>();

            if (popupImplementor.IsAnyPopupShowing)
                return default(R);

            R vmResult = default(R);
            popupImplementor.IsAnyPopupShowing = true;
            try
            {
                var page = pg ?? AppContainer.Resolve<T>() as BasePopupPage;
                var vm = page.BindingContext as PopupViewModel<R>;

                await popupImplementor.PromptPopup(page);
                vmResult = vm.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                popupImplementor.IsAnyPopupShowing = false;
            }

            return vmResult;
        }
    }
}
