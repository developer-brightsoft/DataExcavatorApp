using DEClientInterface.Objects;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEClientInterface.UIControls
{
    /// <summary>
    /// Interaction logic for DEProjectTemplateCard.xaml
    /// </summary>
    public partial class DEProjectTemplateCard : UserControl
    {

        private MainWindow MainWindowLink { get; set; }

        private DEProjectTemplate ProjectTemplateLink { get; set; }

        public DEProjectTemplateCard(MainWindow MainWindowLink, DEProjectTemplate ProjectTemplate)
        {
            this.MainWindowLink = MainWindowLink;
            ProjectTemplateLink = ProjectTemplate;
            InitializeComponent();
            RenderProjectTemplateData();
        }

        private void RenderProjectTemplateData()
        {
            ProjectShortNameTextBlock.Text = ProjectTemplateLink.ProjectTemplateUICardStyling.TemplateShortName;
            NavigateURLBlock.NavigateUri = new Uri(ProjectTemplateLink.TemplateWebsiteURL);
            WebsiteHyperLinkText.Text = ProjectTemplateLink.TemplateWebsiteURL;
            ProjectUpdateDateTextBlock.Text = string.Format("Update from {0}", ProjectTemplateLink.TemplateUpdateDate.ToString("yyyy-MM-dd"));
            string text = ProjectTemplateLink.TemplateDescription;
            if (text.Length > 115)
            {
                text = text.Substring(0, 115) + "...";
            }
            ProjectTemplateShortDescriptionTextBlock.Text = text;
            BrushConverter brushConverter = new BrushConverter();
            ProjectTopBlockShortNameOuter.Background = (Brush)brushConverter.ConvertFrom(ProjectTemplateLink.ProjectTemplateUICardStyling.CardBackgroundColorHex);
            ProjectShortNameTextBlock.Foreground = (Brush)brushConverter.ConvertFrom(ProjectTemplateLink.ProjectTemplateUICardStyling.FontColorHex);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindowLink.importProjectFromTemplateModal != null)
            {
                MainWindowLink.importProjectFromTemplateModal.Focus();
                return;
            }
            MainWindowLink.importProjectFromTemplateModal = new DEProjectCubeProperties(MainWindowLink, ProjectTemplateLink);
            MainWindowLink.importProjectFromTemplateModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MainWindowLink.importProjectFromTemplateModal.Closed += ImportProjectFromTemplateModal_Closed;
            MainWindowLink.importProjectFromTemplateModal.Show();
            MainWindowLink.ShowMainWindowShadowOverlay(MainWindowLink.importProjectFromTemplateModal);
        }

        private void ImportProjectFromTemplateModal_Closed(object sender, EventArgs e)
        {
            MainWindowLink.importProjectFromTemplateModal = null;
            MainWindowLink.HideMainWindowShadowOverlay();
        }

    }
}
