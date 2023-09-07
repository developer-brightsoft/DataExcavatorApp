using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using ExcavatorSharp.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for DELicenseActivation.xaml
    /// </summary>
    public partial class DELicenseActivation : Window
    {
        public DELicenseInfo LicenseInfoModalLink { get; set; }

        public DELicenseGetDemoKeyWindow GetDemoKeyModalLink { get; set; }

        public DELicenseActivation(DELicenseInfo LicenseInfoModalLink, DELicenseGetDemoKeyWindow GetDemoKeyModalLink = null)
        {
            this.LicenseInfoModalLink = LicenseInfoModalLink;
            this.GetDemoKeyModalLink = GetDemoKeyModalLink;
            InitializeComponent();
            if (this.LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() != null && this.LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().IsKeyDateValidAndNonOutdated() && this.LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().IsProductCodeValid())
            {
                LicenseKeyField.Foreground = Brushes.Green;
                LicenseKeyField.Text = this.LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().LicenseKeyData;
            }
            else if (this.LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() != null)
            {
                LicenseKeyField.Foreground = Brushes.Red;
            }
        }

        private void ApplyKeyButton_Click(object sender, RoutedEventArgs e)
        {
            WaitLoader.Visibility = Visibility.Visible;
            string LicenseKey = LicenseKeyField.Text;
            Task.Run(delegate
            {
                try
                {
                    if (LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() != null && LicenseInfoModalLink.MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().LicenseKeyData == LicenseKey)
                    {
                        base.Dispatcher.Invoke(delegate
                        {
                            LicenseKeyField.Foreground = Brushes.Green;
                            WaitLoader.Visibility = Visibility.Hidden;
                            MessageBox.Show("The key you activate is identical to the key you are using. You have already activated this key.", "Activation rejected", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        });
                    }
                    else
                    {
                        LicenseActivationResponse ActivationResult = LicenseInfoModalLink.MainWindowLink.DELicenseActivator.TryToActivateNewKey(LicenseKey);
                        base.Dispatcher.Invoke(delegate
                        {
                            if (!ActivationResult.IsLicenseActivated)
                            {
                                WaitLoader.Visibility = Visibility.Hidden;
                                LicenseKeyField.Foreground = Brushes.Red;
                                MessageBox.Show(ActivationResult.LicenseActivationResponseText, "License activation error", MessageBoxButton.OK, MessageBoxImage.Hand);
                            }
                            else
                            {
                                WaitLoader.Visibility = Visibility.Hidden;
                                LicenseKeyField.Foreground = Brushes.Green;
                                if (GetDemoKeyModalLink != null)
                                {
                                    GetDemoKeyModalLink.Close();
                                    GetDemoKeyModalLink = null;
                                }
                                LicenseInfoModalLink.MainWindowLink.ClearWorkingAreaAndProjectsStorage();
                                LicenseInfoModalLink.MainWindowLink.DEUIProjectsStorage.LoadIndexFileFromHDD(LicenseInfoModalLink.MainWindowLink);
                                MessageBox.Show("License activated", "License activation results", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                Close();
                                LicenseInfoModalLink.ReloadLicenseInfoOutputData();
                                LicenseInfoModalLink.MainWindowLink.HideErrorNotification();
                                LicenseInfoModalLink.MainWindowLink.LicenseNotActiveYet.Visibility = Visibility.Hidden;
                                if (LicenseInfoModalLink.MainWindowLink.DEUIProjectsStorage.SavedProjectsLinks.Count == 0)
                                {
                                    LicenseInfoModalLink.Close();
                                    LicenseInfoModalLink.MainWindowLink.NoProjectsYetOverlay.Visibility = Visibility.Visible;
                                }
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoader.Visibility = Visibility.Hidden;
                    });
                    Logger.LogError("Error during license activation", ex);
                    App.TrySendAppCrashReport(ex, "Error during license activation");
                }
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (GetDemoKeyModalLink != null)
            {
                GetDemoKeyModalLink.overlayController.HideOverlay();
                GetDemoKeyModalLink.ActivateDemoKeyButton.Visibility = Visibility.Visible;
            }
        }
    }
}
