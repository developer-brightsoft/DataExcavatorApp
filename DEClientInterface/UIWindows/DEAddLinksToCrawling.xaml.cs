using DEClientInterface.Controls;
using DEClientInterface.Logic;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEAddLinksToCrawling.xaml
    /// </summary>
    public partial class DEAddLinksToCrawling : Window
    {
        private DEProjectCube ProjectCubeLink { get; set; }

        private bool isModalOpenedFromStartButtonClick { get; set; }

        public DEAddLinksToCrawling(DEProjectCube ProjectCubeLink, bool isModalOpenedFromStartButtonClick)
        {
            this.ProjectCubeLink = ProjectCubeLink;
            this.isModalOpenedFromStartButtonClick = isModalOpenedFromStartButtonClick;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", this.ProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
            base.Loaded += DEAddLinksToCrawling_Loaded;
            CrawlingServerLinksBuffer crawlingServerLinksBufferCopy = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetCrawlingServerLinksBufferCopy();
            if (crawlingServerLinksBufferCopy.CrawledLinks.Count == 0 && crawlingServerLinksBufferCopy.LinksToCrawl.Count == 0)
            {
                string text = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.WebsiteRootUrl;
                if (ProjectCubeLink.ProjectCubeTestURLContainer != null && ProjectCubeLink.ProjectCubeTestURLContainer.LastTestingURL != string.Empty)
                {
                    text = text + "\r\n" + ProjectCubeLink.ProjectCubeTestURLContainer.LastTestingURL;
                }
                LinksListTextBox.Text = text;
            }
        }

        private void DEAddLinksToCrawling_Loaded(object sender, RoutedEventArgs e)
        {
            LinksListTextBox.Focus();
            LinksListTextBox.CaretIndex = LinksListTextBox.Text.Length;
        }

        private void CancelAddLinksToCrawl_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddLinkToCrawlButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessAddLinksToCrawl();
        }

        private void ProcessAddLinksToCrawl()
        {
            LinksListTextBox.MarkAsCorrectlyCompleted();
            string text = LinksListTextBox.Text.Trim();
            if (text.Length == 0)
            {
                LinksListTextBox.MarkAsUncorrectlyCompleted();
                LinksListTextBox.Focus();
                return;
            }
            AddLinksWaitLoader.Visibility = Visibility.Visible;
            string[] LinksSeparated = text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            CrawlingServerAddLinkToCrawlResults LinksAddingResults = null;
            Task.Run(delegate
            {
                LinksAddingResults = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.AddLinksToCrawling(LinksSeparated.ToList());
                if (LinksAddingResults.AddedLinksCount > 0)
                {
                    ProjectCubeLink.MarkProjectCubeAsCanBeProcessed();
                }
                base.Dispatcher.Invoke(delegate
                {
                    AddLinksWaitLoader.Visibility = Visibility.Hidden;
                    DEAddLinksToCrawlingResults dEAddLinksToCrawlingResults = new DEAddLinksToCrawlingResults(LinksAddingResults, ProjectCubeLink, isModalOpenedFromStartButtonClick);
                    dEAddLinksToCrawlingResults.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dEAddLinksToCrawlingResults.Closed += LinksAddingResultsModal_Closed;
                    ProjectCubeLink.ParentWindowLink.PreventShadowOverlayHiding();
                    ProjectCubeLink.ParentWindowLink.ShowMainWindowShadowOverlay(dEAddLinksToCrawlingResults);
                    dEAddLinksToCrawlingResults.Show();
                    Close();
                });
            });
        }

        private void LinksAddingResultsModal_Closed(object sender, EventArgs e)
        {
            ProjectCubeLink.ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void LoadLinksFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            DialogResult dialogResult = openFileDialog.ShowDialog();
            if (dialogResult != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            string fileName = openFileDialog.FileName;
            if (File.Exists(fileName))
            {
                string text = null;
                try
                {
                    text = File.ReadAllText(fileName);
                }
                catch (Exception thrownException)
                {
                    Logger.LogError($"Cannot load links from file = {fileName}", thrownException);
                }
                LinksListTextBox.Text = text;
                ProcessAddLinksToCrawl();
            }
        }

        private void LinksListTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string text = LinksListTextBox.Text;
            if (text.IndexOf(",") != -1)
            {
                text = text.Replace(",", "\r\n");
                LinksListTextBox.Text = text;
                LinksListTextBox.CaretIndex = LinksListTextBox.Text.Length;
            }
        }
    }
}
