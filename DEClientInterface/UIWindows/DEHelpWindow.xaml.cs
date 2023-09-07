using System;
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

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEHelpWindow.xaml
    /// </summary>
    public partial class DEHelpWindow : Window
    {
        private MainWindow MainWindowLink { get; set; }

        public DEHelpWindow(MainWindow MainWindowLink)
        {
            InitializeComponent();
            this.MainWindowLink = MainWindowLink;
            base.DataContext = this;
        }

        private void WebsiteUrlHyperLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void CreateNewProjectHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Close();
            MainWindowLink.OpenChooseCreatingProjectWay();
        }
    }
}
