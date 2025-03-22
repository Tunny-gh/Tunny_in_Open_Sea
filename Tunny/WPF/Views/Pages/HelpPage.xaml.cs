using System;
using System.Windows.Controls;

using CefSharp;
using CefSharp.Wpf;

using Tunny.WPF.Common;

namespace Tunny.WPF.Views.Pages
{
    public partial class HelpPage : Page, IDisposable
    {
        private readonly Lazy<ChromiumWebBrowser> _browser;

        public HelpPage()
        {
            InitializeComponent();
            _browser = new Lazy<ChromiumWebBrowser>();
        }

        internal void OpenSite(HelpType? type)
        {
            if (!_browser.IsValueCreated)
            {
                HelpPageFrame.Content = _browser.Value;
            }
            switch (type)
            {
                case null:
                case HelpType.TunnyAbout:
                    _browser.Value.Address = "https://tunny-docs.deno.dev/";
                    break;
                case HelpType.TunnyDocument:
                    _browser.Value.Address = "https://tunny-docs.deno.dev/docs/getting-start";
                    break;
                case HelpType.OptunaSampler:
                    _browser.Value.Address = "https://optuna.readthedocs.io/en/stable/reference/samplers/index.html";
                    break;
                case HelpType.OptunaHub:
                    _browser.Value.Address = "https://hub.optuna.org/";
                    break;
                case HelpType.TunnyLicense:
                    _browser.Value.Address = "https://github.com/hrntsm/Tunny/blob/main/LICENSE";
                    break;
                case HelpType.PythonPackagesLicense:
                    _browser.Value.Address = "https://github.com/hrntsm/Tunny/blob/main/PYTHON_PACKAGE_LICENSES";
                    break;
                case HelpType.OtherLicense:
                    _browser.Value.Address = "https://github.com/hrntsm/Tunny/blob/main/THIRD_PARTY_LICENSES";
                    break;
                case HelpType.FAQ:
                    _browser.Value.Address = "https://tunny-docs.deno.dev/faqs/faq";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Dispose()
        {
            Cef.Shutdown();
            _browser?.Value.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
