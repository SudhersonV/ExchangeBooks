using System.Threading;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExchangeBooks.Core.Pages
{
    public partial class BasePopupPage : PopupPage
    {
        public static BasePopupPage CurrentPopup = null;

        protected Command closeCommand;
        protected static ManualResetEvent evt;

        public BasePopupPage()
        {
            closeCommand = new Command(HandleCloseAction);
            SetBinding(IsShowingProperty, new Binding("IsShowing", BindingMode.TwoWay));
            SetBinding(CloseWhenBackgroundIsClickedProperty, new Binding("BackgroundDismiss"));
        }

        public static readonly BindableProperty IsShowingProperty = BindableProperty.Create("IsShowing", typeof(bool), typeof(BasePopupPage), default(bool),
            BindingMode.TwoWay, propertyChanged: OnIsShowingChanged);

        public bool IsShowing
        {
            get { return (bool)GetValue(IsShowingProperty); }
            set { SetValue(IsShowingProperty, value); }
        }
        
        public static async Task PromptPopup(BasePopupPage page)
        {
            if (MainThread.IsMainThread)
            {
                await Task.Run(() =>
                {
                    AskAndBlockThread(page);
                });
            }
            else
            {
                AskAndBlockThread(page);
            }
        }

        public virtual async void HandleCloseAction(object obj)
        {
            evt.Set();
            await PopupNavigation.Instance.PopAsync();
        }

        protected override void OnAppearing()
        {
            IsShowing = true;
            base.OnAppearing();
        }

        protected override bool OnBackgroundClicked()
        {
            if (CloseWhenBackgroundIsClicked)
            {
                evt.Set();
                return base.OnBackgroundClicked();
            }
            return false;
        }

        private static void AskAndBlockThread(BasePopupPage page)
        {
            evt = new ManualResetEvent(false);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                CurrentPopup = page;
                await PopupNavigation.Instance.PushAsync(page);
            });

            evt.WaitOne();
            CurrentPopup = null;
        }


        private static void OnIsShowingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BasePopupPage page && newValue != null && newValue is bool isShowing)
            {
                if (!isShowing)
                    page.HandleCloseAction(null);
            }
        }
    }
}
