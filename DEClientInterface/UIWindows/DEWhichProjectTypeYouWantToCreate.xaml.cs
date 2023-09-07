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
    /// Interaction logic for DEWhichProjectTypeYouWantToCreate.xaml
    /// </summary>
    public partial class DEWhichProjectTypeYouWantToCreate : Window
    {
        private MainWindow MainWindowLink { get; set; }

        public DEWhichProjectTypeYouWantToCreate(MainWindow MainWindowLink)
        {
            this.MainWindowLink = MainWindowLink;
            InitializeComponent();
        }

        private void CreateProjectWithWizardButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindowLink.OpenCreateMyFirstProjectModal();
            MainWindowLink.PreventShadowOverlayHiding();
            Close();
        }

        private void CreateSTDProjectButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindowLink.OpenCreateStandardProjectModal();
            MainWindowLink.PreventShadowOverlayHiding();
            Close();
        }
    }
}
