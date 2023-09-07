using CefSharp.Wpf;
using DEClientInterface.Controls;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEPageLinksObserving.xaml
    /// </summary>
    public partial class DEPageLinksObserving : Window
    {
        public List<PageLink> ObservedLinks = new List<PageLink>();

        private ChromiumWebBrowser ChromiumBrowserLoaded = null;

        private DEProjectCube ProjectCubeLink { get; set; }

        public DEPageLinksObserving(DEProjectCube ProjectCubeLink)
        {
            InitializeComponent();
            this.ProjectCubeLink = ProjectCubeLink;
            base.Title = base.Title.Replace("PROJECTNAME", ProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
            base.Closed += DEPageLinksObserving_Closed;
            base.Loaded += DEPageLinksObserving_Loaded;
        }

        private void DEPageLinksObserving_Loaded(object sender, RoutedEventArgs e)
        {
            ObservingPageUrl.Focus();
        }

        private void DEPageLinksObserving_Closed(object sender, EventArgs e)
        {
            ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.LogMessageAdded -= TaskLink_LogMessageAdded;
            if (ChromiumBrowserLoaded != null)
            {
                ChromiumBrowserLoaded.Dispose();
                ChromiumBrowserLoaded = null;
            }
        }

        private void AddSelectedLinksToCrawling_Click(object sender, RoutedEventArgs e)
        {
            WaitLoaderWithoutLogs.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    List<string> SelectedLinksToCrawl = new List<string>();
                    base.Dispatcher.Invoke(delegate
                    {
                        for (int i = 0; i < FoundedLinksListDataGridView.SelectedItems.Count; i++)
                        {
                            SelectedLinksToCrawl.Add((FoundedLinksListDataGridView.SelectedItems[i] as PageLink).NormalizedOriginalLink);
                        }
                    });
                    if (SelectedLinksToCrawl.Count == 0)
                    {
                        MessageBox.Show("You haven't selected any links. Please, select links to addition.", "No links selected", MessageBoxButton.OK, MessageBoxImage.Hand);
                        base.Dispatcher.Invoke(delegate
                        {
                            WaitLoaderWithoutLogs.Visibility = Visibility.Hidden;
                        });
                    }
                    else
                    {
                        CrawlingServerAddLinkToCrawlResults LinksAddingResults = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.AddLinksToCrawling(SelectedLinksToCrawl);
                        base.Dispatcher.Invoke(delegate
                        {
                            DEAddLinksToCrawlingResults dEAddLinksToCrawlingResults = new DEAddLinksToCrawlingResults(LinksAddingResults, ProjectCubeLink, isModalOpenedFromStartButtonClick: false);
                            dEAddLinksToCrawlingResults.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            dEAddLinksToCrawlingResults.Show();
                            ProjectCubeLink.ParentWindowLink.ShowMainWindowShadowOverlay(dEAddLinksToCrawlingResults);
                            ProjectCubeLink.ParentWindowLink.PreventShadowOverlayHiding();
                            WaitLoaderWithoutLogs.Visibility = Visibility.Hidden;
                            Close();
                        });
                    }
                }
                catch (Exception exception)
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoaderWithoutLogs.Visibility = Visibility.Hidden;
                        MessageBox.Show("Something went wrong. Please, try again or contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    });
                    App.TrySendAppCrashReport(exception, "Error in links observing - task inner level");
                }
            });
        }

        private void TryToObserveDataButton_Click(object sender, RoutedEventArgs e)
        {
            ObserveLinks();
        }

        private void ObservingPageUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ObserveLinks();
            }
        }

        private void ObserveLinks()
        {
            ObservingPageUrl.MarkAsCorrectlyCompleted();
            string PageUrl = ObservingPageUrl.Text.Trim();
            string websiteRootUrl = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.WebsiteRootUrl;
            if (ChromiumBrowserLoaded != null)
            {
                base.Dispatcher.Invoke(delegate
                {
                    ChromiumBrowserLoaded.Dispose();
                    ChromiumBrowserLoaded = null;
                });
            }
            if ((PageUrl.IndexOf("http://www.") == 0 || PageUrl.IndexOf("https://www.") == 0) && websiteRootUrl.IndexOf("http://www.") == -1 && websiteRootUrl.IndexOf("https://www.") == -1)
            {
                PageUrl = PageUrl.Replace("http://www.", "http://").Replace("https://www.", "https://");
            }
            if ((websiteRootUrl.IndexOf("http://www.") == 0 || websiteRootUrl.IndexOf("https://www.") == 0) && PageUrl.IndexOf("http://www.") == -1 && PageUrl.IndexOf("https://www.") == -1)
            {
                PageUrl = PageUrl.Replace("https://", "https://www.").Replace("http://", "http://www.");
            }
            if (PageUrl.Length == 0 || !Uri.IsWellFormedUriString(PageUrl, UriKind.Absolute) || !ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.IsLinkRefferedToDomain(PageUrl))
            {
                if ((PageUrl.IndexOf("http://") != -1 && websiteRootUrl.IndexOf("https://") != -1) || (PageUrl.IndexOf("https://") != -1 && websiteRootUrl.IndexOf("http://") != -1))
                {
                    MessageBox.Show($"Data protocols at the home page and at the testing link are different (http and https). Please use a page with the same protocol as the main page of the site in the main settings ({websiteRootUrl}).", "Different data protocols", MessageBoxButton.OK, MessageBoxImage.Hand);
                    ObservingPageUrl.MarkAsUncorrectlyCompleted();
                    return;
                }
                try
                {
                    Uri uri = new Uri(PageUrl);
                    string text = $"{uri.Scheme}://{uri.Host}";
                    if (websiteRootUrl.Trim('/') != text.Trim('/'))
                    {
                        MessageBox.Show($"The domain of the site from the project settings does not coincide with the domain from the link you have entered. Site address in the settings = {websiteRootUrl}, the site address from the link you have entered = {text}. Please, make sure that you have entered the right link.", "Different domains", MessageBoxButton.OK, MessageBoxImage.Hand);
                        ObservingPageUrl.MarkAsUncorrectlyCompleted();
                        return;
                    }
                }
                catch (Exception)
                {
                }
                MessageBox.Show("Wrong page address", "Wrong URL", MessageBoxButton.OK, MessageBoxImage.Hand);
                ObservingPageUrl.MarkAsUncorrectlyCompleted();
                return;
            }
            HwndSource rectangleHWND = null;
            base.Dispatcher.Invoke(delegate
            {
                rectangleHWND = (HwndSource)PresentationSource.FromVisual(this);
            });
            WaitLoader.Visibility = Visibility.Visible;
            WaitLoader.ClearLogs();
            Task.Run(delegate
            {
                ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.LogMessageAdded -= TaskLink_LogMessageAdded;
                ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.LogMessageAdded += TaskLink_LogMessageAdded;
                DataExcavatorPageLinksObservingResult ObservingResults = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.ObservePageLinks(PageUrl);
                if (ObservingResults.PageLinks != null)
                {
                    for (int i = 0; i < ObservingResults.PageLinks.Count; i++)
                    {
                        ObservedLinks.Add(ObservingResults.PageLinks[i]);
                    }
                }
                string TestLogsAssembled = string.Join("\r\n", ObservingResults.PageObservingLogs);
                base.Dispatcher.Invoke(delegate
                {
                    TestLogsTextBox.Text = TestLogsAssembled;
                    if (ObservingResults.PageCrawledResponseData != null)
                    {
                        PageSourceTextBox.Text = ObservingResults.PageCrawledResponseData.DownloadedPageHtml;
                    }
                    FoundedLinksListDataGridView.ItemsSource = ObservedLinks;
                    FoundedLinksListDataGridView.Items.Refresh();
                    if (ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetCrawlingServerPropertiesCopy().PrimaryDataCrawlingWay == DataCrawlingType.CEFCrawling)
                    {
                        if (ObservingResults.PageCrawledResponseData != null && ObservingResults.PageCrawledResponseData != null)
                        {
                            /*RenderedPageContent.Content = ObservingResults.CEFCurrentBrowser;
							ChromiumBrowserLoaded = ObservingResults.CEFCurrentBrowser;*/
                        }
                    }
                    else
                    {
                        TextBlock content = new TextBlock
                        {
                            Text = "It is not possible to render a page with CrawlerSettings.CrawlerEngine=NativeCrawling. Please use CEFCrawling.",
                            FontSize = 18.0,
                            Padding = new Thickness(10.0)
                        };
                        RenderedPageContent.Content = content;
                    }
                    if (!ObservingResults.Success)
                    {
                        MessageBox.Show("There is some problems with links observing. Please, see logs.", "Links observing error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        LinksObservingTab.SelectedIndex = 2;
                    }
                    WaitLoader.Visibility = Visibility.Hidden;
                });
            });
        }

        private void TaskLink_LogMessageAdded(DataExcavatorTaskEventCallback Callback)
        {
            WaitLoader.AddLogEntry(Callback.GetEventAssembledText());
        }
    }
}
