using CefSharp.Wpf;
using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Common;
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
    /// Interaction logic for DECreateFirstProjectWithPicker.xaml
    /// </summary>
    public partial class DECreateFirstProjectWithPicker : Window
    {
        private string ActuallyOfferedWebsiteDomainIfTemplateFound = string.Empty;

        public DOMSelectorsTester CSSSelectorsTester = new DOMSelectorsTester();

        public ChromiumJSNodesPicker PageNodesPicker = null;


        private MainWindow MainWindowLink { get; set; }

        public DECreateFirstProjectWithPicker()
        {
            InitializeComponent();
        }

        public DECreateFirstProjectWithPicker(MainWindow MainWindowLink)
        {
            this.MainWindowLink = MainWindowLink;
            InitializeComponent();
        }

        private void NewProjectModal_Closed(object sender, EventArgs e)
        {
            MainWindowLink.HideMainWindowShadowOverlay();
        }

        private void NavigateSamplePageStartButton_Click(object sender, RoutedEventArgs e)
        {
            base.Dispatcher.BeginInvoke((Action)delegate
            {
                LoadDemoPageAndSetupSelectors();
            });
        }

        private void LoadDemoPageAndSetupSelectors()
        {
            ExamplePageOverlayStartModalTextBox.MarkAsCorrectlyCompleted();
            string text = ExamplePageOverlayStartModalTextBox.Text.Trim();
            if (text == string.Empty || !Uri.IsWellFormedUriString(text, UriKind.Absolute))
            {
                ExamplePageOverlayStartModalTextBox.MarkAsUncorrectlyCompleted();
                MessageBox.Show("Uncorrect demo page URL. Please, complete demo page width valid URL", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            CompletePageAddressToStartGrid.Visibility = Visibility.Collapsed;
            WaitLoaderOverlay.Visibility = Visibility.Visible;
            PageNodesPicker = new ChromiumJSNodesPicker(MainWindowLink, null, ProjectSettingsPage_PickNodesWebBrowser, PickedElementsStackEmulator, ExamplePageOverlayStartModalTextBox.Text, this);
            PageNodesPicker.NavigateAndSetupPickerWithPickedNodes();
        }

        private void CreateProjectFromExpressWindow_Click(object sender, RoutedEventArgs e)
        {
            string text = ExamplePageOverlayStartModalTextBox.Text;
            List<DataGrabbingPatternItem> list = new List<DataGrabbingPatternItem>();
            for (int i = 0; i < PickedElementsStackEmulator.Children.Count; i++)
            {
                DataGrabbingPatternItem item = (PickedElementsStackEmulator.Children[i] as DEGrabberFlatItemSelector).TryGetPatternItemFromUIData();
                list.Add(item);
            }
            if (list.Count == 0)
            {
                MessageBox.Show("There are no nodes to scrape selected on the page. Please click on the page elements to scrape them.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            DEProjectCubeProperties dEProjectCubeProperties = new DEProjectCubeProperties(MainWindowLink, text, list);
            dEProjectCubeProperties.Closed += ProjectCubeProperties_Closed;
            Close();
            MainWindowLink.ShowMainWindowShadowOverlay(dEProjectCubeProperties);
            dEProjectCubeProperties.Show();
        }

        private void ProjectCubeProperties_Closed(object sender, EventArgs e)
        {
            MainWindowLink.HideMainWindowShadowOverlay();
        }

        private void ExamplePageOverlayStartModalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                base.Dispatcher.BeginInvoke((Action)delegate
                {
                    LoadDemoPageAndSetupSelectors();
                });
            }
        }
    }
}
