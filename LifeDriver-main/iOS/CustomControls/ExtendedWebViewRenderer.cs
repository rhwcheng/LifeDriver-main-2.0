using StyexFleetManagement.CustomControls;
using StyexFleetManagement.iOS;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(ExtendedWebViewRenderer))]

namespace StyexFleetManagement.iOS
{
    public class ExtendedWebViewRenderer : WkWebViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            NavigationDelegate = new ExtendedUiWebViewDelegate(this);
        }
    }

    public class ExtendedUiWebViewDelegate : WKNavigationDelegate
    {
        readonly ExtendedWebViewRenderer webViewRenderer;

        public ExtendedUiWebViewDelegate(ExtendedWebViewRenderer _webViewRenderer = null)
        {
            webViewRenderer = _webViewRenderer ?? new ExtendedWebViewRenderer();
        }

        public override async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (!(webViewRenderer.Element is ExtendedWebView wv)) return;

            await System.Threading.Tasks.Task.Delay(100); // wait here till content is rendered
            wv.HeightRequest = (double) webView.ScrollView.ContentSize.Height;
        }
    }
}