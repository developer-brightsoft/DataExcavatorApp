using DEClientInterface.Logic;
using ExcavatorSharp.Licensing;
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
    /// Interaction logic for DELicenseInfo.xaml
    /// </summary>
    public partial class DELicenseInfo : Window
    {
        private DELicenseActivation LicenseActivationModal = null;

        private DELicenseGetDemoKeyWindow GetDemoKeyModal = null;


        public MainWindow MainWindowLink { get; set; }

        private UIOverlayController overlayController { get; set; }

        public DELicenseInfo(MainWindow MainWindowLink)
        {
            this.MainWindowLink = MainWindowLink;
            InitializeComponent();
            ReloadLicenseInfoOutputData();
            overlayController = new UIOverlayController(this, ShadowOverlay);
        }

        public void ReloadLicenseInfoOutputData()
        {
            LicenseKey actualLicenseKeyCopy = MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy();
            if (actualLicenseKeyCopy == null)
            {
                LicenseInformationText.Text = "There is no active license. Please activate the license key or use the demo.";
            }
            else if (actualLicenseKeyCopy.IsProductCodeValid() && actualLicenseKeyCopy.KeyExpirationDate > DateTime.Now)
            {
                int num = Convert.ToInt32(Math.Round((actualLicenseKeyCopy.KeyExpirationDate - DateTime.Now).TotalDays, 0));
                string text = ((actualLicenseKeyCopy.KeyProjectsLimit != -1) ? actualLicenseKeyCopy.KeyProjectsLimit.ToString() : "no limit");
                string text2 = ((actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject != -1) ? actualLicenseKeyCopy.KeyTotalThreadsLimitPerProject.ToString() : "no limit");
                string text3 = string.Format("Your license key is valid until {0} ({1} days left). The key type is an {2} key. Limit of projects - {3}, limit of threads - {4}.", actualLicenseKeyCopy.KeyExpirationDate.ToString("yyyy-MM-dd"), num, actualLicenseKeyCopy.GetKeyTypeName(), text, text2);
                LicenseInformationText.Text = text3;
            }
            else if (!actualLicenseKeyCopy.IsProductCodeValid())
            {
                string text4 = $"Your product code is invalid. Please update the key.";
                LicenseInformationText.Text = text4;
            }
            else
            {
                string text5 = string.Format("Your license key has expired (was valid till {0}). Please update the key.", actualLicenseKeyCopy.KeyExpirationDate.ToString("yyyy-MM-dd"));
                LicenseInformationText.Text = text5;
            }
        }

        public static bool IsLicenseOK(MainWindow mainWindowLink)
        {
            LicenseKey actualLicenseKeyCopy = mainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy();
            if (actualLicenseKeyCopy == null)
            {
                return false;
            }
            if (actualLicenseKeyCopy.IsProductCodeValid() && actualLicenseKeyCopy.KeyExpirationDate > DateTime.Now)
            {
                return true;
            }
            return false;
        }

        private void EnterLicenseKeyButton_Click(object sender, RoutedEventArgs e)
        {
            LicenseActivationModal = new DELicenseActivation(this);
            LicenseActivationModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            LicenseActivationModal.Closed += LicenseActivationModal_Closed;
            ShowOverlay(LicenseActivationModal);
            LicenseActivationModal.Show();
        }

        private void LicenseActivationModal_Closed(object sender, EventArgs e)
        {
            HideOverlay();
            LicenseActivationModal = null;
        }

        private void VisitWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            HTTPServingCommon.OpenURLInBrowser("https://data-excavator.com/");
        }

        public void ShowOverlay(Window overlayWindow)
        {
            overlayController.ShowOverlay(overlayWindow);
        }

        public void HideOverlay()
        {
            overlayController.HideOverlay();
        }

        private void GetDemoKeyButton_Click(object sender, RoutedEventArgs e)
        {
            GetDemoKeyModal = new DELicenseGetDemoKeyWindow(MainWindowLink, this);
            GetDemoKeyModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            GetDemoKeyModal.Closed += GetDemoKeyModal_Closed;
            ShowOverlay(GetDemoKeyModal);
            GetDemoKeyModal.Show();
        }

        private void GetDemoKeyModal_Closed(object sender, EventArgs e)
        {
            HideOverlay();
            GetDemoKeyModal = null;
        }
    }
}
