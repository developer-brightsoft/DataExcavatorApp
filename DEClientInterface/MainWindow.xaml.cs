// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.MainWindow
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using CefSharp.Wpf;
using DEClientInterface;
using DEClientInterface.Controls;
using DEClientInterface.InterfaceLogic;
using DEClientInterface.Logic;
using DEClientInterface.Objects;
using DEClientInterface.UIControls;
using DEClientInterface.UIWindows;
using ExcavatorSharp.CEF;
using ExcavatorSharp.Common;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Licensing;

namespace DEClientInterface
{
    public partial class MainWindow : Window
    {

        public static List<ChromiumWebBrowser> BrowsersLinks = new List<ChromiumWebBrowser>();

        private DELicenseInfo licenseInfoModal = null;

        private DEHelpWindow deHelpWindowModal = null;

        private DECommonPropertiesWindow deAppCommonPropertiesModal = null;

        private DECreateFirstProject createExpresstProjectModal = null;

        private DECreateFirstProjectWithPicker createExpressProjectWithPickerModal = null;

        private DEProjectCubeProperties createStandardProjectModal = null;

        private DEProjectCubeProperties importProjectFromFileModal = null;

        internal DEProjectCubeProperties importProjectFromTemplateModal = null;

        private DEWhichProjectTypeYouWantToCreate ChooseWayForNewProjectModal = null;


        public DataExcavatorTasksFactory DETasksFactoryCoreStorage { get; set; }

        public ProjectsStorage DEUIProjectsStorage { get; set; }

        public LicenseActivator DELicenseActivator { get; set; }

        public CorePropertiesStorage CorePropertiesLoader { get; set; }

        public DEProjectTemplatesStorage TemplatesStorage { get; set; }

        public UIOverlayController overlayController { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            base.Closing += MainWindow_Closing;
            base.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CEFSharpFactory.InitializeCEFBrowser();
            }
            catch (Exception ex)
            {
                Logger.LogError("Cannot initialize CEF browser", ex);
                App.TrySendAppCrashReport(ex, "Cannot initialize CEF browser");
                Thread.Sleep(5000);
                MessageBox.Show($"The CEF component cannot be initialized. Error details: {ex.ToString()}");
                Application.Current.Shutdown();
            }
            Task.Run(delegate
            {
                base.Dispatcher.Invoke(delegate
                {
                    DETasksFactoryCoreStorage = new DataExcavatorTasksFactory();
                    if (!DETasksFactoryCoreStorage.IsApplicationRunnedInASingleCopy())
                    {
                        Logger.LogInformation("You are trying to run multiple copies of the program at once. You can only run one copy of the application. The attempt was rejected - the application is closed.");
                        MessageBox.Show("You are trying to run multiple instances of the DataExcavator.exe application at once. Our application can only run in a single instance. The application will be closed.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        Close();
                        try
                        {
                            Application.Current.Shutdown();
                            return;
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                    DELicenseActivator = new LicenseActivator(DETasksFactoryCoreStorage);
                    CorePropertiesLoader = new CorePropertiesStorage();
                    CorePropertiesLoader.LoadDECoreProperties();
                    /*ECHttpServingCommon.ConfigureServicePointManager();*/
                    DELicenseActivator.TryToReadLicenseOnload();
                    DEUIProjectsStorage = new ProjectsStorage();
                    DEUIProjectsStorage.LoadIndexFileFromHDD(this);
                    TemplatesStorage = new DEProjectTemplatesStorage(this);
                    overlayController = new UIOverlayController(this, MainWindowShadowOverlay);
                    NoProjectsYetOverlay.MainWindowLink = this;
                    LicenseNotActiveYet.MainWindowLink = this;
                    TemplatesStorage.Initialize();
                    if (DETasksFactoryCoreStorage.GetActualLicenseKeyCopy() == null || !DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().IsProductCodeValid() || !DETasksFactoryCoreStorage.GetActualLicenseKeyCopy().IsKeyDateValidAndNonOutdated())
                    {
                        SetLicenseInactiveOrExpiredUIState();
                    }
                    if (!DELicenseInfo.IsLicenseOK(this))
                    {
                        AppLoaderOverlay.Visibility = Visibility.Hidden;
                        LicenseNotActiveYet.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (DEUIProjectsStorage.SavedProjectsLinks.Count == 0)
                        {
                            NoProjectsYetOverlay.Visibility = Visibility.Visible;
                        }
                        AppLoaderOverlay.Visibility = Visibility.Hidden;
                    }
                });
            });
        }

        private void LicenseInfoModal_Closed(object sender, EventArgs e)
        {
            licenseInfoModal = null;
            HideMainWindowShadowOverlay();
        }

        private bool CheckProjectsLimitationBeforeOpenCreateNewProjectAnyModal()
        {
            // LicenseKey actualLicenseKeyCopy = DETasksFactoryCoreStorage.GetActualLicenseKeyCopy();
            // if (actualLicenseKeyCopy == null)
            // {
            //     MessageBox.Show("You cannot create a new project because the license key is not activated. Please buy a license key or activate a demo version of the application.", "License error", MessageBoxButton.OK, MessageBoxImage.Hand);
            //     ShowLicenseInformationModal();
            //     return false;
            // }
            // if (actualLicenseKeyCopy.KeyProjectsLimit != -1 && DETasksFactoryCoreStorage.GetTasksList().Count + 1 > actualLicenseKeyCopy.KeyProjectsLimit)
            // {
            //     MessageBox.Show($"Your license key is limited to {actualLicenseKeyCopy.KeyProjectsLimit} projects. You cannot create new project because you have already reached the maximum number of projects.", "License limitation error", MessageBoxButton.OK, MessageBoxImage.Hand);
            //     return false;
            // }
            return true;
        }

        public void OpenCreateMyFirstProjectModal()
        {
            if (createExpressProjectWithPickerModal != null)
            {
                createExpressProjectWithPickerModal.Focus();
                return;
            }
            if (createStandardProjectModal != null)
            {
                createStandardProjectModal.Close();
            }
            createExpressProjectWithPickerModal = new DECreateFirstProjectWithPicker(this);
            createExpressProjectWithPickerModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            createExpressProjectWithPickerModal.Closed += CreateExpressProjectWithPickerModal_Closed;
            ShowMainWindowShadowOverlay(createExpressProjectWithPickerModal);
            createExpressProjectWithPickerModal.Show();
        }

        private void CreateExpressProjectWithPickerModal_Closed(object sender, EventArgs e)
        {
            HideMainWindowShadowOverlay();
            createExpressProjectWithPickerModal = null;
        }

        private void CreateFirstProjectModal_Closed(object sender, EventArgs e)
        {
            HideMainWindowShadowOverlay();
            createExpresstProjectModal = null;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ClearWorkingAreaAndProjectsStorage();
            DETasksFactoryCoreStorage.DisposeExcavator();
            WaitLoader.Visibility = Visibility.Visible;
            Logger.LogInformation("Application closed");
        }

        public void ClearWorkingAreaAndProjectsStorage()
        {
            if (DEProjectsArea.Children.Count > 0)
            {
                for (int i = 0; i < DEProjectsArea.Children.Count; i++)
                {
                    (DEProjectsArea.Children[i] as DEProjectCube).StopCountersRefresher();
                }
                DEProjectsArea.Children.Clear();
            }
            DETasksFactoryCoreStorage.ClearAllTasks();
        }

        public void SetLicenseInactiveOrExpiredUIState()
        {
            ShowErrorNotification("ERROR: License is inactive or expired. Please, activate license key or use demo key.");
        }

        public void ShowErrorNotification(string Message)
        {
            ErrorsInformationPanel.Visibility = Visibility.Visible;
            ErrorsInformationPanelText.Text = Message;
        }

        public void HideErrorNotification()
        {
            ErrorsInformationPanel.Visibility = Visibility.Hidden;
            ErrorsInformationPanelText.Text = string.Empty;
        }

        public string RollNewProjectName(string OldProjectName)
        {
            string empty = string.Empty;
            bool flag = false;
            int num = 1;
            do
            {
                flag = true;
                empty = $"{OldProjectName} #copy{num}";
                for (int i = 0; i < DEUIProjectsStorage.SavedProjectsLinks.Count; i++)
                {
                    if (DEUIProjectsStorage.SavedProjectsLinks[i].ProjectName == empty)
                    {
                        num++;
                        flag = false;
                        break;
                    }
                }
            }
            while (!flag);
            return empty;
        }

        private void TopMenu_CreateNewProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenCreateStandardProjectModal();
        }

        public void OpenCreateStandardProjectModal()
        {
            if (!CheckProjectsLimitationBeforeOpenCreateNewProjectAnyModal())
            {
                return;
            }
            if (createStandardProjectModal != null)
            {
                createStandardProjectModal.Focus();
                return;
            }
            if (createExpresstProjectModal != null)
            {
                createExpresstProjectModal.Close();
            }
            createStandardProjectModal = new DEProjectCubeProperties(this);
            createStandardProjectModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            createStandardProjectModal.Closed += CreateStandardProjectModal_Closed;
            ShowMainWindowShadowOverlay(createStandardProjectModal);
            createStandardProjectModal.Show();
        }

        private void CreateStandardProjectModal_Closed(object sender, EventArgs e)
        {
            HideMainWindowShadowOverlay();
            createStandardProjectModal = null;
        }

        public void OpenChooseCreatingProjectWay()
        {
            if (ChooseWayForNewProjectModal != null)
            {
                ChooseWayForNewProjectModal.Close();
            }
            ChooseWayForNewProjectModal = new DEWhichProjectTypeYouWantToCreate(this);
            ChooseWayForNewProjectModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ChooseWayForNewProjectModal.Closed += ChooseWayForNewProjectModal_Closed;
            ShowMainWindowShadowOverlay(ChooseWayForNewProjectModal);
            ChooseWayForNewProjectModal.Show();
        }

        private void ChooseWayForNewProjectModal_Closed(object sender, EventArgs e)
        {
            HideMainWindowShadowOverlay();
            ChooseWayForNewProjectModal = null;
        }

        public DEProjectCube ProcessAddDataExcavatorTaskIntoUI(DEProjectLink ProjectObjectContainer)
        {
            DETasksFactoryCoreStorage.AddTask(ProjectObjectContainer.TaskLink);
            DEUIProjectsStorage.SavedProjectsLinks.Add(ProjectObjectContainer);
            DEProjectCube dEProjectCube2 = (ProjectObjectContainer.ProjectCubeLink = new DEProjectCube(this, ProjectObjectContainer));
            DEProjectsArea.Children.Add(dEProjectCube2);
            return dEProjectCube2;
        }

        private void MainMenu_ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainMenu_LicenseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowLicenseInformationModal();
        }

        public void ShowLicenseInformationModal()
        {
            if (licenseInfoModal != null)
            {
                licenseInfoModal.Focus();
                return;
            }
            licenseInfoModal = new DELicenseInfo(this);
            licenseInfoModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            licenseInfoModal.Closed += LicenseInfoModal_Closed;
            ShowMainWindowShadowOverlay(licenseInfoModal);
            licenseInfoModal.Show();
        }

        private void MainMenu_HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (deHelpWindowModal != null)
            {
                deHelpWindowModal.Focus();
                return;
            }
            deHelpWindowModal = new DEHelpWindow(this);
            deHelpWindowModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            deHelpWindowModal.Closed += DeHelpWindowModal_Closed;
            ShowMainWindowShadowOverlay(deHelpWindowModal);
            deHelpWindowModal.Show();
        }

        private void DeHelpWindowModal_Closed(object sender, EventArgs e)
        {
            deHelpWindowModal = null;
            HideMainWindowShadowOverlay();
        }

        private void MainMenu_SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (deAppCommonPropertiesModal != null)
            {
                deAppCommonPropertiesModal.Focus();
                return;
            }
            deAppCommonPropertiesModal = new DECommonPropertiesWindow(this);
            deAppCommonPropertiesModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            deAppCommonPropertiesModal.Closed += DeAppCommonPropertiesModal_Closed;
            ShowMainWindowShadowOverlay(deAppCommonPropertiesModal);
            deAppCommonPropertiesModal.Show();
        }

        private void DeAppCommonPropertiesModal_Closed(object sender, EventArgs e)
        {
            deAppCommonPropertiesModal = null;
            HideMainWindowShadowOverlay();
        }

        private void TopMenu_CreateNewProjectFromImportFile_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckProjectsLimitationBeforeOpenCreateNewProjectAnyModal())
            {
                return;
            }
            if (importProjectFromFileModal != null)
            {
                importProjectFromFileModal.Focus();
                return;
            }
            importProjectFromFileModal = new DEProjectCubeProperties(this, null, OpenImportProjectDialog: true);
            importProjectFromFileModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            importProjectFromFileModal.Closed += ImportProjectFromFileModal_Closed;
            /*if (!importProjectFromFileModal.isProjectImportCancelled)
            {
                importProjectFromFileModal.Show();
            }
            else
            {
                importProjectFromFileModal = null;
            }*/
        }

        private void ImportProjectFromFileModal_Closed(object sender, EventArgs e)
        {
            importProjectFromFileModal = null;
            HideMainWindowShadowOverlay();
        }

        private void TopMenu_ProjectMaster_Click(object sender, RoutedEventArgs e)
        {
            if (CheckProjectsLimitationBeforeOpenCreateNewProjectAnyModal())
            {
                OpenChooseCreatingProjectWay();
            }
        }

        internal void ShowMainWindowShadowOverlay(Window showedModalLink)
        {
            overlayController.ShowOverlay(showedModalLink);
        }

        internal void HideMainWindowShadowOverlay()
        {
            overlayController.HideOverlay();
        }

        internal void PreventShadowOverlayHiding()
        {
            overlayController.PreventOverlayHiding();
        }



    }
}