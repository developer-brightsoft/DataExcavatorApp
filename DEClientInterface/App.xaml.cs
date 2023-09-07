using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DEClientInterface.Logic;
using ExcavatorSharp.Common;

namespace DEClientInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary> 
    public partial class App : Application
    {

        public static string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public App()
        {
            try
            {
                IOCommon.CheckAppIOPermissions();
            }
            catch (Exception)
            {
                MessageBox.Show("The program was unable to access the C:\\ProgramData\\PIC\\DataExcavator folder. The access to this folder is necessary for correct work of the program. Probably you don't have enough rights to work in the system. Try to start the program in administrator mode.", "Application launch is interrupted", MessageBoxButton.OK, MessageBoxImage.Hand);
                Shutdown();
                return;
            }
            Logger.InitLogger();
            Logger.LogInformation("Application started");
            base.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception exception = e.Exception;
            e.Handled = true;
            MessageBox.Show($"Fatal error: {exception.ToString()}. Please, contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            Logger.LogError("Application fatal exception thrown", exception);
            TrySendAppCrashReport(exception, "Exception intercepted at application level");
        }

        public static void TrySendAppCrashReport(Exception exception, string AdditionalData = "")
        {
            if (DEConfig.AllowToSendCrashReports)
            {
                /*APICrashReporter.SendCrashReport("UI", GetAppVersion(), exception, AdditionalData);*/
            }
        }

        /*[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri a = new Uri("mainwindow.xaml", UriKind.Relative);
                base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
				Uri resourceLocator = new Uri("/DEClientInterface;component/app.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		[STAThread]
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}*/

    }
}
