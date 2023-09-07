using CefSharp.Wpf;
using CefSharp;
using DEClientInterface.Objects;
using DEClientInterface.UIControls;
using ExcavatorSharp.Common;
using ExcavatorSharp.Crawler;
using ExcavatorSharp.Excavator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ExcavatorSharp.CEF;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEObserveAnyWebsitePageWindow.xaml
    /// </summary>
    public partial class DEObserveAnyWebsitePageWindow : Window
    {
        private Mutex ProxyMutex = new Mutex();

        private DataExcavatorTask ExcavatorTaskLink { get; set; }

        private DEProjectTestingUrlContainer ProjectTestURLContainer { get; set; }

        public DEObserveAnyWebsitePageWindow(DataExcavatorTask ExcavatorTaskLink, DEProjectTestingUrlContainer ProjectTestURLContainer)
        {
            this.ExcavatorTaskLink = ExcavatorTaskLink;
            this.ProjectTestURLContainer = ProjectTestURLContainer;
            InitializeComponent();
            ChromiumWebBrowserInstance.IsBrowserInitializedChanged += ChromiumWebBrowserInstance_IsBrowserInitializedChanged;
            ChromiumWebBrowserInstance.LoadingStateChanged += ChromiumWebBrowserInstance_LoadingStateChanged;
            ChromiumWebBrowserInstance.ConsoleMessage += ChromiumWebBrowserInstance_ConsoleMessage;
            base.Title = base.Title.Replace("PROJECTNAME", this.ExcavatorTaskLink.TaskName);
            ExamplePageUrl.Text = ProjectTestURLContainer.LastTestingURL;
            base.Loaded += DEObserveAnyWebsitePageWindow_Loaded;
        }

        private void DEObserveAnyWebsitePageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ExamplePageUrl.SelectAll();
            ExamplePageUrl.Focus();
        }

        private void ChromiumWebBrowserInstance_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ExcavatorTaskLink.GetCrawlingServerPropertiesCopy().HTTPWebRequestProxiesList != null && ExcavatorTaskLink.GetCrawlingServerPropertiesCopy().HTTPWebRequestProxiesList.Count > 0)
            {
                ProxyAccessor proxyAccessor = new ProxyAccessor(ExcavatorTaskLink.GetCrawlingServerPropertiesCopy(), ProxyMutex);
                WebProxy webProxy = proxyAccessor.PeekProxy();
                string login = ((webProxy.Credentials != null) ? (webProxy.Credentials as NetworkCredential).UserName : string.Empty);
                string password = ((webProxy.Credentials != null) ? (webProxy.Credentials as NetworkCredential).Password : string.Empty);
                CEFSetProxy(webProxy.Address.AbsoluteUri, webProxy.Address.Port, login, password);
            }
        }

        private void ChromiumWebBrowserInstance_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            base.Dispatcher.Invoke(delegate
            {
                ListBoxItem newItem = new ListBoxItem
                {
                    Content = $"{DateTime.Now.ToString()} | File #{e.Source}, Line #{e.Line.ToString()} | {e.Message}"
                };
                TestProjectSettings_BrowserLogs.Items.Add(newItem);
            });
        }

        private void ChromiumWebBrowserInstance_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
        }

        private void TryToOpenPageButton_Click(object sender, RoutedEventArgs e)
        {
            TryLoadPage();
        }

        private void TryLoadPage()
        {
            string ExamplePageURLText = ExamplePageUrl.Text;
            AsyncHelpers.RunSync(() => LoadPageWithGETAsync(ExamplePageURLText));
            GetSourceCodes();
        }

        public void GetSourceCodes()
        {
            string text = AsyncHelpers.RunSync(() => ChromiumWebBrowserInstance.GetSourceAsync());
            PageSourceTextBox.Text = text;
        }

        private Task LoadPageWithGETAsync(string address)
        {
            TaskCompletionSource<bool> TaskSource = new TaskCompletionSource<bool>(TaskCreationOptions.None);
            EventHandler<LoadingStateChangedEventArgs> CEFPageLoadedHandler = null;
            CEFPageLoadedHandler = delegate (object sender, LoadingStateChangedEventArgs args)
            {
                if (!args.IsLoading)
                {
                    ChromiumWebBrowserInstance.LoadingStateChanged -= CEFPageLoadedHandler;
                    TaskSource.TrySetResult(result: true);
                }
            };
            ChromiumWebBrowserInstance.LoadingStateChanged += CEFPageLoadedHandler;
            ChromiumWebBrowserInstance.Load(address);
            return TaskSource.Task;
        }

        private void TryToReloadPageBytton_Click(object sender, RoutedEventArgs e)
        {
            ChromiumWebBrowserInstance.GetBrowser().Reload();
        }

        private void CEFSetProxy(string Address, int Port, string Login, string Password)
        {
            AsyncHelpers.RunSync(() => Cef.UIThreadTaskFactory.StartNew(delegate
            {
                if (Login != string.Empty && Password != string.Empty)
                {
                    ChromiumWebBrowserInstance.RequestHandler = new CEFProxyAuthRequestHandler(Login, Password);
                }
                IRequestContext requestContext = ChromiumWebBrowserInstance.GetBrowser().GetHost().RequestContext;
                string error;
                bool flag = requestContext.SetPreference("proxy", new Dictionary<string, object>(2)
                {
                    ["mode"] = "fixed_servers",
                    ["server"] = Address.Replace("http://", string.Empty).Replace("https://", string.Empty).Trim(' ', '/')
                }, out error);
            }));
        }

        private void ExamplePageUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TryLoadPage();
            }
        }
    }
}
