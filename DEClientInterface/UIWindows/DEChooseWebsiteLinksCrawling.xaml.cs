using DEClientInterface.Controls;
using ExcavatorSharp.Excavator;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEChooseWebsiteLinksCrawling.xaml
    /// </summary>
    public partial class DEChooseWebsiteLinksCrawling : Window
    {
        private DEProjectCube projectCubeLink { get; set; }

        private MainWindow mainWindowLink { get; set; }

        public DEChooseWebsiteLinksCrawling(DEProjectCube ProjectCubeLink, MainWindow MainWindowLink)
        {
            projectCubeLink = ProjectCubeLink;
            mainWindowLink = MainWindowLink;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", ProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
        }

        private void AnalyseAllPages_Click(object sender, RoutedEventArgs e)
        {
            DataExcavatorTask taskLink = projectCubeLink.DataExcavatorUIProjectLink.TaskLink;
            CrawlingServerProperties crawlingServerPropertiesCopy = taskLink.GetCrawlingServerPropertiesCopy();
            GrabbingServerProperties grabbingServerPropertiesCopy = taskLink.GetGrabbingServerPropertiesCopy();
            List<DataGrabbingPattern> dataGrabbingPatternsCopy = taskLink.GetDataGrabbingPatternsCopy();
            crawlingServerPropertiesCopy.CrawlWebsiteLinks = true;
            taskLink.UpdateExcavatorTaskData(taskLink.TaskName, taskLink.WebsiteRootUrl,
                /*taskLink.WebsitePageToScrapeExampleUrl,*/
                taskLink.TaskDescription, dataGrabbingPatternsCopy, crawlingServerPropertiesCopy, grabbingServerPropertiesCopy, taskLink.TaskOperatingDirectory);
            taskLink.SaveTaskToHDD();
            projectCubeLink.UpdageHWND();
            projectCubeLink.DataExcavatorUIProjectLink.CrawlWebsiteLinksSettedWay = true;
            mainWindowLink.DEUIProjectsStorage.FlushIndexFileToHDD();
            CheckInitialLinksForCrawlingAreDefined();
        }

        private void AnalyseSpecifiedLinks_Click(object sender, RoutedEventArgs e)
        {
            DataExcavatorTask taskLink = projectCubeLink.DataExcavatorUIProjectLink.TaskLink;
            CrawlingServerProperties crawlingServerPropertiesCopy = taskLink.GetCrawlingServerPropertiesCopy();
            GrabbingServerProperties grabbingServerPropertiesCopy = taskLink.GetGrabbingServerPropertiesCopy();
            List<DataGrabbingPattern> dataGrabbingPatternsCopy = taskLink.GetDataGrabbingPatternsCopy();
            crawlingServerPropertiesCopy.CrawlWebsiteLinks = false;
            taskLink.UpdateExcavatorTaskData(taskLink.TaskName, taskLink.WebsiteRootUrl,
                /*taskLink.WebsitePageToScrapeExampleUrl,*/
                taskLink.TaskDescription, dataGrabbingPatternsCopy, crawlingServerPropertiesCopy, grabbingServerPropertiesCopy, taskLink.TaskOperatingDirectory);
            taskLink.SaveTaskToHDD();
            projectCubeLink.UpdageHWND();
            projectCubeLink.DataExcavatorUIProjectLink.CrawlWebsiteLinksSettedWay = true;
            mainWindowLink.DEUIProjectsStorage.FlushIndexFileToHDD();
            CheckInitialLinksForCrawlingAreDefined();
        }

        private void CheckInitialLinksForCrawlingAreDefined()
        {
            if (projectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetTaskActualMetrics().PagesToCrawlQueueLength == 0)
            {
                mainWindowLink.PreventShadowOverlayHiding();
                projectCubeLink.ShowAddLinksToCrawlingModal(isFromStartButtonClick: true);
                Close();
            }
            else
            {
                projectCubeLink.StartProject();
                Close();
            }
        }
    }
}
