using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.CEF;
using ExcavatorSharp.Common;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Objects;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DECreateFirstProject.xaml
    /// </summary>
    public partial class DECreateFirstProject : Window
    {
        private string ActuallyOfferedWebsiteDomainIfTemplateFound = string.Empty;

        public DOMSelectorsTester CSSSelectorsTester = new DOMSelectorsTester();

        private bool IsImageSelectorFound = false;

        private int ImageSelectorFieldIndex = -1;

        private MainWindow MainWindowLink { get; set; }

        public DECreateFirstProject()
        {
            InitializeComponent();
        }

        public DECreateFirstProject(MainWindow MainWindowLink)
        {
            this.MainWindowLink = MainWindowLink;
            InitializeComponent();
        }

        private void CreateNewProject_Click(object sender, RoutedEventArgs e)
        {
            WebsiteUrl.MarkAsCorrectlyCompleted();
            CSSSelector1.MarkAsCorrectlyCompleted();
            CSSSelector2.MarkAsCorrectlyCompleted();
            CSSSelector3.MarkAsCorrectlyCompleted();
            string text = WebsiteUrl.Text.Trim();
            string text2 = CSSSelector1.Text.Trim();
            string text3 = CSSSelector2.Text.Trim();
            string text4 = CSSSelector3.Text.Trim();
            bool flag = false;
            if (text.Length == 0)
            {
                WebsiteUrl.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (text2.Length == 0 && text3.Length == 0 && text4.Length == 0)
            {
                CSSSelector1.MarkAsUncorrectlyCompleted();
                CSSSelector2.MarkAsUncorrectlyCompleted();
                CSSSelector3.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
            {
                WebsiteUrl.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (text2.IndexOf(":") != -1 && CSSHelpers.CheckIsCSSPseudoSelectorUsed(text2))
            {
                MessageBox.Show($"CSS pseudo-selectors are not supported. Selector value = {text2}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                CSSSelector1.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            /*else if (!CSSSelectorsTester.TestSelector(text2, DataGrabbingSelectorType.CSS_Selector))
			{
				MessageBox.Show($"Wrong or not-supported node selector. Selector value = {text2}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				CSSSelector1.MarkAsUncorrectlyCompleted();
				flag = true;
			}*/
            if (text3.IndexOf(":") != -1 && CSSHelpers.CheckIsCSSPseudoSelectorUsed(text3))
            {
                MessageBox.Show($"CSS pseudo-selectors are not supported. Selector value = {text3}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                CSSSelector2.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            /*else if (!CSSSelectorsTester.TestSelector(text3, DataGrabbingSelectorType.CSS_Selector))
			{
				MessageBox.Show($"Wrong or not-supported node selector. Selector value = {text3}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				CSSSelector2.MarkAsUncorrectlyCompleted();
				flag = true;
			}*/
            if (text4.IndexOf(":") != -1 && CSSHelpers.CheckIsCSSPseudoSelectorUsed(text4))
            {
                MessageBox.Show($"CSS pseudo-selectors are not supported. Selector value = {text4}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                CSSSelector3.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            /*else if (!CSSSelectorsTester.TestSelector(text4, DataGrabbingSelectorType.CSS_Selector))
			{
				MessageBox.Show($"Wrong or not-supported node selector. Selector value = {text4}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				CSSSelector3.MarkAsUncorrectlyCompleted();
				flag = true;
			}*/
            if (!flag && !LookupForTheProjectInTemplatesLibrary(text))
            {
                List<string> list = new List<string>();
                if (text2.Length > 0)
                {
                    list.Add(text2);
                }
                if (text3.Length > 0)
                {
                    list.Add(text3);
                }
                if (text4.Length > 0)
                {
                    list.Add(text4);
                }
                DEProjectCubeProperties dEProjectCubeProperties = new DEProjectCubeProperties(MainWindowLink, text, list, ImageSelectorFieldIndex);
                dEProjectCubeProperties.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Close();
                dEProjectCubeProperties.Closed += NewProjectModal_Closed;
                MainWindowLink.ShowMainWindowShadowOverlay(dEProjectCubeProperties);
                dEProjectCubeProperties.Show();
            }
        }

        private void NewProjectModal_Closed(object sender, EventArgs e)
        {
            MainWindowLink.HideMainWindowShadowOverlay();
        }

        private void AutoDetectCSSSelectors_Click(object sender, RoutedEventArgs e)
        {
            TryToDetectCSSSelectors();
        }

        private void TryToDetectCSSSelectors()
        {
            // if (!DELicenseInfo.IsLicenseOK(MainWindowLink))
            // {
            //     MessageBox.Show("It was not possible to start the search for .CSS selectors because a valid license to use the program was not found.", "License error", MessageBoxButton.OK, MessageBoxImage.Hand);
            //     return;
            // }
            WebsiteUrl.MarkAsCorrectlyCompleted();
            CSSSelector1.Text = "";
            CSSSelector2.Text = "";
            CSSSelector3.Text = "";
            string WebsiteUrlLinkText = WebsiteUrl.Text.Trim();
            bool flag = false;
            if (WebsiteUrlLinkText.Length == 0)
            {
                WebsiteUrl.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (!Uri.IsWellFormedUriString(WebsiteUrlLinkText, UriKind.Absolute))
            {
                WebsiteUrl.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (flag)
            {
                return;
            }
            Uri uri = new Uri(WebsiteUrl.Text);
            string WebsiteRootUri = $"{uri.Scheme}://{uri.Host}";
            if (LookupForTheProjectInTemplatesLibrary(WebsiteUrlLinkText))
            {
                return;
            }
            WaitLoaderWithLogs.Visibility = Visibility.Visible;
            WaitLoaderWithLogs.ClearLogs();
            Task.Run(delegate
            {
                try
                {
                    IsImageSelectorFound = false;
                    ImageSelectorFieldIndex = -1;
                    string text = string.Format("{0}/{1}", DEClientInterface.InterfaceLogic.IOCommon.GetDataExcavatorUIFolderPath(), "temp-projects");
                    if (!Directory.Exists(text))
                    {
                        Directory.CreateDirectory(text);
                    }
                    string text2 = "";
                    bool flag2 = false;
                    while (!flag2)
                    {
                        Random random = new Random();
                        text2 = string.Format("{0}/{1}_{2}_{3}", text, DateTime.Now.ToString("MMdd"), DateTime.Now.ToString("HHmmss"), random.Next(100, 500));
                        if (!Directory.Exists(text2))
                        {
                            Directory.CreateDirectory(text2);
                            flag2 = true;
                        }
                    }
                    if (text2 == string.Empty)
                    {
                        string message = "An empty directory was rolled (DECreateFirstProject.xaml.cs)";
                        Logger.LogError(message, new Exception(message));
                        MessageBox.Show($"Something went wrong. Please, try again.");
                    }
                    else
                    {
                        CrawlingServerProperties crawlingServerProperties = new CrawlingServerProperties(DataCrawlingType.CEFCrawling, new string[1] { "*" });
                        crawlingServerProperties.CEFCrawlingBehaviors = new List<CEFCrawlingBehavior>();
                        crawlingServerProperties.CEFCrawlingBehaviors.Add(new CEFCrawlingBehavior("*", 10));
                        crawlingServerProperties.PageDownloadTimeoutMilliseconds = 60000;
                        DataExcavatorTask dataExcavatorTask = new DataExcavatorTask("Auto detect .CSS-selectors", WebsiteRootUri,
                            /*WebsiteRootUri,*/
                            ".CSS selectors auto-detect", new List<DataGrabbingPattern>(), crawlingServerProperties, new GrabbingServerProperties(1, 5000, StoreGrabbedData: false), text2);
                        dataExcavatorTask.LogMessageAdded -= DETask_LogMessageAdded;
                        dataExcavatorTask.LogMessageAdded += DETask_LogMessageAdded;
                        HwndSource rectangleHWND = null;
                        base.Dispatcher.Invoke(delegate
                        {
                            rectangleHWND = (HwndSource)PresentationSource.FromVisual(this);
                        });
                        DataExcavatorPageLinksObservingResult dataExcavatorPageLinksObservingResult = dataExcavatorTask.ObservePageLinks(WebsiteUrlLinkText);
                        if (!dataExcavatorPageLinksObservingResult.Success)
                        {
                            MessageBox.Show("Can't auto-detect .CSS-selectors. Please, complete them maually.");
                            base.Dispatcher.Invoke(delegate
                            {
                                WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                            });
                        }
                        else
                        {
                            string downloadedPageHtml = dataExcavatorPageLinksObservingResult.PageCrawledResponseData.DownloadedPageHtml;
                            try
                            {
                                Directory.Delete(text2, recursive: true);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"Error during temp directory deleting. Path = {text2}", ex);
                                App.TrySendAppCrashReport(ex, "Error during temp directory deleting");
                            }
                            string[] item = new string[19]
                            {
                                "h1#title", "[itemtype=\"http://schema.org/Product\"] [itemprop=\"name\"]", "h1[itemprop=\"name\"]", "h1", "[itemprop=\"name\"]", "#title", ".title", ".page-title", ".product-title", ".item-name",
                                ".itemName", ".itemname", ".item-title", ".itemTitle", "h2", "h3", "h4", "h5", "h6"
                            };
                            string[] item2 = new string[15]
                            {
                                "[itemtype=\"http://schema.org/Product\"] [itemprop=\"price\"]", "[itemprop=\"price\"]", ".price", ".product-price", ".productPrice", ".product-price", ".prod-price", ".amount", ".cost", "#prcIsum",
                                ".current-price-value", ".price-value", ".current-price", ".priceVal", ".StandardPriceBlock"
                            };
                            string[] ProductImagesCSSSelectors = new string[10] { "[itemtype=\"http://schema.org/Product\"] [itemprop=\"image\"] img", "[itemprop=\"image\"] img", "[itemtype=\"http://schema.org/Product\"] [itemprop=\"image\"]", "[itemprop=\"image\"]", ".imgTagWrapper img", ".main-image", ".image", ".item-image", ".zoomer-image", ".zoomImg" };
                            string[] item3 = new string[9] { "[itemtype=\"http://schema.org/Product\"] [itemprop=\"description\"]", "[itemprop=\"description\"]", ".description", ".details", ".item-description", ".item-details", ".item_description", ".item_details", ".itemDetails" };
                            string[] item4 = new string[5] { "[itemtype=\"http://schema.org/Product\"] .features", "#feature-bullets", ".item-features", ".properties", ".itemProps" };
                            HtmlDocument htmlDocument = new HtmlDocument();
                            htmlDocument.LoadHtml(downloadedPageHtml);
                            Queue<string> FoundSelectors = new Queue<string>();
                            List<string[]> list = new List<string[]> { item, item2, ProductImagesCSSSelectors, item3, item4 };
                            for (int i = 0; i < list.Count; i++)
                            {
                                string[] array = list[i];
                                foreach (string text3 in array)
                                {
                                    IList<HtmlNode> list2 = htmlDocument.QuerySelectorAll(text3);
                                    if (list2.Count > 0)
                                    {
                                        FoundSelectors.Enqueue(text3);
                                        break;
                                    }
                                }
                            }
                            List<TextBox> TextBoxesToComplete = new List<TextBox>();
                            TextBoxesToComplete.Add(CSSSelector1);
                            TextBoxesToComplete.Add(CSSSelector2);
                            TextBoxesToComplete.Add(CSSSelector3);
                            base.Dispatcher.Invoke(delegate
                            {
                                for (int k = 0; k < TextBoxesToComplete.Count; k++)
                                {
                                    if (FoundSelectors.Count == 0)
                                    {
                                        break;
                                    }
                                    string text4 = FoundSelectors.Dequeue();
                                    TextBoxesToComplete[k].Text = text4;
                                }
                                for (int l = 0; l < TextBoxesToComplete.Count; l++)
                                {
                                    bool flag3 = false;
                                    string text5 = TextBoxesToComplete[l].Text;
                                    for (int m = 0; m < ProductImagesCSSSelectors.Length; m++)
                                    {
                                        if (ProductImagesCSSSelectors[m] == text5)
                                        {
                                            flag3 = true;
                                            IsImageSelectorFound = true;
                                            ImageSelectorFieldIndex = l;
                                            break;
                                        }
                                    }
                                    if (flag3)
                                    {
                                        break;
                                    }
                                }
                                WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                            });
                        }
                    }
                }
                catch (Exception ex2)
                {
                    MessageBox.Show($"Fatal error: {ex2.ToString()}. Please, contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    Logger.LogError("Application fatal exception thrown", ex2);
                    App.TrySendAppCrashReport(ex2, "Error in express project creation");
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoaderWithLogs.Visibility = Visibility.Hidden;
                    });
                }
            });
        }

        private bool LookupForTheProjectInTemplatesLibrary(string WebsitePageUrl)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(WebsitePageUrl);
            }
            catch (Exception)
            {
                return false;
            }
            string host = uri.Host;
            if (ActuallyOfferedWebsiteDomainIfTemplateFound != string.Empty && host == ActuallyOfferedWebsiteDomainIfTemplateFound)
            {
                return false;
            }
            ActuallyOfferedWebsiteDomainIfTemplateFound = host;
            for (int i = 0; i < MainWindowLink.TemplatesStorage.ProjectTemplates.Count; i++)
            {
                if (MainWindowLink.TemplatesStorage.ProjectTemplates[i].TemplateWebsiteURL.IndexOf(host) != -1)
                {
                    ActuallyOfferedWebsiteDomainIfTemplateFound = host;
                    MessageBoxResult messageBoxResult = MessageBox.Show($"We have ready-made settings for {host} website. It's more complex than can be done with express-project. Do you want to use ready-made template?", "Template found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        MainWindowLink.PreventShadowOverlayHiding();
                        MainWindowLink.importProjectFromTemplateModal = new DEProjectCubeProperties(MainWindowLink, MainWindowLink.TemplatesStorage.ProjectTemplates[i]);
                        MainWindowLink.importProjectFromTemplateModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        MainWindowLink.importProjectFromTemplateModal.Closed += ImportProjectFromTemplateModal_Closed;
                        MainWindowLink.importProjectFromTemplateModal.Show();
                        MainWindowLink.ShowMainWindowShadowOverlay(MainWindowLink.importProjectFromTemplateModal);
                        Close();
                        return true;
                    }
                }
            }
            return false;
        }

        private void ImportProjectFromTemplateModal_Closed(object sender, EventArgs e)
        {
            MainWindowLink.importProjectFromTemplateModal = null;
            MainWindowLink.HideMainWindowShadowOverlay();
        }

        private void DETask_LogMessageAdded(DataExcavatorTaskEventCallback Callback)
        {
            base.Dispatcher.Invoke(delegate
            {
                WaitLoaderWithLogs.AddLogEntry(Callback.GetEventAssembledText());
            });
        }

        private void HelpHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void WebsiteUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TryToDetectCSSSelectors();
            }
        }
    }
}
