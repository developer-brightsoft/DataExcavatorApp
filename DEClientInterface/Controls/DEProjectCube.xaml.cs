using DEClientInterface.Logic;
using DEClientInterface.Objects;
using DEClientInterface.UIExtensions;
using DEClientInterface.UIWindows;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Licensing;
using ExcavatorSharp.Objects;
using FontAwesome5.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEClientInterface.Controls
{
    /// <summary>
    /// Interaction logic for DEProjectCube.xaml
    /// </summary>
    public partial class DEProjectCube : UserControl
    {
        internal DEProjectTestingUrlContainer ProjectCubeTestURLContainer = new DEProjectTestingUrlContainer();

        private DEAddLinksToCrawling addLinksToCrawlingModal = null;

        private DEProjectCubeProperties projectCubePropertiesModal = null;

        private DELogs LogsModalLink = null;

        private DEGrabbingResultsOverview grabbingResultsOverviewModal = null;

        private DELinksBufferOverview linksBufferOverview = null;

        private DEPageLinksObserving linksObservingModalWindow = null;

        private DEProjectCubeProperties copyTaskSettingsModal = null;

        private DEChooseWebsiteLinksCrawling chooseWebsiteLinksCrawlingWay = null;

        private DETestTaskSettings testTaskSettingsModal = null;

        private DEObserveAnyWebsitePageWindow observeWebsitePageModal = null;

        private Thread CountersRefreshingThread = null;

        private bool CubeActionsMutexEmulator = false;

        public DEProjectLink DataExcavatorUIProjectLink { get; set; }

        internal MainWindow ParentWindowLink { get; set; }

        public DEProjectCube(MainWindow ParentWindowLink, DEProjectLink ParentTaskLink)
        {
            DataExcavatorUIProjectLink = ParentTaskLink;
            this.ParentWindowLink = ParentWindowLink;
            InitializeComponent();
            ReInitializeDETaskLogic();
        }

        public void ReInitializeDETaskLogic()
        {
            /*DataExcavatorUIProjectLink.TaskLink.TaskScrapingCompleted -= TaskLink_TaskScrapingCompleted;
			DataExcavatorUIProjectLink.TaskLink.TaskScrapingCompleted += TaskLink_TaskScrapingCompleted;
			DataExcavatorUIProjectLink.TaskLink.NeedToSolveCaptchaHandler -= TaskLink_NeedToSolveCaptchaHandler;
			DataExcavatorUIProjectLink.TaskLink.NeedToSolveCaptchaHandler += TaskLink_NeedToSolveCaptchaHandler;*/
            ProjectNameTextBlock.Text = DataExcavatorUIProjectLink.ProjectName;
            base.Dispatcher.Invoke(delegate
            {
                InvalidateCounters(CheckTaskCompleted: true);
            });
            UpdageHWND();
        }

        /*private void TaskLink_NeedToSolveCaptchaHandler(CaptchaManualSolvingDemandArgs CaptchaSolvingDemand)
		{
			base.Dispatcher.Invoke(delegate
			{
				DECaptchaManualSolver dECaptchaManualSolver = new DECaptchaManualSolver(DataExcavatorUIProjectLink.ProjectName);
				dECaptchaManualSolver.AssociateChromium(CaptchaSolvingDemand);
				dECaptchaManualSolver.Show();
			});
		}*/

        public void UpdageHWND()
        {
            HwndSource hWNDSource = (HwndSource)PresentationSource.FromVisual(ParentWindowLink);
            /*DataExcavatorUIProjectLink.TaskLink.SetHWNDSource(hWNDSource);*/
        }

        private void TaskLink_TaskScrapingCompleted(string Information)
        {
            MarkProjectAsCompleted();
        }

        public void MarkProjectAsCompleted()
        {
            base.Dispatcher.Invoke(delegate
            {
                DEScraperRunningIcon.Visibility = Visibility.Collapsed;
                SetStatusStringData("Task completed", Brushes.Green);
                TaskStatus.FontWeight = FontWeights.Bold;
                UnmarkStartButtonPressed();
                CubeActionsMutexEmulator = false;
                ScrapingProcessProgressBar.Value = 100.0;
                ScrapingProcessTextBarNumValue.Text = "100";
                ProjectCubeMainBackgroundBlock.Background = UICommonExtensions.BrushFromHex("#FFC1FFC1");
                MenuItemStartTask.Visibility = Visibility.Collapsed;
                MenuItemStopTask.Visibility = Visibility.Collapsed;
            });
        }

        public void MarkProjectCubeAsCanBeProcessed()
        {
            if (DataExcavatorUIProjectLink.TaskLink.TaskState != 0)
            {
                base.Dispatcher.Invoke(delegate
                {
                    MenuItemStartTask.Visibility = Visibility.Visible;
                    MenuItemStopTask.Visibility = Visibility.Visible;
                    DEScraperRunningIcon.Visibility = Visibility.Collapsed;
                    SetStatusStringData("Waiting for action", Brushes.Gray);
                    UnmarkStartButtonPressed();
                    ProjectCubeMainBackgroundBlock.Background = Brushes.White;
                });
            }
        }

        public void StartCountersRefresher()
        {
            CountersRefreshingThread = new Thread(CountersRefreshingThreadBody);
            CountersRefreshingThread.Start();
        }

        public void StopCountersRefresher()
        {
            if (CountersRefreshingThread != null)
            {
                try
                {
                    CountersRefreshingThread.Abort();
                    CountersRefreshingThread = null;
                }
                catch (Exception)
                {
                }
            }
        }

        private void CountersRefreshingThreadBody()
        {
            while (true)
            {
                try
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        InvalidateCounters();
                    });
                }
                catch (Exception)
                {
                }
                Thread.Sleep(1000);
            }
        }

        public void InvalidateCounters(bool CheckTaskCompleted = false)
        {
            DataExcavatorTaskActualMetric taskActualMetrics = DataExcavatorUIProjectLink.TaskLink.GetTaskActualMetrics();
            PagesToCrawlCounterTextBox.Text = (taskActualMetrics.PagesToCrawlQueueLength + taskActualMetrics.PagesBeingCrawledCount).ToString();
            PagesCrawledCounterTextBox.Text = taskActualMetrics.TotalCrawledPagesCount.ToString();
            PagesGrabbedCounterTextBox.Text = taskActualMetrics.TotalGrabbedPagesCount.ToString();
            PagesToGrabCounterTextBox.Text = (taskActualMetrics.PagesToGrabQueueLengh + taskActualMetrics.PagesBeingGrabbedCount).ToString();
            SessionAverageCrawlingTimeSecontsTextBox.Text = taskActualMetrics.SessionWebsiteResponseAverageSpeedSeconds.ToString();
            SessionPagesErrorsCounterTextBox.Text = taskActualMetrics.SessionCrawlingErrorsCount.ToString();
            BinaryFilesGrabbedTextBox.Text = taskActualMetrics.GrabbedBinaryFilesCount.ToString();
            double num = 0.0;
            if (taskActualMetrics.PagesToCrawlQueueLength + taskActualMetrics.PagesBeingCrawledCount <= 0)
            {
                num = ((taskActualMetrics.PagesToGrabQueueLengh + taskActualMetrics.PagesBeingGrabbedCount > 0) ? 99.0 : ((taskActualMetrics.PagesToCrawlQueueLength + taskActualMetrics.PagesBeingCrawledCount != 0 || taskActualMetrics.PagesToGrabQueueLengh + taskActualMetrics.PagesBeingGrabbedCount != 0) ? 100.0 : 0.0));
            }
            else
            {
                int num2 = taskActualMetrics.PagesToCrawlQueueLength + taskActualMetrics.PagesBeingCrawledCount + taskActualMetrics.TotalCrawledPagesCount;
                if (num2 < 1)
                {
                    num2 = 1;
                }
                num = Math.Round((double)taskActualMetrics.TotalCrawledPagesCount / (double)num2 * 100.0, 2);
            }
            if (num > 100.0)
            {
                num = 100.0;
            }
            ScrapingProcessTextBarNumValue.Text = num.ToString();
            ScrapingProcessProgressBar.Value = num;
            if (num >= 96.0)
            {
                PercentageTriangleArrow.Background = UICommonExtensions.BrushFromHex("#04b124");
            }
            else
            {
                PercentageTriangleArrow.Background = UICommonExtensions.BrushFromHex("#FFE6E6E6");
            }
            if (!CheckTaskCompleted)
            {
                return;
            }
            CrawlingServerProperties crawlingServerPropertiesCopy = DataExcavatorUIProjectLink.TaskLink.GetCrawlingServerPropertiesCopy();
            if (!crawlingServerPropertiesCopy.ReindexCrawledPages)
            {
                if (taskActualMetrics.TotalCrawledPagesCount == 0)
                {
                    MarkProjectCubeAsCanBeProcessed();
                }
                else if (taskActualMetrics.PagesToCrawlQueueLength == 0
                    // && taskActualMetrics.PagesBeingCrawledCount == 0 
                    && taskActualMetrics.PagesToGrabQueueLengh == 0
                // && taskActualMetrics.PagesBeingGrabbedCount == 0
                )
                {
                    MarkProjectAsCompleted();
                }
                else
                {
                    MarkProjectCubeAsCanBeProcessed();
                }
            }
            else
            {
                MarkProjectCubeAsCanBeProcessed();
            }
        }

        private void DataExcavatorParentTask_PageGrabbed(PageGrabbedCallback GrabbedPageData)
        {
            base.Dispatcher.Invoke(delegate
            {
                InvalidateCounters();
            });
        }

        private void DataExcavatorParentTask_PageCrawled(PageCrawledCallback CrawledPageData)
        {
            base.Dispatcher.Invoke(delegate
            {
                InvalidateCounters();
            });
        }

        private void MenuItemStartTask_Click(object sender, RoutedEventArgs e)
        {
            StartProject();
        }

        public void StartProject()
        {
            if (CubeActionsMutexEmulator)
            {
                return;
            }
            CubeActionsMutexEmulator = true;
            DataExcavatorTaskActualMetric taskActualMetrics = DataExcavatorUIProjectLink.TaskLink.GetTaskActualMetrics();
            if (taskActualMetrics.TotalCrawledPagesCount == 0 && !DataExcavatorUIProjectLink.CrawlWebsiteLinksSettedWay)
            {
                if (chooseWebsiteLinksCrawlingWay == null)
                {
                    chooseWebsiteLinksCrawlingWay = new DEChooseWebsiteLinksCrawling(this, ParentWindowLink);
                    chooseWebsiteLinksCrawlingWay.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    ParentWindowLink.ShowMainWindowShadowOverlay(chooseWebsiteLinksCrawlingWay);
                    chooseWebsiteLinksCrawlingWay.Closed += ChooseWebsiteLinksCrawlingWay_Closed;
                    CubeActionsMutexEmulator = false;
                    chooseWebsiteLinksCrawlingWay.Show();
                }
                else
                {
                    CubeActionsMutexEmulator = false;
                    chooseWebsiteLinksCrawlingWay.Focus();
                }
                return;
            }
            if (taskActualMetrics.PagesToCrawlQueueLength == 0 && taskActualMetrics.TotalCrawledPagesCount == 0)
            {
                MessageBox.Show("Before running the project, you must specify at least one link for crawling.", "Initial link(s) for crawling not specifed", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                CubeActionsMutexEmulator = false;
                ShowAddLinksToCrawlingModal(isFromStartButtonClick: true);
                return;
            }
            if (DataExcavatorUIProjectLink.TaskLink.TaskState == DataExcavatorTaskState.Executing)
            {
                CubeActionsMutexEmulator = false;
                return;
            }
            SetStatusStringData("Starting...", Brushes.Green);
            MarkStartButtonPressed();
            try
            {
                DataExcavatorUIProjectLink.TaskLink.StartTask(delegate
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        DEScraperRunningIcon.Visibility = Visibility.Visible;
                        SetStatusStringData("Task running", Brushes.Green);
                        CubeActionsMutexEmulator = false;
                        StartCountersRefresher();
                    });
                });
            }
            catch (LicenseValidationException ex)
            {
                MessageBox.Show($"Error validating the license key. Please check your license key. Details of the error: {ex.Message.ToString()}", "License error", MessageBoxButton.OK, MessageBoxImage.Hand);
                Logger.LogError("Error during project starting - license exception thrown", ex);
                SetStatusStringData("Waiting for action", Brushes.Gray);
                DEScraperRunningIcon.Visibility = Visibility.Collapsed;
                CubeActionsMutexEmulator = false;
            }
        }

        private void MarkStartButtonPressed()
        {
            base.Dispatcher.Invoke(delegate
            {
                MenuItemStartTask.Visibility = Visibility.Collapsed;
                MenuTaskStartedPassiveLabel.Visibility = Visibility.Visible;
            });
        }

        public void UnmarkStartButtonPressed()
        {
            base.Dispatcher.Invoke(delegate
            {
                MenuItemStartTask.Visibility = Visibility.Visible;
                MenuTaskStartedPassiveLabel.Visibility = Visibility.Collapsed;
            });
        }

        private void ChooseWebsiteLinksCrawlingWay_Closed(object sender, EventArgs e)
        {
            chooseWebsiteLinksCrawlingWay = null;
            ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void AddLinksToCrawlingModal_Closed(object sender, EventArgs e)
        {
            addLinksToCrawlingModal = null;
            ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void MenuItemStopTask_Click(object sender, RoutedEventArgs e)
        {
            if (CubeActionsMutexEmulator)
            {
                return;
            }
            CubeActionsMutexEmulator = true;
            if (DataExcavatorUIProjectLink.TaskLink.TaskState == DataExcavatorTaskState.Stopped)
            {
                CubeActionsMutexEmulator = false;
                return;
            }
            SetStatusStringData("Stopping...", Brushes.Red);
            DataExcavatorUIProjectLink.TaskLink.StopTask(delegate
            {
                base.Dispatcher.Invoke(delegate
                {
                    DEScraperRunningIcon.Visibility = Visibility.Collapsed;
                    SetStatusStringData("Task stopped", Brushes.Red);
                    StopCountersRefresher();
                    UnmarkStartButtonPressed();
                    CubeActionsMutexEmulator = false;
                    UnmarkStartButtonPressed();
                });
            });
        }

        public void SetStatusStringData(string StatusText, Brush ForegroundAreaColor)
        {
            TaskStatus.Text = StatusText;
            TaskStatus.Foreground = ForegroundAreaColor;
            TaskStatus.FontWeight = FontWeights.Normal;
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            if (CubeActionsMutexEmulator)
            {
                return;
            }
            CubeActionsMutexEmulator = true;
            if (DataExcavatorUIProjectLink.TaskLink.TaskState == DataExcavatorTaskState.Executing)
            {
                SetStatusStringData("Stopping...", Brushes.Red);
                DataExcavatorUIProjectLink.TaskLink.StopTask(delegate
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        DEScraperRunningIcon.Visibility = Visibility.Collapsed;
                        SetStatusStringData("Task stopped", Brushes.Red);
                        StopCountersRefresher();
                        UnmarkStartButtonPressed();
                        CubeActionsMutexEmulator = false;
                        if (projectCubePropertiesModal != null)
                        {
                            CubeActionsMutexEmulator = false;
                            projectCubePropertiesModal.Focus();
                        }
                        else
                        {
                            projectCubePropertiesModal = new DEProjectCubeProperties(ParentWindowLink, DataExcavatorUIProjectLink);
                            projectCubePropertiesModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            projectCubePropertiesModal.ProjectCubeTestURLContainer = ProjectCubeTestURLContainer;
                            CubeActionsMutexEmulator = false;
                            projectCubePropertiesModal.Closed += ProjectCubePropertiesModal_Closed;
                            ParentWindowLink.ShowMainWindowShadowOverlay(projectCubePropertiesModal);
                            projectCubePropertiesModal.Show();
                        }
                    });
                });
            }
            else if (projectCubePropertiesModal != null)
            {
                CubeActionsMutexEmulator = false;
                projectCubePropertiesModal.Focus();
            }
            else
            {
                projectCubePropertiesModal = new DEProjectCubeProperties(ParentWindowLink, DataExcavatorUIProjectLink);
                projectCubePropertiesModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                projectCubePropertiesModal.ProjectCubeTestURLContainer = ProjectCubeTestURLContainer;
                CubeActionsMutexEmulator = false;
                projectCubePropertiesModal.Closed += ProjectCubePropertiesModal_Closed;
                ParentWindowLink.ShowMainWindowShadowOverlay(projectCubePropertiesModal);
                projectCubePropertiesModal.Show();
            }
        }

        private void ProjectCubePropertiesModal_Closed(object sender, EventArgs e)
        {
            ParentWindowLink.HideMainWindowShadowOverlay();
            projectCubePropertiesModal = null;
        }

        private void MenuItemAddLinkToCrawling_Click(object sender, RoutedEventArgs e)
        {
            ShowAddLinksToCrawlingModal(isFromStartButtonClick: false);
        }

        public void ShowAddLinksToCrawlingModal(bool isFromStartButtonClick)
        {
            if (addLinksToCrawlingModal != null)
            {
                addLinksToCrawlingModal.Focus();
                return;
            }
            addLinksToCrawlingModal = new DEAddLinksToCrawling(this, isFromStartButtonClick);
            addLinksToCrawlingModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ParentWindowLink.ShowMainWindowShadowOverlay(addLinksToCrawlingModal);
            addLinksToCrawlingModal.Closed += AddLinksToCrawlingModal_Closed;
            addLinksToCrawlingModal.Show();
        }

        private void StatsAndLogsModalLink_Closed(object sender, EventArgs e)
        {
            LogsModalLink.ReleaseResources();
            ParentWindowLink.HideMainWindowShadowOverlay();
            LogsModalLink = null;
        }

        private void MenuItemLogs_Click(object sender, RoutedEventArgs e)
        {
            if (LogsModalLink != null)
            {
                LogsModalLink.Focus();
            }
            LogsModalLink = new DELogs(DataExcavatorUIProjectLink);
            LogsModalLink.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            LogsModalLink.Initialize();
            LogsModalLink.Closed += StatsAndLogsModalLink_Closed;
            ParentWindowLink.ShowMainWindowShadowOverlay(LogsModalLink);
            LogsModalLink.Show();
        }

        private void MenuItemGrabbingResultsOverview_Click(object sender, RoutedEventArgs e)
        {
            if (CubeActionsMutexEmulator)
            {
                return;
            }
            CubeActionsMutexEmulator = true;
            if (DataExcavatorUIProjectLink.TaskLink.TaskState == DataExcavatorTaskState.Stopped)
            {
                CubeActionsMutexEmulator = false;
                if (grabbingResultsOverviewModal != null)
                {
                    grabbingResultsOverviewModal.Focus();
                    return;
                }
                grabbingResultsOverviewModal = new DEGrabbingResultsOverview(this);
                grabbingResultsOverviewModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                grabbingResultsOverviewModal.Closed += GrabbingResultsOverviewModal_Closed;
                ParentWindowLink.ShowMainWindowShadowOverlay(grabbingResultsOverviewModal);
                grabbingResultsOverviewModal.Show();
                return;
            }
            SetStatusStringData("Stopping...", Brushes.Red);
            DataExcavatorUIProjectLink.TaskLink.StopTask(delegate
            {
                base.Dispatcher.Invoke(delegate
                {
                    DEScraperRunningIcon.Visibility = Visibility.Collapsed;
                    SetStatusStringData("Task stopped", Brushes.Red);
                    StopCountersRefresher();
                    UnmarkStartButtonPressed();
                    CubeActionsMutexEmulator = false;
                    if (grabbingResultsOverviewModal != null)
                    {
                        grabbingResultsOverviewModal.Focus();
                    }
                    else
                    {
                        grabbingResultsOverviewModal = new DEGrabbingResultsOverview(this);
                        grabbingResultsOverviewModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        grabbingResultsOverviewModal.Closed += GrabbingResultsOverviewModal_Closed;
                        ParentWindowLink.ShowMainWindowShadowOverlay(grabbingResultsOverviewModal);
                        grabbingResultsOverviewModal.Show();
                    }
                });
            });
        }

        private void GrabbingResultsOverviewModal_Closed(object sender, EventArgs e)
        {
            grabbingResultsOverviewModal = null;
            ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void MenuItemsLinksBuffer_Click(object sender, RoutedEventArgs e)
        {
            if (linksBufferOverview != null)
            {
                linksBufferOverview.Focus();
                return;
            }
            linksBufferOverview = new DELinksBufferOverview(this);
            linksBufferOverview.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            linksBufferOverview.Closed += LinksBufferOverview_Closed;
            ParentWindowLink.ShowMainWindowShadowOverlay(linksBufferOverview);
            linksBufferOverview.Show();
        }

        private void LinksBufferOverview_Closed(object sender, EventArgs e)
        {
            linksBufferOverview = null;
            ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void MenuItemDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete the «{DataExcavatorUIProjectLink.ProjectName}» project?", "Project deleting", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }
            if (DataExcavatorUIProjectLink.TaskLink.TaskState == DataExcavatorTaskState.Stopped)
            {
                DataExcavatorUIProjectLink.TaskLink.DeleteTaskFromHDD();
                ParentWindowLink.DETasksFactoryCoreStorage.RemoveTask(DataExcavatorUIProjectLink.TaskLink);
                ParentWindowLink.DEUIProjectsStorage.RemoveTaskFromIndexFile(DataExcavatorUIProjectLink);
                base.Dispatcher.Invoke(delegate
                {
                    ParentWindowLink.DEProjectsArea.Children.Remove(this);
                    if (ParentWindowLink.DEProjectsArea.Children.Count == 0)
                    {
                        ParentWindowLink.NoProjectsYetOverlay.Visibility = Visibility.Visible;
                    }
                });
                return;
            }
            DataExcavatorUIProjectLink.TaskLink.StopTask(delegate
            {
                StopCountersRefresher();
                UnmarkStartButtonPressed();
                DataExcavatorUIProjectLink.TaskLink.DeleteTaskFromHDD();
                ParentWindowLink.DETasksFactoryCoreStorage.RemoveTask(DataExcavatorUIProjectLink.TaskLink);
                ParentWindowLink.DEUIProjectsStorage.RemoveTaskFromIndexFile(DataExcavatorUIProjectLink);
                base.Dispatcher.Invoke(delegate
                {
                    ParentWindowLink.DEProjectsArea.Children.Remove(this);
                    if (ParentWindowLink.DEProjectsArea.Children.Count == 0)
                    {
                        ParentWindowLink.NoProjectsYetOverlay.Visibility = Visibility.Visible;
                    }
                });
            });
        }

        private void MenuItemOverviewPageLinks_Click(object sender, RoutedEventArgs e)
        {
            if (linksObservingModalWindow != null)
            {
                linksObservingModalWindow.Focus();
                return;
            }
            linksObservingModalWindow = new DEPageLinksObserving(this);
            linksObservingModalWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            linksObservingModalWindow.Closed += LinksObservingModalWindow_Closed;
            ParentWindowLink.ShowMainWindowShadowOverlay(linksObservingModalWindow);
            linksObservingModalWindow.Show();
        }

        private void LinksObservingModalWindow_Closed(object sender, EventArgs e)
        {
            linksObservingModalWindow = null;
            ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void MenuItemVisitWebsite_Click(object sender, RoutedEventArgs e)
        {
            string websiteRootUrl = DataExcavatorUIProjectLink.TaskLink.WebsiteRootUrl;
            HTTPServingCommon.OpenURLInBrowser(websiteRootUrl);
        }

        private void MenuItemOpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            string taskOperatingDirectory = DataExcavatorUIProjectLink.TaskLink.TaskOperatingDirectory;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = taskOperatingDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception thrownException)
            {
                MessageBox.Show("Exception thrown during project directory opening. Please, follow to next path: " + taskOperatingDirectory, "Error during projects navigation", MessageBoxButton.OK, MessageBoxImage.Hand);
                Logger.LogError("Error during project navigation", thrownException);
            }
        }

        private void MenuItemTestProjectSettings_Clck(object sender, RoutedEventArgs e)
        {
            if (testTaskSettingsModal != null)
            {
                testTaskSettingsModal.Focus();
                return;
            }
            if (DataExcavatorUIProjectLink == null || DataExcavatorUIProjectLink.TaskLink == null)
            {
                MessageBox.Show("Project properties not found", "Project testing error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (ProjectCubeTestURLContainer == null)
            {
                ProjectCubeTestURLContainer = new DEProjectTestingUrlContainer
                {
                    // LastTestingURL = DataExcavatorUIProjectLink.TaskLink.WebsitePageToScrapeExampleUrl
                };
            }
            else if (ProjectCubeTestURLContainer.LastTestingURL == null || ProjectCubeTestURLContainer.LastTestingURL == string.Empty)
            {
                // ProjectCubeTestURLContainer.LastTestingURL = DataExcavatorUIProjectLink.TaskLink.WebsitePageToScrapeExampleUrl;
            }
            testTaskSettingsModal = new DETestTaskSettings(DataExcavatorUIProjectLink.TaskLink, ProjectCubeTestURLContainer);
            testTaskSettingsModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            testTaskSettingsModal.Closed += TestTaskSettingsModal_Closed;
            ParentWindowLink.ShowMainWindowShadowOverlay(testTaskSettingsModal);
            testTaskSettingsModal.Show();
        }

        private void TestTaskSettingsModal_Closed(object sender, EventArgs e)
        {
            ParentWindowLink.HideMainWindowShadowOverlay();
            testTaskSettingsModal = null;
        }

        public void ShowProjectsLimitiationOverlay()
        {
            /*LicenseKeyProjectsLimitationBackground.Visibility = Visibility.Visible;*/
        }

        public void HideProjectsLimitationOverlay()
        {
            LicenseKeyProjectsLimitationBackground.Visibility = Visibility.Hidden;
        }

        private void MenuItemCopyProject_Click(object sender, RoutedEventArgs e)
        {
            LicenseKey actualLicenseKeyCopy = ParentWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy();
            if (actualLicenseKeyCopy.KeyProjectsLimit != -1 && ParentWindowLink.DETasksFactoryCoreStorage.GetTasksList().Count + 1 > actualLicenseKeyCopy.KeyProjectsLimit)
            {
                MessageBox.Show($"Your license key is limited to {actualLicenseKeyCopy.KeyProjectsLimit} projects. You cannot copy a project because you have already reached the maximum number of projects.", "License limitation error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            DataExcavatorTask taskLink = DataExcavatorUIProjectLink.TaskLink;
            DataExcavatorTaskIO dataExcavatorTaskIO = new DataExcavatorTaskIO();
            string jSONData = dataExcavatorTaskIO.ExportDETaskIntoJSON(taskLink);
            DataExcavatorTaskExportContainer dataExcavatorTaskExportContainer = dataExcavatorTaskIO.ImportDETaskFromJSON(jSONData);
            string text = ParentWindowLink.RollNewProjectName(dataExcavatorTaskExportContainer.ProjectName);
            DataExcavatorTask excavatorTaskLink = new DataExcavatorTask(text, dataExcavatorTaskExportContainer.WebsiteRootUrl,
                    // dataExcavatorTaskExportContainer.WebsitePageToScrapeExampleUrl, 
                    dataExcavatorTaskExportContainer.ProjectDescription, dataExcavatorTaskExportContainer.GrabbingPatterns, dataExcavatorTaskExportContainer.CrawlerPeroperties, dataExcavatorTaskExportContainer.GrabberProperties, "");
            string projectPath = DEProjectCubeProperties.RollProjectDefaultPath(text);
            if (copyTaskSettingsModal != null)
            {
                copyTaskSettingsModal.Focus();
                return;
            }
            copyTaskSettingsModal = new DEProjectCubeProperties(ParentWindowLink, excavatorTaskLink, projectPath);
            copyTaskSettingsModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            copyTaskSettingsModal.Closed += CopyTaskSettingsModal_Closed;
            ParentWindowLink.ShowMainWindowShadowOverlay(copyTaskSettingsModal);
            copyTaskSettingsModal.Show();
        }

        private void CopyTaskSettingsModal_Closed(object sender, EventArgs e)
        {
            copyTaskSettingsModal = null;
            ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void MenuItemOpenWebBrowser_Click(object sender, RoutedEventArgs e)
        {
            observeWebsitePageModal = new DEObserveAnyWebsitePageWindow(DataExcavatorUIProjectLink.TaskLink, ProjectCubeTestURLContainer);
            observeWebsitePageModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            observeWebsitePageModal.Closed += ObserveWebsitePageModal_Closed;
            ParentWindowLink.ShowMainWindowShadowOverlay(observeWebsitePageModal);
            observeWebsitePageModal.Show();
        }

        private void ObserveWebsitePageModal_Closed(object sender, EventArgs e)
        {
            ParentWindowLink.HideMainWindowShadowOverlay();
            observeWebsitePageModal = null;
        }
    }
}
