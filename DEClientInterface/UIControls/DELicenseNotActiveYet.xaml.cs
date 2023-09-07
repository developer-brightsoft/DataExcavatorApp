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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEClientInterface.UIControls
{
    /// <summary>
    /// Interaction logic for DELicenseNotActiveYet.xaml
    /// </summary>
    public partial class DELicenseNotActiveYet : UserControl
    {
        public MainWindow MainWindowLink { get; set; }

        public DELicenseNotActiveYet()
        {
            InitializeComponent();
        }

        private void ActivateLicenseKey_Click(object sender, RoutedEventArgs e)
        {
            MainWindowLink.ShowLicenseInformationModal();
        }
    }
}
