using ExchangeBooks.Core.Pages;
using Xamarin.Forms;

namespace ExchangeBooks.Views
{
    public partial class EulaPage : BasePopupPage
    {
        public EulaPage()
        {
            InitializeComponent();
            eulaWebView.SetBinding(WebView.SourceProperty, new Binding("SourceUrl"));

            btnAccept.IsEnabled = eulaChkBox.IsChecked = eulaChkBox.IsEnabled = false;
            eulaWebView.Navigating += EulaWebView_Navigating;
            eulaWebView.Navigated += EulaWebView_Navigated;
        }

        private void EulaWebView_Navigating(object sender, Xamarin.Forms.WebNavigatingEventArgs e)
        {
            webViewLoading.IsVisible = true;
        }

        private void EulaWebView_Navigated(object sender, Xamarin.Forms.WebNavigatedEventArgs e)
        {
            webViewLoading.IsVisible = false;
            eulaChkBox.IsVisible = true;
            eulaChkBox.IsEnabled = true;

            eulaWebView.Navigating -= EulaWebView_Navigating;
            eulaWebView.Navigated -= EulaWebView_Navigated;
        }

        public void eulaCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            btnAccept.IsEnabled = e.Value;
        }
    }
}
