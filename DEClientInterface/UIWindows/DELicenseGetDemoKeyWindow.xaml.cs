using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using RestSharp;
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
    /// Interaction logic for DELicenseGetDemoKeyWindow.xaml
    /// </summary>
    public partial class DELicenseGetDemoKeyWindow : Window
    {
        private DELicenseInfo LicenseInfoModal = null;

        private DELicenseActivation LicenseActivationModal = null;

        public MainWindow MainWindowLink { get; set; }

        public UIOverlayController overlayController { get; set; }

        public DELicenseGetDemoKeyWindow(MainWindow MainWindowLink, DELicenseInfo LicenseInfoModal)
        {
            this.MainWindowLink = MainWindowLink;
            this.LicenseInfoModal = LicenseInfoModal;
            InitializeComponent();
            overlayController = new UIOverlayController(this, ShadowOverlay);
        }

        private void GetDemoKeyButton_Click(object sender, RoutedEventArgs e)
        {
            DEDemoKeyUserNameTextArea.MarkAsCorrectlyCompleted();
            DEDemoKeyUserEmailTextArea.MarkAsCorrectlyCompleted();
            string UserName = DEDemoKeyUserNameTextArea.Text.Trim();
            string UserEmail = DEDemoKeyUserEmailTextArea.Text.Trim();
            if (UserName == string.Empty || UserEmail == string.Empty)
            {
                if (UserName == string.Empty)
                {
                    DEDemoKeyUserNameTextArea.MarkAsUncorrectlyCompleted();
                    MessageBox.Show("Please, complete your name");
                    return;
                }
                if (UserEmail == string.Empty)
                {
                    DEDemoKeyUserEmailTextArea.MarkAsUncorrectlyCompleted();
                    MessageBox.Show("Please, complete your email");
                    return;
                }
            }
            WaitLoader.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    RestClient restClient = new RestClient();
                    RestRequest restRequest = new RestRequest("https://data-excavator.com/de-licensing/api.php");
                    restRequest.Method = Method.POST;
                    restRequest.AddParameter("action", "create-license-key");
                    restRequest.AddParameter("UserEmail", UserEmail);
                    restRequest.AddParameter("UserName", UserName);
                    IRestResponse restResponse = restClient.Execute(restRequest);
                    string content = restResponse.Content;
                    if (content == "DEMO_CREATED_OK")
                    {
                        MessageBox.Show($"The demo key was successfully created and sent to the mail {UserEmail}. Copy it from your email and activate it in the next window, which we will open right now.", "Demo key issued", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        base.Dispatcher.Invoke(delegate
                        {
                            base.Dispatcher.Invoke(delegate
                            {
                                WaitLoader.Visibility = Visibility.Hidden;
                            });
                            LicenseActivationModal = new DELicenseActivation(LicenseInfoModal, this);
                            LicenseActivationModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            overlayController.ShowOverlay(LicenseActivationModal);
                            LicenseActivationModal.Show();
                        });
                    }
                    else if (content == "DEMO_ALREADY_ISSUED")
                    {
                        MessageBox.Show("A demo key has already been issued to this email address. We cannot reissue the demo version and recommend that you consider purchasing a paid key. We look forward to your support!");
                        base.Dispatcher.Invoke(delegate
                        {
                            WaitLoader.Visibility = Visibility.Hidden;
                        });
                    }
                }
                catch (Exception thrownException)
                {
                    MessageBox.Show("Error duting demo key creation. Please, contact support or get your key from our website (Main menu -> Pages -> Get demo license key)");
                    Logger.LogError("Cannot create demo key with API", thrownException);
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoader.Visibility = Visibility.Hidden;
                    });
                }
            });
        }

        private void ActivateDemoKeyButton_Click(object sender, RoutedEventArgs e)
        {
            LicenseActivationModal = new DELicenseActivation(LicenseInfoModal, this);
            LicenseActivationModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            overlayController.ShowOverlay(LicenseActivationModal);
            LicenseActivationModal.Show();
        }
    }
}
