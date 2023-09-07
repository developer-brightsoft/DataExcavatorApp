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

namespace DEClientInterface.UIControls
{
    /// <summary>
    /// Interaction logic for DEAppNoProjectsYet.xaml
    /// </summary>
    public partial class DEAppNoProjectsYet : UserControl
    {
        public MainWindow MainWindowLink { get; set; }

        public DEAppNoProjectsYet()
        {
            InitializeComponent();
        }

        private void CreateFirstScrapingProject_Click(object sender, RoutedEventArgs e)
        {
            MainWindowLink.OpenChooseCreatingProjectWay();
        }

        private void SelectReadyMadeTemplate_Click(object sender, RoutedEventArgs e)
        {
            MainWindowLink.MainTabControl.SelectedIndex = 1;
        }

        private void ReadFAQ_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://data-excavator.com/faq/"));
        }

    }
}
