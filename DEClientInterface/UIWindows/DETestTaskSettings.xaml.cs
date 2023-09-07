using CefSharp.Wpf;
using DEClientInterface.Logic;
using DEClientInterface.Objects;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Crawler;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Exporter;
using ExcavatorSharp.Grabber;
using ExcavatorSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for DETestTaskSettings.xaml
    /// </summary>
    public partial class DETestTaskSettings : Window
    {
        private Thread MakeSettingsTestingThread = null;

        private ChromiumWebBrowser ChromiumBrowserLoaded = null;


        private DataExcavatorTask ExcavatorTaskLink { get; set; }

        private DEProjectTestingUrlContainer ProjectTestURLContainer { get; set; }

        public DETestTaskSettings(DataExcavatorTask ExcavatorTaskLink, DEProjectTestingUrlContainer ProjectTestURLContainer)
        {
            this.ProjectTestURLContainer = ProjectTestURLContainer;
            this.ExcavatorTaskLink = ExcavatorTaskLink;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", this.ExcavatorTaskLink.TaskName);
            ExamplePageUrl.Text = this.ProjectTestURLContainer.LastTestingURL;
            base.Closed += DETestTaskSettings_Closed;
            base.Loaded += DETestTaskSettings_Loaded;
        }

        private void DETestTaskSettings_Loaded(object sender, RoutedEventArgs e)
        {
            ExamplePageUrl.SelectAll();
            ExamplePageUrl.Focus();
        }

        private void DETestTaskSettings_Closed(object sender, EventArgs e)
        {
            ExcavatorTaskLink.LogMessageAdded -= ExcavatorTaskLink_LogMessageAdded;
            if (ChromiumBrowserLoaded != null)
            {
                ChromiumBrowserLoaded.Dispose();
                ChromiumBrowserLoaded = null;
            }
        }

        private void TryToGrabDataButton_Click(object sender, RoutedEventArgs e)
        {
            TryToMakeURLTest();
        }

        private void TryToMakeURLTest()
        {
            ExamplePageUrl.MarkAsCorrectlyCompleted();
            string text = ExamplePageUrl.Text.Trim();
            string websiteRootUrl = ExcavatorTaskLink.WebsiteRootUrl;
            ProjectTestURLContainer.LastTestingURL = text;
            if ((text.IndexOf("http://www.") == 0 || text.IndexOf("https://www.") == 0) && websiteRootUrl.IndexOf("http://www.") == -1 && websiteRootUrl.IndexOf("https://www.") == -1)
            {
                text = text.Replace("http://www.", "http://").Replace("https://www.", "https://");
            }
            if ((websiteRootUrl.IndexOf("http://www.") == 0 || websiteRootUrl.IndexOf("https://www.") == 0) && text.IndexOf("http://www.") == -1 && text.IndexOf("https://www.") == -1)
            {
                text = text.Replace("https://", "https://www.").Replace("http://", "http://www.");
            }
            if (text.Length == 0 || !Uri.IsWellFormedUriString(text, UriKind.Absolute) || !ExcavatorTaskLink.IsLinkRefferedToDomain(text))
            {
                if ((text.IndexOf("http://") != -1 && websiteRootUrl.IndexOf("https://") != -1) || (text.IndexOf("https://") != -1 && websiteRootUrl.IndexOf("http://") != -1))
                {
                    MessageBox.Show($"Data protocols at the home page and at the testing link are different (http and https). Please use a page with the same protocol as the main page of the site in the main settings ({websiteRootUrl}).", "Different data protocols", MessageBoxButton.OK, MessageBoxImage.Hand);
                    ExamplePageUrl.MarkAsUncorrectlyCompleted();
                    return;
                }
                try
                {
                    Uri uri = new Uri(text);
                    string text2 = $"{uri.Scheme}://{uri.Host}";
                    if (websiteRootUrl.Trim('/') != text2.Trim('/'))
                    {
                        MessageBox.Show($"The domain of the site from the project settings does not coincide with the domain from the link you have entered. Site address in the settings = {websiteRootUrl}, the site address from the link you have entered = {text2}. Please, make sure that you have entered the right link.", "Different domains", MessageBoxButton.OK, MessageBoxImage.Hand);
                        ExamplePageUrl.MarkAsUncorrectlyCompleted();
                        return;
                    }
                }
                catch (Exception)
                {
                }
                MessageBox.Show("Wrong page address", "Wrong URL", MessageBoxButton.OK, MessageBoxImage.Hand);
                ExamplePageUrl.MarkAsUncorrectlyCompleted();
            }
            else
            {
                MakeSettingsTestingThread = new Thread(MakeSettingsTestingThreadBody);
                MakeSettingsTestingThread.Start(new MakeSettingsTestingArg
                {
                    PageUrl = text,
                    ParentThreadLink = MakeSettingsTestingThread
                });
            }
        }

        private void MakeSettingsTestingThreadBody(object Args)
        {
            MakeSettingsTestingArg makeSettingsTestingArg = Args as MakeSettingsTestingArg;
            base.Dispatcher.BeginInvoke((Action)delegate
            {
                TestLogsTextBox.Text = string.Empty;
                PageSourceTextBox.Text = string.Empty;
                GrabbingResultsTextBox.Text = string.Empty;
                GrabbingResultsImagesStackPanel.Children.Clear();
                WaitLoaderWithLogs.ClearLogs();
                WaitLoaderWithLogs.Visibility = Visibility.Visible;
                WaitLoaderWithLogs.AddLogEntry("Test started");
            });
            if (ChromiumBrowserLoaded != null)
            {
                base.Dispatcher.Invoke(delegate
                {
                    ChromiumBrowserLoaded.Dispose();
                    ChromiumBrowserLoaded = null;
                });
            }
            ExcavatorTaskLink.LogMessageAdded -= ExcavatorTaskLink_LogMessageAdded;
            ExcavatorTaskLink.LogMessageAdded += ExcavatorTaskLink_LogMessageAdded;
            HwndSource rectangleHWND = null;
            base.Dispatcher.Invoke(delegate
            {
                rectangleHWND = (HwndSource)PresentationSource.FromVisual(this);
            });
            DataExcavatorTaskTestingResult ScrapingTestResults = null;
            try
            {
                ScrapingTestResults = ExcavatorTaskLink.TestTask(makeSettingsTestingArg.PageUrl);
            }
            catch (Exception ex2)
            {
                Exception ex3 = ex2;
                Exception ex = ex3;
                Logger.LogError($"Error thrown during task testing; Page url = {makeSettingsTestingArg.PageUrl}", ex);
                App.TrySendAppCrashReport(ex, "Error in project settings test");
                base.Dispatcher.Invoke(delegate
                {
                    TestLogsTextBox.Text = $"Exception thrown; \r\n {ex.ToString()}";
                    PageSourceTextBox.Text = string.Empty;
                    GrabbingResultsTextBox.Text = string.Empty;
                    GrabbingResultsImagesStackPanel.Children.Clear();
                    WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                });
                makeSettingsTestingArg.ParentThreadLink.Abort();
                return;
            }
            if (ScrapingTestResults == null)
            {
                Logger.LogInformation("Test failed. Please, verify your settings and scraping timeout.");
                base.Dispatcher.Invoke(delegate
                {
                    TestLogsTextBox.Text = $"Test failed. Please, verify your settings and scraping timeout.";
                    PageSourceTextBox.Text = string.Empty;
                    GrabbingResultsTextBox.Text = string.Empty;
                    GrabbingResultsImagesStackPanel.Children.Clear();
                    WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                });
                makeSettingsTestingArg.ParentThreadLink.Abort();
                return;
            }
            base.Dispatcher.BeginInvoke((Action)delegate
            {
                try
                {
                    DETestResultsObjectiveList_StakPanel.Children.Clear();
                    if (ScrapingTestResults.GrabbedData != null && ScrapingTestResults.GrabbedData.GrabbedDataGroups.Count > 0)
                    {
                        List<GrabbedDataGroup> grabbedDataGroups = ScrapingTestResults.GrabbedData.GrabbedDataGroups;
                        GrabbedDataGroup grabbedDataGroup = grabbedDataGroups[0];
                        List<GroupedDataItem> list = grabbedDataGroup.GrabbingResults[0];
                        for (int i = 0; i < list.Count; i++)
                        {
                            try
                            {
                                DEFoundDataRow dEFoundDataRow = new DEFoundDataRow();
                                dEFoundDataRow.LoadResultsData(list[i], string.Empty, ScrapingTestResults.GrabbedData.BinaryDataItems);
                                DETestResultsObjectiveList_StakPanel.Children.Add(dEFoundDataRow);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    string text = string.Join("\r\n", ScrapingTestResults.LogsList);
                    TestLogsTextBox.Text = text;
                    if (ScrapingTestResults.PageCrawledResponseData != null)
                    {
                        PageSourceTextBox.Text = ScrapingTestResults.PageCrawledResponseData.DownloadedPageHtml;
                    }
                    if (ScrapingTestResults.GrabbedData != null && ScrapingTestResults.GrabbedData.GrabbedDataGroups.Count > 0)
                    {
                        GrabbingResultsTextBox.Text = GrabbedDataGroup.PresentAsExportedJSON(ScrapingTestResults.GrabbedData.GrabbedDataGroups, DataExportingType.OuterHTML);
                    }
                    else
                    {
                        GrabbingResultsTextBox.Text = string.Empty;
                    }
                    if (ExcavatorTaskLink.GetCrawlingServerPropertiesCopy().PrimaryDataCrawlingWay == DataCrawlingType.CEFCrawling)
                    {
                        if (ScrapingTestResults.PageCrawledResponseData != null)
                        {
                            /*RenderedPageScrollViewer.Content = ScrapingTestResults.CEFCurrentBrowser;
							ChromiumBrowserLoaded = ScrapingTestResults.CEFCurrentBrowser;*/
                        }
                    }
                    else
                    {
                        TextBlock content = new TextBlock
                        {
                            Text = "It is not possible to render page with CrawlerSettings.CrawlerEngine=NativeCrawling. Please use CEFCrawling.",
                            FontSize = 18.0,
                            Padding = new Thickness(10.0)
                        };
                        RenderedPageScrollViewer.Content = content;
                    }
                    GrabbingResultsImagesStackPanel.Children.Clear();
                    if (ScrapingTestResults.GrabbedData != null && ScrapingTestResults.GrabbedData.GrabbedDataGroups.Count > 0)
                    {
                        WebsiteInnerLinksAnalyser websiteInnerLinksAnalyser = new WebsiteInnerLinksAnalyser();
                        for (int j = 0; j < ScrapingTestResults.GrabbedData.BinaryDataItems.Count; j++)
                        {
                            if (ScrapingTestResults.GrabbedData.BinaryDataItems[j].ResourceContent != null)
                            {
                                string fileExtension = websiteInnerLinksAnalyser.GetFileExtension(ScrapingTestResults.GrabbedData.BinaryDataItems[j].AttributeValue);
                                DEFilePreviewCard element = new DEFilePreviewCard(fileExtension, ScrapingTestResults.GrabbedData.BinaryDataItems[j].ResourceContent);
                                GrabbingResultsImagesStackPanel.Children.Add(element);
                            }
                        }
                    }
                    if (!ScrapingTestResults.Success)
                    {
                        MessageBox.Show("There is some problems with task testing. Please, see logs.", "Task testing error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        TestTaskTabControl.SelectedIndex = 3;
                    }
                    else
                    {
                        TestTaskTabControl.SelectedIndex = 0;
                    }
                    WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                }
                catch (Exception ex5)
                {
                    TestLogsTextBox.Text = $"Exception thrown; \r\n {ex5.ToString()}";
                    PageSourceTextBox.Text = string.Empty;
                    GrabbingResultsTextBox.Text = string.Empty;
                    GrabbingResultsImagesStackPanel.Children.Clear();
                    Logger.LogError($"Error during settings test", ex5);
                    App.TrySendAppCrashReport(ex5, "Error in project settings test - dispatcher level");
                    WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                }
            });
            makeSettingsTestingArg.ParentThreadLink.Abort();
        }

        private void ExcavatorTaskLink_LogMessageAdded(DataExcavatorTaskEventCallback Callback)
        {
            WaitLoaderWithLogs.AddLogEntry(Callback.GetEventAssembledText());
        }

        private void ExamplePageUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TryToMakeURLTest();
            }
        }
    }
}
