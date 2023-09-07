using DEClientInterface.Controls;
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
    /// Interaction logic for DEAddLinksToCrawlingResults.xaml
    /// </summary>
    public partial class DEAddLinksToCrawlingResults : Window
    {

        private DEProjectCube ProjectCubeLink { get; set; }

        private bool isModalOpenedFromStartButtonClick { get; set; }

        private CrawlingServerAddLinkToCrawlResults linksAdditionResults { get; set; }

        public DEAddLinksToCrawlingResults(CrawlingServerAddLinkToCrawlResults linksAdditionResults, DEProjectCube ProjectCubeLink, bool isModalOpenedFromStartButtonClick)
        {
            this.ProjectCubeLink = ProjectCubeLink;
            this.isModalOpenedFromStartButtonClick = isModalOpenedFromStartButtonClick;
            this.linksAdditionResults = linksAdditionResults;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", this.ProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Results of adding links:\n\r");
            stringBuilder.Append($"There are {linksAdditionResults.AddedLinksCount} links added, {linksAdditionResults.SkippedLinksCount} links skipped;\n\r");
            stringBuilder.Append("Detailed results:\n\r");
            for (int i = 0; i < linksAdditionResults.LinksAddingLogs.Count; i++)
            {
                stringBuilder.Append(i + 1).Append(".) ").Append(linksAdditionResults.LinksAddingLogs[i].Link)
                    .Append(": ")
                    .Append(linksAdditionResults.LinksAddingLogs[i].LinkAddingResultMessage)
                    .Append(";\n\r");
            }
            AddLinksToCrawlingContentResults.Text = stringBuilder.ToString().Trim();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            ProjectCubeLink.InvalidateCounters();
            ProjectCubeLink.ParentWindowLink.HideMainWindowShadowOverlay();
            if (isModalOpenedFromStartButtonClick)
            {
                if (linksAdditionResults.AddedLinksCount > 0)
                {
                    ProjectCubeLink.StartProject();
                }
                else
                {
                    MessageBox.Show("Cannot start the project because no links have been added", "Cannot start the project", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
        }
    }
}
