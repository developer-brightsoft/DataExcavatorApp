// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.DEProjectCubeProperties
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using CefSharp.Wpf;
using DEClientInterface;
using DEClientInterface.InterfaceLogic;
using DEClientInterface.Logic;
using DEClientInterface.Objects;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using DEClientInterface.UIWindows;
using ExcavatorSharp.Captcha;
using ExcavatorSharp.CEF;
using ExcavatorSharp.Common;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Licensing;
using ExcavatorSharp.Objects;
using Newtonsoft.Json;

namespace DEClientInterface
{
	public partial class DEProjectCubeProperties : Window, IComponentConnector
	{
		public DEProjectLink LinkedUITask = null;

		public MainWindow MainWindowLink = null;

		internal DEProjectTestingUrlContainer ProjectCubeTestURLContainer = new DEProjectTestingUrlContainer();

		public DOMSelectorsTester CSSSelectorsTester = new DOMSelectorsTester();

		public ChromiumJSNodesPicker PageNodesPicker = null;

		private DEProjectEntityJSONProperties EditProxiesListModal = null;

		private DEProjectEntityJSONProperties EditCrawlingArgumentsModal = null;

		private Dictionary<string, string> ActualCrawlingArgumentsSet = new Dictionary<string, string>();

		private List<DataCrawlingWebProxy> ActualProxiesList = new List<DataCrawlingWebProxy>();

		private Dictionary<string, string> ActualCrawlingArgumentsList = new Dictionary<string, string>();

		internal List<CEFCrawlingBehavior> ActualCEFCrawlingBehaviors = new List<CEFCrawlingBehavior>();

		internal CEFWebsiteAuthBehavior WebsiteAuthenticaionBehavior = null;

		internal bool isProjectImportCancelled = false;

		private DEWebsiteAuthBehavior WebsiteAuthenticationBehaviorModalWindow { get; set; }

		private DECEFBehaviorEditModal EditCEFBehaviorModal { get; set; }

		private DECaptchaBehavior EditCaptchaBehaviorModal { get; set; }

		private Window helpWindow { get; set; }

		private UIOverlayController overlayController { get; set; }

		internal CAPTCHASolverSettings ActualCAPTCHAResolverSettings { get; set; }

		public DEProjectCubeProperties(MainWindow MainWindowLink)
		{
			this.MainWindowLink = MainWindowLink;
			InitializeComponent();
			base.Title = $"Project properties";
			overlayController = new UIOverlayController(this, ProjectPropertiesShadowOverlay);
			base.Closing += DEProjectCubeProperties_Closing;
			DropSettingToDefaults(ShowMessageBox: false);
			UpdateDataToScrapeNodesCount();
		}

		public DEProjectCubeProperties(MainWindow MainWindowLink = null, DEProjectLink LinkedUITask = null, bool OpenImportProjectDialog = false)
		{
			this.MainWindowLink = MainWindowLink;
			this.LinkedUITask = LinkedUITask;
			InitializeComponent();
			overlayController = new UIOverlayController(this, ProjectPropertiesShadowOverlay);
			base.Closing += DEProjectCubeProperties_Closing;
			if (LinkedUITask != null)
			{
				LoadSettingsFromExistingUITask();
				base.Title = base.Title.Replace("PROJECTNAME", LinkedUITask.ProjectName);
			}
			else if (OpenImportProjectDialog)
			{
				LoadProjectDataFromImportFile();
				base.Title = $"Project properties: import from file";
			}
			UpdateDataToScrapeNodesCount();
		}

		public DEProjectCubeProperties(MainWindow MainWindowLink, DataExcavatorTask ExcavatorTaskLink = null, string ProjectPath = "")
		{
			this.MainWindowLink = MainWindowLink;
			InitializeComponent();
			overlayController = new UIOverlayController(this, ProjectPropertiesShadowOverlay);
			base.Closing += DEProjectCubeProperties_Closing;
			SetPropertiesToSpecified(ExcavatorTaskLink.GetCrawlingServerPropertiesCopy(), ExcavatorTaskLink.GetGrabbingServerPropertiesCopy(), ExcavatorTaskLink.GetDataGrabbingPatternsCopy());
			DEWebsiteRootUrlTextArea.Text = ExcavatorTaskLink.WebsiteRootUrl;
			DETaskNameTextArea.Text = ExcavatorTaskLink.TaskName;
			DETaskOperatingDirectoryTextArea.Text = ProjectPath;
			// DEWebsitePageToScrapeExample.Text = ExcavatorTaskLink.WebsitePageToScrapeExampleUrl;
			// PICKNODES_PageUrlTextBlock.Text = ExcavatorTaskLink.WebsitePageToScrapeExampleUrl;
			// ExamplePageOverlayStartModalTextBox.Text = ExcavatorTaskLink.WebsitePageToScrapeExampleUrl;
			base.Title = $"Project properties: copy project";
			UpdateDataToScrapeNodesCount();
		}

		public DEProjectCubeProperties(MainWindow MainWindowLink, DEProjectTemplate ProjectTemplateLink)
		{
			this.MainWindowLink = MainWindowLink;
			InitializeComponent();
			overlayController = new UIOverlayController(this, ProjectPropertiesShadowOverlay);
			base.Closing += DEProjectCubeProperties_Closing;
			SetPropertiesToSpecified(ProjectTemplateLink.ExcavatorProjectPackedData.CrawlerPeroperties, ProjectTemplateLink.ExcavatorProjectPackedData.GrabberProperties, ProjectTemplateLink.ExcavatorProjectPackedData.GrabbingPatterns);
			DEWebsiteRootUrlTextArea.Text = ProjectTemplateLink.ExcavatorProjectPackedData.WebsiteRootUrl;
			DEWebsitePageToScrapeExample.Text = ProjectTemplateLink.WebsitePageToScrapeExampleUrl;
			PICKNODES_PageUrlTextBlock.Text = ProjectTemplateLink.WebsitePageToScrapeExampleUrl;
			ExamplePageOverlayStartModalTextBox.Text = ProjectTemplateLink.WebsitePageToScrapeExampleUrl;
			DETaskNameTextArea.Text = ProjectTemplateLink.ExcavatorProjectPackedData.ProjectName;
			DETaskOperatingDirectoryTextArea.Text = RollProjectDefaultPath(ProjectTemplateLink.ExcavatorProjectPackedData.ProjectName);
			base.Title = $"Project properties: create from template";
			UpdateDataToScrapeNodesCount();
		}

		public DEProjectCubeProperties(MainWindow MainWindowLink, string TargetPageUrl, List<string> CssSelectorsSet, int ImageSelectorPosition = -1)
		{
			this.MainWindowLink = MainWindowLink;
			InitializeComponent();
			overlayController = new UIOverlayController(this, ProjectPropertiesShadowOverlay);
			base.Closing += DEProjectCubeProperties_Closing;
			Uri uri = new Uri(TargetPageUrl);
			ProjectCubeTestURLContainer = new DEProjectTestingUrlContainer();
			if (TargetPageUrl != string.Empty)
			{
				DEWebsitePageToScrapeExample.Text = TargetPageUrl;
				ProjectCubeTestURLContainer.LastTestingURL = TargetPageUrl;
			}
			DropSettingToDefaults(ShowMessageBox: false);
			base.Title = "Project properties: create express project";
			string text = $"{uri.Scheme}://{uri.Host}";
			DEWebsiteRootUrlTextArea.Text = text;
			string oldProjectName = string.Format("{0}_{1}", uri.Host, DateTime.Now.ToString("yyyy-MM-dd"));
			oldProjectName = this.MainWindowLink.RollNewProjectName(oldProjectName);
			DETaskNameTextArea.Text = oldProjectName;
			DETaskOperatingDirectoryTextArea.Text = RollProjectDefaultPath(oldProjectName);
			DataGrabbingPattern dataGrabbingPattern = new DataGrabbingPattern
			{
				GrabbingItemsPatterns = new List<DataGrabbingPatternItem>(),
				AllowedPageUrlsSubstrings = new string[1] { "*" },
				// AllowedPageHTMLSubstrings = new string[1] { "*" },
				PatternName = "Grabber pattern #0"
			};
			for (int i = 0; i < CssSelectorsSet.Count; i++)
			{
				DataGrabbingPatternItem dataGrabbingPatternItem = new DataGrabbingPatternItem($"Element_{i}", new GrabberSelector(CssSelectorsSet[i], DataGrabbingSelectorType.CSS_Selector));
				if (ImageSelectorPosition != -1 && ImageSelectorPosition == i)
				{
					dataGrabbingPatternItem.ParseBinaryAttributes = true;
					dataGrabbingPatternItem.ParsingBinaryAttributes = new ParsingBinaryAttributePattern[1];
					dataGrabbingPatternItem.ParsingBinaryAttributes[0] = new ParsingBinaryAttributePattern("src", IsAttributeAreLinkToSomeResouce: true, IsWeMustDownloadContentUnderAttributeLink: true);
				}
				dataGrabbingPattern.GrabbingItemsPatterns.Add(dataGrabbingPatternItem);
			}
			DEPatternAllowedPageUrlsSubstringsTextArea.Text = string.Join(", ", dataGrabbingPattern.AllowedPageUrlsSubstrings);
			for (int j = 0; j < dataGrabbingPattern.GrabbingItemsPatterns.Count; j++)
			{
				DEGrabberFlatItemSelector element = new DEGrabberFlatItemSelector(this, dataGrabbingPattern.GrabbingItemsPatterns[j])
				{
					SelectorNumber = 
					{
						Text = "#" + (j + 1)
					}
				};
				ParsingElementsScrollViewerContentPanel.Children.Add(element);
			}
			UpdateDataToScrapeNodesCount();
		}

		public DEProjectCubeProperties(MainWindow MainWindowLink, string TargetPageUrl, List<DataGrabbingPatternItem> CssSelectorsSet)
		{
			this.MainWindowLink = MainWindowLink;
			InitializeComponent();
			overlayController = new UIOverlayController(this, ProjectPropertiesShadowOverlay);
			base.Closing += DEProjectCubeProperties_Closing;
			Uri uri = new Uri(TargetPageUrl);
			ProjectCubeTestURLContainer = new DEProjectTestingUrlContainer();
			if (TargetPageUrl != string.Empty)
			{
				DEWebsitePageToScrapeExample.Text = TargetPageUrl;
				ProjectCubeTestURLContainer.LastTestingURL = TargetPageUrl;
			}
			DropSettingToDefaults(ShowMessageBox: false);
			base.Title = "Project properties: create express project";
			string text = $"{uri.Scheme}://{uri.Host}";
			DEWebsiteRootUrlTextArea.Text = text;
			string oldProjectName = string.Format("{0}_{1}", uri.Host, DateTime.Now.ToString("yyyy-MM-dd"));
			oldProjectName = this.MainWindowLink.RollNewProjectName(oldProjectName);
			DETaskNameTextArea.Text = oldProjectName;
			DETaskOperatingDirectoryTextArea.Text = RollProjectDefaultPath(oldProjectName);
            DataGrabbingPattern dataGrabbingPattern = new DataGrabbingPattern
            {
                GrabbingItemsPatterns = new List<DataGrabbingPatternItem>(),
                AllowedPageUrlsSubstrings = new string[1] { "*" },
                // AllowedPageHTMLSubstrings = new string[1] { "*" },
                PatternName = "Grabber pattern #0"
            };
            dataGrabbingPattern.GrabbingItemsPatterns = CssSelectorsSet;
			DEPatternAllowedPageUrlsSubstringsTextArea.Text = string.Join(", ", dataGrabbingPattern.AllowedPageUrlsSubstrings);
			for (int i = 0; i < dataGrabbingPattern.GrabbingItemsPatterns.Count; i++)
			{
				DEGrabberFlatItemSelector element = new DEGrabberFlatItemSelector(this, dataGrabbingPattern.GrabbingItemsPatterns[i])
				{
					SelectorNumber = 
					{
						Text = "#" + (i + 1)
					}
				};
				ParsingElementsScrollViewerContentPanel.Children.Add(element);
			}
			UpdateDataToScrapeNodesCount();
		}

		public void UpdateDataToScrapeNodesCount()
		{
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				int count = ParsingElementsScrollViewerContentPanel.Children.Count;
				DataToScrapeTabHeadingTextBox.Text = $"Elements to scrape ({count})";
			});
		}

		private void DEProjectCubeProperties_Closing(object sender, CancelEventArgs e)
		{
			CascadeCloseChildren();
		}

		public static string RollProjectDefaultPath(string ProjectName)
		{
			string text = string.Empty;
			bool flag = false;
			int num = 1;
			while (!flag)
			{
				text = $"{DEClientInterface.InterfaceLogic.IOCommon.GetDataExcavatorCommonProjectsFolderPath()}/{ProjectName.RemoveAllPunctuation()}{num++}";
				if (!Directory.Exists(text))
				{
					flag = true;
				}
			}
			return text;
		}

		private void LoadSettingsFromExistingUITask()
		{
			DETaskNameTextArea.Text = LinkedUITask.ProjectName;
			DETaskOperatingDirectoryTextArea.Text = LinkedUITask.ProjectPath;
			DEWebsiteRootUrlTextArea.Text = LinkedUITask.TaskLink.WebsiteRootUrl;
			// DEWebsitePageToScrapeExample.Text = LinkedUITask.TaskLink.WebsitePageToScrapeExampleUrl;
			// PICKNODES_PageUrlTextBlock.Text = LinkedUITask.TaskLink.WebsitePageToScrapeExampleUrl;
			// ExamplePageOverlayStartModalTextBox.Text = LinkedUITask.TaskLink.WebsitePageToScrapeExampleUrl;
			SetPropertiesToSpecified(LinkedUITask.TaskLink.GetCrawlingServerPropertiesCopy(), LinkedUITask.TaskLink.GetGrabbingServerPropertiesCopy(), LinkedUITask.TaskLink.GetDataGrabbingPatternsCopy());
		}

		public void SaveSettings()
		{
			// if (MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() == null || (MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() != null && !MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().IsKeyDateValidAndNonOutdated()))
			// {
			// 	System.Windows.MessageBox.Show("License error. Please, use demo or buy a license key.", "License error", MessageBoxButton.OK, MessageBoxImage.Hand);
			// 	return;
			// }
			DEprojectCubeSettingsSet ActualSettings = ValidateSettingsAndGetAssembledSettingsContainer();
			if (ProjectCubeTestURLContainer.LastTestingURL == string.Empty)
			{
				ProjectCubeTestURLContainer.LastTestingURL = ActualSettings.WebsitePageToScrapeExampleUrl;
			}
			if (ActualSettings == null)
			{
				return;
			}
			if (LinkedUITask == null)
			{
				DataExcavatorTask dataExcavatorTask = new DataExcavatorTask(
					ActualSettings.TaskName, 
					ActualSettings.WebsiteRootUrl, 
					//ActualSettings.WebsitePageToScrapeExampleUrl, 
					ActualSettings.TaskDescription, 
					ActualSettings.GrabbingPatternsList, 
					ActualSettings.CrawlerProperties, 
					ActualSettings.GrabberProperties, 
					ActualSettings.TaskOperatingDirectory);
				dataExcavatorTask.SaveTaskToHDD();
				LinkedUITask = new DEProjectLink(dataExcavatorTask);
				MainWindowLink.ProcessAddDataExcavatorTaskIntoUI(LinkedUITask);
				MainWindowLink.DEUIProjectsStorage.FlushIndexFileToHDD();
				LinkedUITask.ProjectCubeLink.ProjectCubeTestURLContainer = ProjectCubeTestURLContainer;
				MainWindowLink.MainTabControl.SelectedIndex = 0;
				MainWindowLink.NoProjectsYetOverlay.Visibility = Visibility.Hidden;
				Close();
				return;
			}
			LinkedUITask.ProjectName = ActualSettings.TaskName;
			LinkedUITask.ProjectPath = ActualSettings.TaskOperatingDirectory;
			if (LinkedUITask.TaskLink.TaskState == DataExcavatorTaskState.Executing)
			{
				LinkedUITask.TaskLink.StopTask(delegate
				{
					LinkedUITask.ProjectCubeLink.StartCountersRefresher();
					LinkedUITask.ProjectCubeLink.UnmarkStartButtonPressed();
					LinkedUITask.TaskLink.UpdateExcavatorTaskData(ActualSettings.TaskName, ActualSettings.WebsiteRootUrl, 
							// ActualSettings.WebsitePageToScrapeExampleUrl, 
							ActualSettings.TaskDescription, ActualSettings.GrabbingPatternsList, ActualSettings.CrawlerProperties, ActualSettings.GrabberProperties, ActualSettings.TaskOperatingDirectory);
					LinkedUITask.TaskLink.SaveTaskToHDD();
					MainWindowLink.DEUIProjectsStorage.FlushIndexFileToHDD();
					LinkedUITask.ProjectCubeLink.ProjectCubeTestURLContainer = ProjectCubeTestURLContainer;
					LinkedUITask.ProjectCubeLink.ReInitializeDETaskLogic();
					Close();
				});
			}
			else
			{
				LinkedUITask.TaskLink.UpdateExcavatorTaskData(ActualSettings.TaskName, ActualSettings.WebsiteRootUrl, 
						// ActualSettings.WebsitePageToScrapeExampleUrl, 
						ActualSettings.TaskDescription, ActualSettings.GrabbingPatternsList, ActualSettings.CrawlerProperties, ActualSettings.GrabberProperties, ActualSettings.TaskOperatingDirectory);
				LinkedUITask.TaskLink.SaveTaskToHDD();
				MainWindowLink.DEUIProjectsStorage.FlushIndexFileToHDD();
				LinkedUITask.ProjectCubeLink.ProjectCubeTestURLContainer = ProjectCubeTestURLContainer;
				LinkedUITask.ProjectCubeLink.ReInitializeDETaskLogic();
				Close();
			}
		}

		private DEprojectCubeSettingsSet ValidateSettingsAndGetAssembledSettingsContainer()
		{
			string text = DETaskNameTextArea.Text.Trim();
			string text2 = DETaskOperatingDirectoryTextArea.Text.Trim();
			string text3 = DEWebsiteRootUrlTextArea.Text.Trim();
			string text4 = DEWebsitePageToScrapeExample.Text.Trim();
			string taskDescription = "";
			CrawlingServerProperties crawlingServerProperties = new CrawlingServerProperties();
			DataCrawlingType primaryDataCrawlingWay = DataCrawlingType.NativeCrawling;
			if (DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox.SelectedItem != null)
			{
				primaryDataCrawlingWay = EnumToItemsSource.GetValueFromDescription<DataCrawlingType>(DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox.SelectedItem.ToString());
			}
			string[] array = DETaskCrawlingServerRespectOnlySpecifiedUrlsTextArea.Text.Trim().Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = DETaskCrawlingServerUrlSubstringsRestrictionsTextArea.Text.Trim().Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string[] array3 = new string[array.Length];
			string[] array4 = new string[array2.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array3[i] = array[i].Trim();
			}
			for (int j = 0; j < array2.Length; j++)
			{
				array4[j] = array2[j].Trim();
			}
			array = array3;
			array2 = array4;
			int num = 0;
			num = Convert.ToInt32(ApplicationSpeedBar.Value);
			int result = 0;
			if (!int.TryParse(DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.Text, out result))
			{
				result = crawlingServerProperties.CrawlingThreadDelayMilliseconds;
			}
			string text5 = DETaskCrawlingServerCrawlerUserAgentTextArea.Text.Trim();
			if (text5 == string.Empty)
			{
				text5 = crawlingServerProperties.CrawlerUserAgent;
			}
			string httpWebRequestMethod = crawlingServerProperties.HttpWebRequestMethod;
			if (DETaskCrawlingServerHttpWebRequestMethodSelectBox.SelectedItem != null)
			{
				httpWebRequestMethod = (DETaskCrawlingServerHttpWebRequestMethodSelectBox.SelectedItem as ComboBoxItem).Tag.ToString();
			}
			int result2 = 0;
			if (!int.TryParse(DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.Text, out result2))
			{
				result2 = crawlingServerProperties.PageDownloadTimeoutMilliseconds;
			}
			int result3 = 0;
			if (!int.TryParse(DETaskCrawlingServerPageDownloadAttemptsTextArea.Text.Trim(), out result3))
			{
				result3 = crawlingServerProperties.PageDownloadAttempts;
			}
			bool crawlWebsiteLinks = crawlingServerProperties.CrawlWebsiteLinks;
			if (DETaskCrawlingServerCrawlWebsiteLinksSelectBox.SelectedItem != null)
			{
				crawlWebsiteLinks = bool.Parse((DETaskCrawlingServerCrawlWebsiteLinksSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			bool storeCrawledData = crawlingServerProperties.StoreCrawledData;
			if (DETaskCrawlingServerStoreCrawledDataSelectBox.SelectedItem != null)
			{
				storeCrawledData = bool.Parse((DETaskCrawlingServerStoreCrawledDataSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			bool storeCrawledPagesHTMLSource = crawlingServerProperties.StoreCrawledPagesHTMLSource;
			if (DETaskCrawlingServerStoreCrawledPagesHTMLSourceSelectBox.SelectedItem != null)
			{
				storeCrawledPagesHTMLSource = bool.Parse((DETaskCrawlingServerStoreCrawledPagesHTMLSourceSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			bool reindexCrawledPages = crawlingServerProperties.ReindexCrawledPages;
			if (DETaskCrawlingServerSReindexCrawledPagesSourceSelectBox.SelectedItem != null)
			{
				reindexCrawledPages = bool.Parse((DETaskCrawlingServerSReindexCrawledPagesSourceSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			int result4 = 0;
			if (!int.TryParse(DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.Text.Trim(), out result4))
			{
				result4 = crawlingServerProperties.ReindexCrawledPagesAfterSpecifiedMinutesInterval;
			}
			bool flag = crawlingServerProperties.RespectRobotsTxtFile;
			if (DETaskCrawlingServerRespectRobotsTxtFileSelectBox.SelectedItem != null)
			{
				flag = bool.Parse((DETaskCrawlingServerRespectRobotsTxtFileSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			bool expandPageFrames = crawlingServerProperties.ExpandPageFrames;
			if (DETaskCrawlingServerExpandIFramesSelectBox.SelectedItem != null)
			{
				expandPageFrames = bool.Parse((DETaskCrawlingServerExpandIFramesSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			string text6 = DETaskCrawlingServerRobotsTxtUserAgentRespectationChainTextArea.Text.Trim();
			if (text6 == string.Empty)
			{
				text6 = string.Join(",", CrawlingServerProperties.DefaultRobotsTxtUserAgentRespectationChain);
			}
			int result5 = 0;
			if (!int.TryParse(DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.Text, out result5))
			{
				result5 = crawlingServerProperties.RobotsTxtReindexTimeDays;
			}
			string text7 = DETaskCrawlingServerSitemapUrlTextArea.Text.Trim();
			if (text7 == string.Empty)
			{
				text7 = crawlingServerProperties.SitemapUrl;
			}
			int result6 = 0;
			if (!int.TryParse(DETaskCrawlingServerSitemapReindexTimeDaysTextArea.Text, out result6))
			{
				result6 = crawlingServerProperties.SitemapReindexTimeDays;
			}
			int result7 = 0;
			if (!int.TryParse(DETaskCrawlingServerPagesToCrawlLimitTextArea.Text, out result7))
			{
				result7 = crawlingServerProperties.PagesToCrawlLimit;
			}
			ProxiesRotationType proxiesRotation = ProxiesRotationType.NoRotation;
			if (DETaskCrawlingServerProxiesRotationSelectBox.SelectedItem != null)
			{
				proxiesRotation = EnumToItemsSource.GetValueFromDescription<ProxiesRotationType>(DETaskCrawlingServerProxiesRotationSelectBox.SelectedItem.ToString());
			}
			int result8 = 0;
			if (!int.TryParse(DETaskCrawlingServerLinksBufferHDDAutoSavingMillisecondsTextArea.Text, out result8))
			{
				result8 = crawlingServerProperties.LinksBufferHDDAutoSavingMilliseconds;
			}
			int result9 = 0;
			if (!int.TryParse(DETaskCrawlingServerConcurrentCollectionsParallelismQuantityTextArea.Text, out result9))
			{
				result9 = crawlingServerProperties.ConcurrentCollectionsParallelismQuantity;
			}
			CrawlingServerProperties crawlerProperties = new CrawlingServerProperties(primaryDataCrawlingWay, array, array2, num, result, text5, httpWebRequestMethod, ActualCrawlingArgumentsList, result2, result3, crawlWebsiteLinks, storeCrawledData, storeCrawledPagesHTMLSource, reindexCrawledPages, result4, flag, text6.Split(','), result5, text7, result6, result7, ActualProxiesList, proxiesRotation, result8, result9, ActualCEFCrawlingBehaviors, WebsiteAuthenticaionBehavior, TakeScreenshotAfterPageLoaded: false, expandPageFrames, ActualCAPTCHAResolverSettings);
			GrabbingServerProperties grabbingServerProperties = new GrabbingServerProperties();
			int num2 = num;
			int result10 = 0;
			if (!int.TryParse(DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.Text, out result10))
			{
				result10 = grabbingServerProperties.GrabbingThreadDelayMilliseconds;
			}
			bool storeGrabbedData = grabbingServerProperties.StoreGrabbedData;
			if (DETaskGrabbingServerStoreGrabbedDataSelectBox.SelectedItem != null)
			{
				storeGrabbedData = bool.Parse((DETaskGrabbingServerStoreGrabbedDataSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			bool storeOnlyNonEmptyData = grabbingServerProperties.StoreOnlyNonEmptyData;
			if (DETaskGrabbingServerStoreOnlyNonEmptyDataSelectBox.SelectedItem != null)
			{
				storeOnlyNonEmptyData = bool.Parse((DETaskGrabbingServerStoreOnlyNonEmptyDataSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			bool flag2 = grabbingServerProperties.ExportDataOnline;
			if (DETaskGrabbingServerExportDataOnlineSelectBox.SelectedItem != null)
			{
				flag2 = bool.Parse((DETaskGrabbingServerExportDataOnlineSelectBox.SelectedItem as ComboBoxItem).Tag.ToString());
			}
			string text8 = grabbingServerProperties.ExportDataOnlineInvokationLink;
			if (flag2)
			{
				text8 = DETaskGrabbingServerGrabbingThreadExportDataOnlineHTTPLinkTextArea.Text.Trim();
			}
			GrabbingServerProperties grabberProperties = new GrabbingServerProperties(num2, result10, storeGrabbedData, storeOnlyNonEmptyData, flag2, text8);
			List<DataGrabbingPattern> list = new List<DataGrabbingPattern>();
			DataGrabbingPattern dataGrabbingPattern = new DataGrabbingPattern();
			list.Add(dataGrabbingPattern);
			string text9 = DEPatternAllowedPageUrlsSubstringsTextArea.Text.Trim();
			string[] allowedPageUrlsSubstrings = text9.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string text10 = DEPatternAllowedPageHTMLSubstringsTextArea.Text.Trim();
			string[] allowedPageHTMLSubstrings = text10.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			bool flag3 = false;
			DataGrabbingSelectorType selectorType = DataGrabbingSelectorType.CSS_Selector;
			string selector = "";
			dataGrabbingPattern.PatternName = "ScrapingPropertiesSet1";
			dataGrabbingPattern.AllowedPageUrlsSubstrings = allowedPageUrlsSubstrings;
			// dataGrabbingPattern.AllowedPageHTMLSubstrings = allowedPageHTMLSubstrings;
			if (flag3)
			{
				dataGrabbingPattern.OuterBlockSelector = new GrabberSelector(selector, selectorType);
			}
			DETaskNameTextArea.MarkAsCorrectlyCompleted();
			DETaskOperatingDirectoryTextArea.MarkAsCorrectlyCompleted();
			DEWebsiteRootUrlTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerRespectOnlySpecifiedUrlsTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerUrlSubstringsRestrictionsTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerCrawlingThreadsCountTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerSitemapReindexTimeDaysTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerPagesToCrawlLimitTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerPageDownloadAttemptsTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerLinksBufferHDDAutoSavingMillisecondsTextArea.MarkAsCorrectlyCompleted();
			DETaskCrawlingServerConcurrentCollectionsParallelismQuantityTextArea.MarkAsCorrectlyCompleted();
			DETaskGrabbingServerGrabbingThreadsCountTextArea.MarkAsCorrectlyCompleted();
			DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.MarkAsCorrectlyCompleted();
			DETaskGrabbingServerGrabbingThreadExportDataOnlineHTTPLinkTextArea.MarkAsCorrectlyCompleted();
			ParsingElementsScrollViewerContentPanel.MarkAsCorrectlyCompleted();
			bool flag4 = false;
			TabItem tabItem = null;
			int result11 = 0;
			if (DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerCrawlingThreadsCountTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerCrawlingThreadsCountTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerCrawlingThreadsCountTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerSitemapReindexTimeDaysTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerSitemapReindexTimeDaysTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerSitemapReindexTimeDaysTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerPagesToCrawlLimitTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerPagesToCrawlLimitTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerPagesToCrawlLimitTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerPageDownloadAttemptsTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerPageDownloadAttemptsTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerPageDownloadAttemptsTextArea.MarkAsUncorrectlyCompleted();
				tabItem = TabItemCrawlerProperties;
				flag4 = true;
			}
			if (DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerLinksBufferHDDAutoSavingMillisecondsTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerLinksBufferHDDAutoSavingMillisecondsTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerLinksBufferHDDAutoSavingMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskCrawlingServerConcurrentCollectionsParallelismQuantityTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskCrawlingServerConcurrentCollectionsParallelismQuantityTextArea.Text.Trim(), out result11))
			{
				DETaskCrawlingServerConcurrentCollectionsParallelismQuantityTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskGrabbingServerGrabbingThreadsCountTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskGrabbingServerGrabbingThreadsCountTextArea.Text.Trim(), out result11))
			{
				DETaskGrabbingServerGrabbingThreadsCountTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.Text.Trim().Length > 0 && !int.TryParse(DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.Text.Trim(), out result11))
			{
				DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			dataGrabbingPattern.GrabbingItemsPatterns = new List<DataGrabbingPatternItem>();
			for (int k = 0; k < ParsingElementsScrollViewerContentPanel.Children.Count; k++)
			{
				DEGrabberFlatItemSelector dEGrabberFlatItemSelector = ParsingElementsScrollViewerContentPanel.Children[k] as DEGrabberFlatItemSelector;
				dEGrabberFlatItemSelector.SelectorNumber.Text = "#" + (k + 1);
				DataGrabbingPatternItem dataGrabbingPatternItem = dEGrabberFlatItemSelector.TryGetPatternItemFromUIData();
				if (dataGrabbingPatternItem != null)
				{
					dataGrabbingPattern.GrabbingItemsPatterns.Add(dataGrabbingPatternItem);
					continue;
				}
				flag4 = true;
				tabItem = TabItemGrabberPatterns;
			}
			if (dataGrabbingPattern.GrabbingItemsPatterns.Count == 0)
			{
				ParsingElementsScrollViewerContentPanel.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemGrabberPatterns;
			}
			if (text == string.Empty)
			{
				DETaskNameTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCommonProperties;
			}
			if (text2 == string.Empty)
			{
				DETaskOperatingDirectoryTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCommonProperties;
			}
			if (text2 != string.Empty && !Directory.Exists(text2))
			{
				bool flag5 = false;
				try
				{
					DirectoryInfo folderFullAccessPermissions = Directory.CreateDirectory(text2);
					ExcavatorSharp.Common.IOCommon.SetFolderFullAccessPermissions(folderFullAccessPermissions);
					flag5 = true;
				}
				catch (Exception thrownException)
				{
					Logger.LogError("Error trying create directory in new project modal", thrownException);
				}
				if (!flag5)
				{
					DETaskOperatingDirectoryTextArea.MarkAsUncorrectlyCompleted();
					flag4 = true;
					tabItem = TabItemCommonProperties;
				}
			}
			if (!Uri.IsWellFormedUriString(text3, UriKind.Absolute))
			{
				DEWebsiteRootUrlTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCommonProperties;
			}
			if (!Uri.IsWellFormedUriString(text4, UriKind.Absolute))
			{
				DEWebsitePageToScrapeExample.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCommonProperties;
			}
			if (text4.IndexOf(text3) == -1)
			{
				DEWebsitePageToScrapeExample.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCommonProperties;
				System.Windows.MessageBox.Show($"The link to the sample page for scraping does not refer to the website address whose home page is listed.", "Example page to scrape error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			if (flag && (result5 < -1 || result5 == 0))
			{
				DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (num < 1)
			{
				DETaskCrawlingServerCrawlingThreadsCountTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (result < 1)
			{
				DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (text7.Length > 0 && (result6 < -1 || result6 == 0))
			{
				DETaskCrawlingServerSitemapReindexTimeDaysTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (result7 < -1 || result7 == 0)
			{
				DETaskCrawlingServerPagesToCrawlLimitTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (result2 < 1)
			{
				DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (result3 < 1)
			{
				DETaskCrawlingServerPageDownloadAttemptsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (result4 < 1)
			{
				DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (num2 < 1)
			{
				DETaskGrabbingServerGrabbingThreadsCountTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (result10 < 1)
			{
				DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			if (flag2 && !Uri.IsWellFormedUriString(text8, UriKind.Absolute))
			{
				DETaskGrabbingServerGrabbingThreadExportDataOnlineHTTPLinkTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
			}
			bool flag6 = false;
			LicenseKey actualLicenseKeyCopy = MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy();
			if (actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject != -1 && num > actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject)
			{
				DETaskCrawlingServerCrawlingThreadsCountTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
				flag6 = true;
			}
			if (actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject != -1 && num2 > actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject)
			{
				DETaskGrabbingServerGrabbingThreadsCountTextArea.MarkAsUncorrectlyCompleted();
				flag4 = true;
				tabItem = TabItemCrawlerProperties;
				flag6 = true;
			}
			if (flag6)
			{
				System.Windows.MessageBox.Show($"The license key you use is limited to {actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject} threads. Please, reduce the number of threads in the [Crawler settings tab] and the [Scraper settings tab].", "License limitation error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			if (flag4)
			{
				if (tabItem != null)
				{
					TaskPropertiesTabControl.SelectedItem = tabItem;
				}
				System.Windows.MessageBox.Show("Some of the settings fields have incorrect values. Please check all fields values and repeat saving.", "Fields validation error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return null;
			}
			for (int l = 0; l < MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks.Count; l++)
			{
				if (MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks[l].ProjectName == text && MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks[l].TaskLink.GetHashCode() != LinkedUITask.TaskLink.GetHashCode())
				{
					System.Windows.MessageBox.Show("Can't save the project. A project with the same name already exists.", "Error in project saving", MessageBoxButton.OK, MessageBoxImage.Hand);
					TaskPropertiesTabControl.SelectedItem = TabItemCommonProperties;
					DETaskNameTextArea.MarkAsUncorrectlyCompleted();
					return null;
				}
			}
			for (int m = 0; m < MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks.Count; m++)
			{
				if (MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks[m].ProjectPath == text2 && MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks[m].TaskLink.GetHashCode() != LinkedUITask.TaskLink.GetHashCode())
				{
					System.Windows.MessageBox.Show("Can't save the project. A project with the same directory already exists.", "Error in project saving", MessageBoxButton.OK, MessageBoxImage.Hand);
					TaskPropertiesTabControl.SelectedItem = TabItemCommonProperties;
					DETaskOperatingDirectoryTextArea.MarkAsUncorrectlyCompleted();
					return null;
				}
			}
			return new DEprojectCubeSettingsSet(text, text3, text4, taskDescription, list, crawlerProperties, grabberProperties, text2);
		}

		public void DropSettingToDefaults(bool ShowMessageBox = true, bool SetPatternsToDefault = false)
		{
			CrawlingServerProperties crawlerProperties = new CrawlingServerProperties();
			GrabbingServerProperties grabberProperties = new GrabbingServerProperties();
			List<DataGrabbingPattern> list = new List<DataGrabbingPattern>();
			if (SetPatternsToDefault)
			{
				list.Add(DataGrabbingPattern.GetDefaultPattern());
			}
			DEPatternAllowedPageUrlsSubstringsTextArea.Text = "*";
			SetPropertiesToSpecified(crawlerProperties, grabberProperties, list);
			if (ShowMessageBox)
			{
				System.Windows.MessageBox.Show("The parameters have been reset to their default values.", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
		}

		private void SetPropertiesToSpecified(CrawlingServerProperties CrawlerProperties, GrabbingServerProperties GrabberProperties, List<DataGrabbingPattern> GrabbingPatterns)
		{
			DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox.SelectedItem = EnumToItemsSource.GetDescriptionFromValue(CrawlerProperties.PrimaryDataCrawlingWay);
			DETaskCrawlingServerRespectOnlySpecifiedUrlsTextArea.Text = string.Join(", ", (CrawlerProperties.RespectOnlySpecifiedUrls != null) ? CrawlerProperties.RespectOnlySpecifiedUrls : new string[0]);
			DETaskCrawlingServerUrlSubstringsRestrictionsTextArea.Text = string.Join(", ", (CrawlerProperties.UrlSubstringsRestrictions != null) ? CrawlerProperties.UrlSubstringsRestrictions : new string[0]);
			DETaskCrawlingServerCrawlingThreadsCountTextArea.Text = CrawlerProperties.CrawlingThreadsCount.ToString();
			ApplicationSpeedBar.Value = CrawlerProperties.CrawlingThreadsCount;
			DETaskCrawlingServerCrawlingThreadDelayMillisecondsTextArea.Text = CrawlerProperties.CrawlingThreadDelayMilliseconds.ToString();
			DETaskCrawlingServerCrawlerUserAgentTextArea.Text = CrawlerProperties.CrawlerUserAgent;
			DETaskCrawlingServerHttpWebRequestMethodSelectBox.SelectItemByTag(CrawlerProperties.HttpWebRequestMethod);
			DETaskCrawlingServerPageDownloadTimeoutMillisecondsTextArea.Text = CrawlerProperties.PageDownloadTimeoutMilliseconds.ToString();
			DETaskCrawlingServerPageDownloadAttemptsTextArea.Text = CrawlerProperties.PageDownloadAttempts.ToString();
			DETaskCrawlingServerCrawlWebsiteLinksSelectBox.SelectItemByTag(CrawlerProperties.CrawlWebsiteLinks);
			DETaskCrawlingServerStoreCrawledDataSelectBox.SelectItemByTag(CrawlerProperties.StoreCrawledData);
			DETaskCrawlingServerStoreCrawledPagesHTMLSourceSelectBox.SelectItemByTag(CrawlerProperties.StoreCrawledPagesHTMLSource);
			DETaskCrawlingServerSReindexCrawledPagesSourceSelectBox.SelectItemByTag(CrawlerProperties.ReindexCrawledPages);
			DETaskCrawlingServerReindexCrawledPagesAfterSpecifiedMinutesIntervalTextArea.Text = CrawlerProperties.ReindexCrawledPagesAfterSpecifiedMinutesInterval.ToString();
			DETaskCrawlingServerRespectRobotsTxtFileSelectBox.SelectItemByTag(CrawlerProperties.RespectRobotsTxtFile);
			DETaskCrawlingServerRobotsTxtUserAgentRespectationChainTextArea.Text = string.Join(",", CrawlerProperties.RobotsTxtUserAgentRespectationChain);
			DETaskCrawlingServerRobotsTxtReindexTimeDaysTextArea.Text = CrawlerProperties.RobotsTxtReindexTimeDays.ToString();
			DETaskCrawlingServerSitemapUrlTextArea.Text = CrawlerProperties.SitemapUrl;
			DETaskCrawlingServerSitemapReindexTimeDaysTextArea.Text = CrawlerProperties.SitemapReindexTimeDays.ToString();
			DETaskCrawlingServerPagesToCrawlLimitTextArea.Text = CrawlerProperties.PagesToCrawlLimit.ToString();
			DETaskCrawlingServerProxiesRotationSelectBox.SelectedItem = EnumToItemsSource.GetDescriptionFromValue(CrawlerProperties.ProxiesRotation);
			DETaskCrawlingServerLinksBufferHDDAutoSavingMillisecondsTextArea.Text = CrawlerProperties.LinksBufferHDDAutoSavingMilliseconds.ToString();
			DETaskCrawlingServerConcurrentCollectionsParallelismQuantityTextArea.Text = CrawlerProperties.ConcurrentCollectionsParallelismQuantity.ToString();
			DETaskCrawlingServerExpandIFramesSelectBox.SelectItemByTag(CrawlerProperties.ExpandPageFrames);
			DETaskGrabbingServerGrabbingThreadsCountTextArea.Text = CrawlerProperties.CrawlingThreadsCount.ToString();
			DETaskGrabbingServerGrabbingThreadDelayMillisecondsTextArea.Text = GrabberProperties.GrabbingThreadDelayMilliseconds.ToString();
			DETaskGrabbingServerStoreGrabbedDataSelectBox.SelectItemByTag(GrabberProperties.StoreGrabbedData);
			DETaskGrabbingServerStoreOnlyNonEmptyDataSelectBox.SelectItemByTag(GrabberProperties.StoreOnlyNonEmptyData);
			DETaskGrabbingServerExportDataOnlineSelectBox.SelectItemByTag(GrabberProperties.ExportDataOnline);
			DETaskGrabbingServerGrabbingThreadExportDataOnlineHTTPLinkTextArea.Text = GrabberProperties.ExportDataOnlineInvokationLink;
			ActualProxiesList = CrawlerProperties.HTTPWebRequestProxiesList;
			ActualCrawlingArgumentsList = CrawlerProperties.CrawlingRequestAdditionalParamsList;
			ActualCEFCrawlingBehaviors = CrawlerProperties.CEFCrawlingBehaviors;
			ActualCAPTCHAResolverSettings = CrawlerProperties.CaptchaSettings;
			if (ActualCEFCrawlingBehaviors == null)
			{
				ActualCEFCrawlingBehaviors = new List<CEFCrawlingBehavior>();
			}
			WebsiteAuthenticaionBehavior = CrawlerProperties.CEFWebsiteAuthenticationBehavior;
			if (ActualProxiesList != null && ActualProxiesList.Count > 0)
			{
				DETaskCrawlingServer_HTTPWebRequestProxiesList_TextBlock.Text = $"{ActualProxiesList.Count} proxies defined";
			}
			else
			{
				DETaskCrawlingServer_HTTPWebRequestProxiesList_TextBlock.Text = string.Empty;
			}
			if (ActualCrawlingArgumentsList != null && ActualCrawlingArgumentsList.Count > 0)
			{
				DETaskCrawlingServer_CrawlingArguments_TextBlock.Text = $"{ActualCrawlingArgumentsList.Count} parameters defined";
			}
			else
			{
				DETaskCrawlingServer_CrawlingArguments_TextBlock.Text = string.Empty;
			}
			if (ActualCEFCrawlingBehaviors != null && ActualCEFCrawlingBehaviors.Count > 0)
			{
				DETaskCrawlingServer_CEFCrawlingBehaviors_TextBlock.Text = string.Format("JS behavior specified", ActualCEFCrawlingBehaviors.Count);
			}
			else
			{
				DETaskCrawlingServer_CEFCrawlingBehaviors_TextBlock.Text = string.Empty;
			}
			if (WebsiteAuthenticaionBehavior != null)
			{
				DETaskCrawlingServer_CEFAuthenticationBehavior_TextBlock.Text = "Authentication is used";
			}
			else
			{
				DETaskCrawlingServer_CEFAuthenticationBehavior_TextBlock.Text = "";
			}
			if (ActualCAPTCHAResolverSettings != null)
			{
				DETaskCrawlingServer_CEFCaptchaBehavior_TextBlock.Text = "CAPTCHA resolver specified";
			}
			else
			{
				DETaskCrawlingServer_CEFCaptchaBehavior_TextBlock.Text = "";
			}
			ParsingElementsScrollViewerContentPanel.Children.Clear();
			if (GrabbingPatterns.Count > 0)
			{
				DataGrabbingPattern dataGrabbingPattern = GrabbingPatterns[0];
				if (dataGrabbingPattern.AllowedPageUrlsSubstrings != null)
				{
					DEPatternAllowedPageUrlsSubstringsTextArea.Text = string.Join(",", dataGrabbingPattern.AllowedPageUrlsSubstrings);
				}
				// if (dataGrabbingPattern.AllowedPageHTMLSubstrings != null)
				// {
				// 	DEPatternAllowedPageHTMLSubstringsTextArea.Text = string.Join(",", dataGrabbingPattern.AllowedPageHTMLSubstrings);
				// }
				for (int i = 0; i < dataGrabbingPattern.GrabbingItemsPatterns.Count; i++)
				{
					DEGrabberFlatItemSelector dEGrabberFlatItemSelector = new DEGrabberFlatItemSelector(this, dataGrabbingPattern.GrabbingItemsPatterns[i]);
					dEGrabberFlatItemSelector.SelectorNumber.Text = "#" + (i + 1);
					ParsingElementsScrollViewerContentPanel.Children.Add(dEGrabberFlatItemSelector);
				}
			}
		}

		private void SelectProjectDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			DialogResult dialogResult = folderBrowserDialog.ShowDialog();
			if (dialogResult == System.Windows.Forms.DialogResult.OK)
			{
				DETaskOperatingDirectoryTextArea.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void RollProjectDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			if (DETaskNameTextArea.Text.Trim().Length > 0)
			{
				DETaskOperatingDirectoryTextArea.Text = RollProjectDefaultPath(DETaskNameTextArea.Text.Trim());
			}
			else
			{
				System.Windows.MessageBox.Show("To roll a directory, you must first enter the project name", "Information", MessageBoxButton.OK);
			}
		}

		private void SaveProjectPropertiesButton_Click(object sender, RoutedEventArgs e)
		{
			SaveSettings();
		}

		private void EditButton_HTTPWebRequestProxiesList_Click(object sender, RoutedEventArgs e)
		{
			if (EditProxiesListModal != null)
			{
				EditProxiesListModal.Focus();
				return;
			}
			DataCrawlingWebProxy item = new DataCrawlingWebProxy("127.0.0.1", 80);
			DataCrawlingWebProxy item2 = new DataCrawlingWebProxy("127.0.0.1", 88);
			string arg = JsonConvert.SerializeObject(new List<DataCrawlingWebProxy> { item, item2 }, Formatting.Indented);
			string exampleTextAndComment = string.Format("{0}\r\n{1}", "/** You can define a list of proxy servers for crawling data. Please, use next data format: **/", arg);
			string actualPropertiesSet = ((ActualProxiesList != null && ActualProxiesList.Count > 0) ? JsonConvert.SerializeObject(ActualProxiesList, Formatting.Indented) : string.Empty);
			string modalTitle = "Edit proxy-servers list";
			if (DETaskNameTextArea.Text.Trim().Length > 0)
			{
				modalTitle = $"{DETaskNameTextArea.Text} → Edit proxy-servers list";
			}
			EditProxiesListModal = new DEProjectEntityJSONProperties(modalTitle, exampleTextAndComment, actualPropertiesSet, this, delegate(string ApplyPropertiesResults, ContentControl ParentWindowLink)
			{
				DEProjectCubeProperties dEProjectCubeProperties = ParentWindowLink as DEProjectCubeProperties;
				ApplyPropertiesResults = ApplyPropertiesResults.Trim();
				if (ApplyPropertiesResults == string.Empty)
				{
					dEProjectCubeProperties.ActualProxiesList = new List<DataCrawlingWebProxy>();
				}
				else
				{
					dEProjectCubeProperties.ActualProxiesList = JsonConvert.DeserializeObject<List<DataCrawlingWebProxy>>(ApplyPropertiesResults);
				}
				if (dEProjectCubeProperties.ActualProxiesList != null && dEProjectCubeProperties.ActualProxiesList.Count > 0)
				{
					dEProjectCubeProperties.DETaskCrawlingServer_HTTPWebRequestProxiesList_TextBlock.Text = $"{dEProjectCubeProperties.ActualProxiesList.Count} proxies defined";
				}
				else
				{
					dEProjectCubeProperties.DETaskCrawlingServer_HTTPWebRequestProxiesList_TextBlock.Text = string.Empty;
				}
				for (int i = 0; i < dEProjectCubeProperties.ActualProxiesList.Count; i++)
				{
					dEProjectCubeProperties.ActualProxiesList[i].InitializeProxy();
				}
				dEProjectCubeProperties.EditProxiesListModal.Close();
			}, delegate(string PropertiesToValidate)
			{
				PropertiesToValidate = PropertiesToValidate.Trim();
				if (!(PropertiesToValidate == string.Empty))
				{
					try
					{
						List<DataCrawlingWebProxy> list = JsonConvert.DeserializeObject<List<DataCrawlingWebProxy>>(PropertiesToValidate);
						return true;
					}
					catch (Exception thrownException)
					{
						Logger.LogError("Error during proxy servers deserializing", thrownException);
						return false;
					}
				}
				return true;
			});
			EditProxiesListModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			EditProxiesListModal.Closed += EditProxiesListModal_Closed;
			ShowShadowOverlay(EditProxiesListModal);
			EditProxiesListModal.Show();
		}

		private void EditProxiesListModal_Closed(object sender, EventArgs e)
		{
			EditProxiesListModal = null;
			HideShadowOverlay();
		}

		private void EditButton_CrawlingArguments_Click(object sender, RoutedEventArgs e)
		{
			if (EditCrawlingArgumentsModal != null)
			{
				EditCrawlingArgumentsModal.Focus();
				return;
			}
			Dictionary<string, string> value = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "argument2", "123" },
				{ "param3", "abcde" }
			};
			string arg = JsonConvert.SerializeObject(value, Formatting.Indented);
			string exampleTextAndComment = string.Format("{0}\r\n{1}", "/** You can define a set of $_GET or $_POST parameters that will be used to crawl each page **/", arg);
			string actualPropertiesSet = ((ActualCrawlingArgumentsList != null && ActualCrawlingArgumentsList.Count > 0) ? JsonConvert.SerializeObject(ActualCrawlingArgumentsList, Formatting.Indented) : string.Empty);
			string modalTitle = "Edit crawling arguments";
			if (DETaskNameTextArea.Text.Trim().Length > 0)
			{
				modalTitle = $"{DETaskNameTextArea.Text.Trim()} → Edit crawling arguments";
			}
			EditCrawlingArgumentsModal = new DEProjectEntityJSONProperties(modalTitle, exampleTextAndComment, actualPropertiesSet, this, delegate(string DefinedArgumentsList, ContentControl ParentWindowLink)
			{
				DEProjectCubeProperties dEProjectCubeProperties = ParentWindowLink as DEProjectCubeProperties;
				DefinedArgumentsList = DefinedArgumentsList.Trim();
				if (DefinedArgumentsList == string.Empty)
				{
					dEProjectCubeProperties.ActualCrawlingArgumentsList = new Dictionary<string, string>();
				}
				else
				{
					dEProjectCubeProperties.ActualCrawlingArgumentsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(DefinedArgumentsList);
				}
				if (ActualCrawlingArgumentsList != null && dEProjectCubeProperties.ActualCrawlingArgumentsList.Count > 0)
				{
					dEProjectCubeProperties.DETaskCrawlingServer_CrawlingArguments_TextBlock.Text = $"{dEProjectCubeProperties.ActualCrawlingArgumentsList.Count} parameters defined";
				}
				else
				{
					dEProjectCubeProperties.DETaskCrawlingServer_CrawlingArguments_TextBlock.Text = string.Empty;
				}
				dEProjectCubeProperties.EditCrawlingArgumentsModal.Close();
			}, delegate(string DataToValidate)
			{
				DataToValidate = DataToValidate.Trim();
				if (!(DataToValidate == string.Empty))
				{
					try
					{
						Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(DataToValidate);
						return true;
					}
					catch (Exception thrownException)
					{
						Logger.LogError("Error in deserializing of arguments", thrownException);
						return false;
					}
				}
				return true;
			});
			EditCrawlingArgumentsModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			EditCrawlingArgumentsModal.Closed += EditCrawlingArgumentsModal_Closed;
			ShowShadowOverlay(EditCrawlingArgumentsModal);
			EditCrawlingArgumentsModal.Show();
		}

		private void EditCrawlingArgumentsModal_Closed(object sender, EventArgs e)
		{
			EditCrawlingArgumentsModal = null;
			HideShadowOverlay();
		}

		private void EditButton_CEFCrawlingBehaviors_Click(object sender, RoutedEventArgs e)
		{
			if (EditCEFBehaviorModal != null)
			{
				EditCEFBehaviorModal.Focus();
			}
			CEFCrawlingBehavior behaviorLink = null;
			if (ActualCEFCrawlingBehaviors.Count > 0)
			{
				behaviorLink = ActualCEFCrawlingBehaviors[0];
			}
			EditCEFBehaviorModal = new DECEFBehaviorEditModal(this, behaviorLink);
			EditCEFBehaviorModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			EditCEFBehaviorModal.Closed += EditCEFBehaviorModal_Closed;
			ShowShadowOverlay(EditCEFBehaviorModal);
			EditCEFBehaviorModal.Show();
		}

		private void EditCEFBehaviorModal_Closed(object sender, EventArgs e)
		{
			EditCEFBehaviorModal = null;
			HideShadowOverlay();
		}

		private void EditButton_CEFAuthenticationBehavior_Click(object sender, RoutedEventArgs e)
		{
			if (EnumToItemsSource.GetValueFromDescription<DataCrawlingType>(DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox.SelectedItem.ToString()) == DataCrawlingType.NativeCrawling)
			{
				System.Windows.MessageBox.Show("You can edit CEF authentication behavior only when the «Crawler engine» setted to CEFCrawling", "You can't edit the CEF auth behavior", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				return;
			}
			if (WebsiteAuthenticationBehaviorModalWindow != null)
			{
				WebsiteAuthenticationBehaviorModalWindow.Focus();
				return;
			}
			WebsiteAuthenticationBehaviorModalWindow = new DEWebsiteAuthBehavior(this, WebsiteAuthenticaionBehavior);
			if (WebsiteAuthenticaionBehavior == null)
			{
				WebsiteAuthenticationBehaviorModalWindow.TransferLinksFromCommonCrawlingProperties(DETaskCrawlingServerRespectOnlySpecifiedUrlsTextArea.Text, DETaskCrawlingServerUrlSubstringsRestrictionsTextArea.Text);
			}
			WebsiteAuthenticationBehaviorModalWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			WebsiteAuthenticationBehaviorModalWindow.Closed += WebsiteAuthenticationBehaviorModalWindow_Closed;
			ShowShadowOverlay(WebsiteAuthenticationBehaviorModalWindow);
			WebsiteAuthenticationBehaviorModalWindow.Show();
		}

		private void WebsiteAuthenticationBehaviorModalWindow_Closed(object sender, EventArgs e)
		{
			WebsiteAuthenticationBehaviorModalWindow = null;
			HideShadowOverlay();
		}

		private void EditButton_CEFCAPTCHABehavior_Click(object sender, RoutedEventArgs e)
		{
			if (EnumToItemsSource.GetValueFromDescription<DataCrawlingType>(DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox.SelectedItem.ToString()) == DataCrawlingType.NativeCrawling)
			{
				System.Windows.MessageBox.Show("You can edit CAPTCHA solver settings only when the «Crawler engine» setted to CEFCrawling", "You can't edit the CAPTCHA solver settings", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				return;
			}
			if (EditCaptchaBehaviorModal != null)
			{
				EditCaptchaBehaviorModal.Focus();
				return;
			}
			EditCaptchaBehaviorModal = new DECaptchaBehavior(this, ActualCAPTCHAResolverSettings);
			EditCaptchaBehaviorModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			EditCaptchaBehaviorModal.Closed += EditCaptchaBehaviorModal_Closed;
			ShowShadowOverlay(EditCaptchaBehaviorModal);
			EditCaptchaBehaviorModal.Show();
		}

		private void EditCaptchaBehaviorModal_Closed(object sender, EventArgs e)
		{
			EditCaptchaBehaviorModal = null;
			HideShadowOverlay();
		}

		private void DropSettingToDetaultsButton_Click(object sender, RoutedEventArgs e)
		{
			DropSettingToDefaults(ShowMessageBox: false, SetPatternsToDefault: true);
		}

		private void ExportSettingButton_Click(object sender, RoutedEventArgs e)
		{
			if (LinkedUITask == null)
			{
				System.Windows.MessageBox.Show("You can't export the project before you will save it first time. Save the project and try to export again.", "Exporting error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "Export setting into file";
			saveFileDialog.Filter = "JSON files (*.json)|*.json";
			saveFileDialog.FileName = $"{TextTransliterations.TransliterateFromCyryllicToLatin(LinkedUITask.ProjectName)} exported.json";
			DialogResult dialogResult = saveFileDialog.ShowDialog();
			if (dialogResult == System.Windows.Forms.DialogResult.OK)
			{
				DataExcavatorTaskIO dataExcavatorTaskIO = new DataExcavatorTaskIO();
				dataExcavatorTaskIO.ExportDETaskIntoFile(saveFileDialog.FileName, LinkedUITask.TaskLink);
			}
		}

		public void LoadProjectDataFromImportFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Import setting rom file";
			openFileDialog.Filter = "JSON files (*.json)|*.json";
			DialogResult dialogResult = openFileDialog.ShowDialog();
			if (dialogResult == System.Windows.Forms.DialogResult.OK)
			{
				DataExcavatorTaskIO dataExcavatorTaskIO = new DataExcavatorTaskIO();
				DataExcavatorTaskExportContainer dataExcavatorTaskExportContainer = null;
				try
				{
					dataExcavatorTaskExportContainer = dataExcavatorTaskIO.ImportDETaskFromFile(openFileDialog.FileName);
				}
				catch (IOException ex)
				{
					Logger.LogError("Error in import of task properties - File reading error.", ex);
					App.TrySendAppCrashReport(ex, "Error in project settings import");
					System.Windows.MessageBox.Show("Error in import of task properties - File reading error.", "Import error", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				catch (Exception ex2)
				{
					Logger.LogError("Error in import of task properties - File data is corrupted.", ex2);
					App.TrySendAppCrashReport(ex2, "Error in project settings import");
					System.Windows.MessageBox.Show("Error in import of task properties - File data is corrupted.", "Import error", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				if (dataExcavatorTaskExportContainer.CrawlerPeroperties == null || dataExcavatorTaskExportContainer.GrabberProperties == null || dataExcavatorTaskExportContainer.GrabbingPatterns == null)
				{
					System.Windows.MessageBox.Show("Error in import of task properties - File data is corrupted.", "Import error", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				MainWindowLink.ShowMainWindowShadowOverlay(this);
				string text = RollProjectDefaultPath(dataExcavatorTaskExportContainer.ProjectName);
				string text2 = MainWindowLink.RollNewProjectName(dataExcavatorTaskExportContainer.ProjectName);
				DETaskNameTextArea.Text = text2;
				DEWebsiteRootUrlTextArea.Text = dataExcavatorTaskExportContainer.WebsiteRootUrl;
				//DEWebsitePageToScrapeExample.Text = dataExcavatorTaskExportContainer.WebsitePageToScrapeExampleUrl;
				//PICKNODES_PageUrlTextBlock.Text = dataExcavatorTaskExportContainer.WebsitePageToScrapeExampleUrl;
				//ExamplePageOverlayStartModalTextBox.Text = dataExcavatorTaskExportContainer.WebsitePageToScrapeExampleUrl;
				DETaskOperatingDirectoryTextArea.Text = text;
				SetPropertiesToSpecified(dataExcavatorTaskExportContainer.CrawlerPeroperties, dataExcavatorTaskExportContainer.GrabberProperties, dataExcavatorTaskExportContainer.GrabbingPatterns);
			}
			else
			{
				isProjectImportCancelled = true;
			}
		}

		private void ImportSettingButton_Click(object sender, RoutedEventArgs e)
		{
			LoadProjectDataFromImportFile();
		}

		private void DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (EnumToItemsSource.GetValueFromDescription<DataCrawlingType>(DETaskCrawlingServerPrimaryDataCrawlingWaySelectBox.SelectedItem.ToString()) == DataCrawlingType.NativeCrawling)
			{
				if (ActualCEFCrawlingBehaviors != null)
				{
					ActualCEFCrawlingBehaviors.Clear();
				}
				DETaskCrawlingServer_CEFCrawlingBehaviors_TextBlock.Text = string.Empty;
				WebsiteAuthenticaionBehavior = null;
				DETaskCrawlingServer_CEFAuthenticationBehavior_TextBlock.Text = string.Empty;
				DETaskCrawlingServerExpandIFramesSelectBox.SelectedItem = null;
				DETaskCrawlingServerExpandIFramesSelectBox.IsEnabled = false;
				DETaskCrawlingServerExpandIFramesSelectBox.IsReadOnly = true;
			}
			else
			{
				DETaskCrawlingServerExpandIFramesSelectBox.IsEnabled = true;
				DETaskCrawlingServerExpandIFramesSelectBox.IsReadOnly = false;
			}
		}

		private void TestProjectSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			// if (MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() == null || (MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() != null && !MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().IsKeyDateValidAndNonOutdated()))
			// {
			// 	System.Windows.MessageBox.Show("License error. Please, use demo or buy a license key.", "License error", MessageBoxButton.OK, MessageBoxImage.Hand);
			// 	return;
			// }
			DEprojectCubeSettingsSet dEprojectCubeSettingsSet = ValidateSettingsAndGetAssembledSettingsContainer();
			if (dEprojectCubeSettingsSet != null)
			{
				DataExcavatorTask excavatorTaskLink = new DataExcavatorTask(dEprojectCubeSettingsSet.TaskName, dEprojectCubeSettingsSet.WebsiteRootUrl, dEprojectCubeSettingsSet.TaskDescription, dEprojectCubeSettingsSet.GrabbingPatternsList, dEprojectCubeSettingsSet.CrawlerProperties, dEprojectCubeSettingsSet.GrabberProperties, dEprojectCubeSettingsSet.TaskOperatingDirectory);
				ProjectCubeTestURLContainer.LastTestingURL = dEprojectCubeSettingsSet.WebsitePageToScrapeExampleUrl;
				DETestTaskSettings dETestTaskSettings = new DETestTaskSettings(excavatorTaskLink, ProjectCubeTestURLContainer);
				dETestTaskSettings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				dETestTaskSettings.Closed += TestSettingsModal_Closed;
				ShowShadowOverlay(dETestTaskSettings);
				dETestTaskSettings.Show();
			}
		}

		private void TestSettingsModal_Closed(object sender, EventArgs e)
		{
			HideShadowOverlay();
		}

		public void CascadeCloseChildren()
		{
			if (EditProxiesListModal != null)
			{
				EditProxiesListModal.Close();
			}
			if (EditCrawlingArgumentsModal != null)
			{
				EditCrawlingArgumentsModal.Close();
			}
			if (EditCEFBehaviorModal != null)
			{
				EditCEFBehaviorModal.Close();
			}
			if (WebsiteAuthenticationBehaviorModalWindow != null)
			{
				WebsiteAuthenticationBehaviorModalWindow.Close();
			}
		}

		private void AddNodeToScrapeButton_Click(object sender, RoutedEventArgs e)
		{
			int count = ParsingElementsScrollViewerContentPanel.Children.Count;
			DEGrabberFlatItemSelector dEGrabberFlatItemSelector = new DEGrabberFlatItemSelector(this);
			dEGrabberFlatItemSelector.SelectorNumber.Text = "#" + (count + 1);
			ParsingElementsScrollViewerContentPanel.Children.Add(dEGrabberFlatItemSelector);
			ParsingElementsScrollViewer.ScrollToBottom();
			dEGrabberFlatItemSelector.ScrapingNodeSelectorType.SelectedItem = EnumToItemsSource.GetDescriptionFromValue(DataGrabbingSelectorType.CSS_Selector);
			UpdateDataToScrapeNodesCount();
		}

		public void HandleCSSSelectorRemovedFromNodesList(string SelectorGUID)
		{
			if (PageNodesPicker != null)
			{
				PageNodesPicker.HandleSelectorRemovedFromUI(SelectorGUID);
			}
		}

		public void HandleCSSSelectorNameUpdatedFromUINodesList(string SelectorGUID, string SelectorNewName)
		{
			if (PageNodesPicker != null)
			{
				PageNodesPicker.HandleNodeNameChangedFromUI(SelectorGUID, SelectorNewName);
			}
		}

		public void HandleCSSSelectorUpdatedFromUINodesList()
		{
			if (PageNodesPicker != null)
			{
				NodePickerOverlay.Visibility = Visibility.Visible;
				NodePicker_LoadPageToPickNodes.Visibility = Visibility.Hidden;
				NodePicker_ReloadPageToUpdateNodes.Visibility = Visibility.Visible;
			}
		}

		private void HelpWindow_Closed(object sender, EventArgs e)
		{
			helpWindow.Closed -= HelpWindow_Closed;
			HideShadowOverlay();
			helpWindow = null;
		}

		private void HelpButton_TaskDirectory_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("ProjectTaskDirectory");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_PrimaryCrawlingWay_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("PrimaryCrawlingWay");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_RobotsTxtUserAgentRespectationChain_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("RobotsTxtUserAgentRespectationChain");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_CrawlingThreadsCount_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CrawlingThreadsCount");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_CrawlingThreadDelayMilliseconds_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CrawlingThreadDelayMilliseconds");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_PagesToCrawlLimit_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("PagesToCrawlLimit");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_PageDownloadTimeoutMilliseconds_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("PageDownloadTimeoutMilliseconds");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButtonPageDownloadAttempts_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("PageDownloadAttempts");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_LinksBufferHDDAutoSavingMilliseconds_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("LinksBufferHDDAutoSavingMilliseconds");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_ConcurrentCollectionsParallelismQuantity_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("ConcurrentCollectionsParallelismQuantity");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_HTTPWebRequestProxiesList_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("HTTPWebRequestProxiesList");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_ProxiesRotation_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CrawlingServerProxiesRotation");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_CEFCrawlingBehaviors_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CEFCrawlingBehaviors");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_CrawlingArguments_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CrawlingArguments");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_GrabbingThreadsCount_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("GrabbingThreadsCount");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_CEFAuthenticationBehavior_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthBehavior");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_CEFCAPTCHABehavior_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("CAPTCHABehavior");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_PatternPagesUrlsSubstrings_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("PatternPagesUrlsSubstrings");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_OuterSelector_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("ParsingPatternOuterSelector");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_ExpandIFrames_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("ExpandIFramesSelectBox");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		private void HelpButton_PatternPagesHTMLSubstrings_Click(object sender, RoutedEventArgs e)
		{
			helpWindow = HelpingButtonsDataStorage.ShowHelpWindow("PageToScrapeHTMLMask");
			helpWindow.Closed += HelpWindow_Closed;
			ShowShadowOverlay(helpWindow);
		}

		public void UpdateNodesToScrapeCount()
		{
			base.Dispatcher.Invoke(delegate
			{
				int count = ParsingElementsScrollViewerContentPanel.Children.Count;
			});
		}

		public void ShowShadowOverlay(Window openedWindow)
		{
			overlayController.ShowOverlay(openedWindow);
		}

		public void HideShadowOverlay()
		{
			overlayController.HideOverlay();
		}

		private void DETaskNameTextArea_LostFocus(object sender, RoutedEventArgs e)
		{
			if (DETaskOperatingDirectoryTextArea.Text.Trim().Length == 0 && DETaskNameTextArea.Text.Trim().Length > 0)
			{
				DETaskOperatingDirectoryTextArea.Text = RollProjectDefaultPath(DETaskNameTextArea.Text.Trim());
			}
		}

		private void PICKNODES_NavigateButton_Click(object sender, RoutedEventArgs e)
		{
			LoadDemoPageAndSetupSelectors();
		}

		private void PICKNODES_ReloadButton_Click(object sender, RoutedEventArgs e)
		{
			LoadDemoPageAndSetupSelectors();
		}

		private void LoadDemoPageAndSetupSelectors()
		{
			PICKNODES_PageUrlTextBlock.MarkAsCorrectlyCompleted();
			string text = DEWebsiteRootUrlTextArea.Text.Trim();
			string text2 = PICKNODES_PageUrlTextBlock.Text.Trim();
			if (text2 == string.Empty || !Uri.IsWellFormedUriString(text2, UriKind.Absolute))
			{
				PICKNODES_PageUrlTextBlock.MarkAsUncorrectlyCompleted();
				System.Windows.MessageBox.Show("Uncorrect demo page URL. Please, complete demo page width valid URL", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else if (text2.IndexOf(text) == -1)
			{
				PICKNODES_PageUrlTextBlock.MarkAsUncorrectlyCompleted();
				System.Windows.MessageBox.Show($"Entered demo page is not equal to the website domain (Root = {text} || Demo = {text2})", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else
			{
				PageNodesPicker = new ChromiumJSNodesPicker(MainWindowLink, this, ProjectSettingsPage_PickNodesWebBrowser, ParsingElementsScrollViewerContentPanel, PICKNODES_PageUrlTextBlock.Text);
				PageNodesPicker.NavigateAndSetupPickerWithPickedNodes();
			}
		}

		private void TaskPropertiesTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (TaskPropertiesTabControl.SelectedIndex == 1)
			{
				PICKNODES_PageUrlTextBlock.Text = DEWebsitePageToScrapeExample.Text;
				ExamplePageOverlayStartModalTextBox.Text = DEWebsitePageToScrapeExample.Text;
			}
		}

		private void NavigateSamplePageStartButton_Click(object sender, RoutedEventArgs e)
		{
			NavigateDemoPageAndSetupSelectorsFunc();
		}

		private void NavigateDemoPageAndSetupSelectorsFunc()
		{
			ExamplePageOverlayStartModalTextBox.MarkAsCorrectlyCompleted();
			string text = ExamplePageOverlayStartModalTextBox.Text;
			if (text == string.Empty || !Uri.IsWellFormedUriString(text, UriKind.Absolute))
			{
				ExamplePageOverlayStartModalTextBox.MarkAsUncorrectlyCompleted();
				System.Windows.MessageBox.Show("Uncorrect demo page URL. Please, complete demo page width valid URL", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else
			{
				PICKNODES_PageUrlTextBlock.Text = text;
				NodePickerOverlay.Visibility = Visibility.Collapsed;
				LoadDemoPageAndSetupSelectors();
			}
		}

		private void NavigateSamplePageReloadPageButton_Click(object sender, RoutedEventArgs e)
		{
			if (PageNodesPicker != null)
			{
				PageNodesPicker.ReAddPickedNodes();
			}
		}

		private void PICKNODES_ReloadCSSSelectorsButton_Click(object sender, RoutedEventArgs e)
		{
			if (PageNodesPicker != null)
			{
				PageNodesPicker.ReAddPickedNodes();
			}
		}

		private void WebsiteOpenHomeURLButton_Click(object sender, RoutedEventArgs e)
		{
			string text = DEWebsiteRootUrlTextArea.Text.Trim();
			if (text != string.Empty)
			{
				Process.Start(text);
			}
		}

		private void WebsiteOpenPageToScrapeURLButton_Click(object sender, RoutedEventArgs e)
		{
			string text = DEWebsitePageToScrapeExample.Text.Trim();
			if (text != string.Empty)
			{
				Process.Start(text);
			}
		}

		private void ExamplePageOverlayStartModalTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				NavigateDemoPageAndSetupSelectorsFunc();
			}
		}

	}
}