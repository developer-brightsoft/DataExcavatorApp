using CefSharp;
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
    /// Interaction logic for DECaptchaManualSolver.xaml
    /// </summary>
    public partial class DECaptchaManualSolver : Window
    {
        private bool IsCaptchaResolved_ButtonClicked = false;

        public DECaptchaManualSolver(string ProjectName)
        {
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", ProjectName);
            base.Closing += DECaptchaManualSolver_Closing;
        }

        private void DECaptchaManualSolver_Closing(object sender, CancelEventArgs e)
        {
            if (!IsCaptchaResolved_ButtonClicked)
            {
                /*GC.KeepAlive(SolvingArgs.CEFBrowserLink);
				SolvingArgs.CaptchaSolvedCallback();
				MainWindow.BrowsersLinks.Remove(SolvingArgs.CEFBrowserLink);
				SolvingArgs.CEFBrowserLink.ConsoleMessage -= CEFBrowserLink_ConsoleMessage;
				SolvingArgs.CEFBrowserLink.CleanupElement = null;
				ContentContainer.Content = null;
				SolvingArgs = null;*/
            }
        }

        /*public void AssociateChromium(CaptchaManualSolvingDemandArgs SolvingArgs)
		{
			this.SolvingArgs = SolvingArgs;
			ContentContainer.Content = SolvingArgs.CEFBrowserLink;
			MainWindow.BrowsersLinks.Add(SolvingArgs.CEFBrowserLink);
			SolvingArgs.CEFBrowserLink.ConsoleMessage += CEFBrowserLink_ConsoleMessage;
		}*/

        private void CEFBrowserLink_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            base.Dispatcher.Invoke(delegate
            {
                ListBoxItem newItem = new ListBoxItem
                {
                    Content = $"{DateTime.Now.ToString()} | Source #{e.Source.ToString()}, Line #{e.Line.ToString()} | {e.Message}"
                };
                BrowserInstanceLogsListBox.Items.Add(newItem);
            });
        }

        private void CaptchaSolvedButton_Click(object sender, RoutedEventArgs e)
        {
            IsCaptchaResolved_ButtonClicked = true;
            /*GC.KeepAlive(SolvingArgs.CEFBrowserLink);
			SolvingArgs.CaptchaSolvedCallback();
			MainWindow.BrowsersLinks.Remove(SolvingArgs.CEFBrowserLink);
			SolvingArgs.CEFBrowserLink.ConsoleMessage -= CEFBrowserLink_ConsoleMessage;
			SolvingArgs.CEFBrowserLink.CleanupElement = null;
			ContentContainer.Content = null;
			SolvingArgs = null;*/
            Close();
        }

        private void ReloadPageButton_Click(object sender, RoutedEventArgs e)
        {
            /*SolvingArgs.CEFBrowserLink.GetBrowser().Reload(ignoreCache: true);*/
        }
    }
}
