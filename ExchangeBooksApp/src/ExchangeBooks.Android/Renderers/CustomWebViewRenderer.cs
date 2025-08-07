using System;
using System.Linq;
using Android.Content;
using ExchangeBooks.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static ExchangeBooks.Constants.Constants;

[assembly: ExportRenderer(typeof(WebView), typeof(CustomWebViewRenderer))]
namespace ExchangeBooks.Droid.Renderers
{
    public class CustomWebViewRenderer : WebViewRenderer
    {

        public CustomWebViewRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);
            // Setting the background as transparent
            //this.Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
            if (e.OldElement == null)
            {

            }

            if (e.NewElement != null)
            {
                Control.SetWebViewClient(new CustomFormsWebViewClient(this));
            }
        }

        internal class CustomFormsWebViewClient : FormsWebViewClient
        {
            CustomWebViewRenderer _renderer;

            public CustomFormsWebViewClient(CustomWebViewRenderer renderer) : base(renderer)
            {
                _renderer = renderer;
            }

            public override void OnReceivedSslError(Android.Webkit.WebView view, Android.Webkit.SslErrorHandler handler, Android.Net.Http.SslError error)
            {
                //Fixing the following error by intercepting the SSL errors and checking the URLs against a white listed domains
                //[ERROR:ssl_client_socket_impl.cc(946)] handshake failed; returned -1, SSL error code 1, net_error -202
                //If the url is from whitelisted domains, we allow else we won't.
                var whiteListedUrls = new[] { IdentityServer.Url };
                if (whiteListedUrls.Any(str => view.Url.StartsWith(str, StringComparison.OrdinalIgnoreCase)))
                    handler.Proceed();
                else
                    handler.Cancel();
            }

            public override void OnReceivedError(Android.Webkit.WebView view, Android.Webkit.IWebResourceRequest request, Android.Webkit.WebResourceError error)
            {
                base.OnReceivedError(view, request, error);
            }

            public override void OnPageFinished(Android.Webkit.WebView view, string url)
            {
                base.OnPageFinished(view, url);
            }

            public override void OnLoadResource(Android.Webkit.WebView view, string url)
            {
                base.OnLoadResource(view, url);
            }
        }
    }
}
