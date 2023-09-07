using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.CEF;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEWebsiteAuthBehavior.xaml
    /// </summary>
    public partial class DEWebsiteAuthBehavior : Window
    {
        

        private CEFWebsiteAuthBehavior AuthenticationBehaviorLink { get; set; }

        private DEProjectCubeProperties ParentWindowLink { get; set; }

        private UIOverlayController overlayController { get; set; }

        public DEWebsiteAuthBehavior(DEProjectCubeProperties ParentWindowLink, CEFWebsiteAuthBehavior AuthenticationBehaviorLink = null)
        {
            this.AuthenticationBehaviorLink = AuthenticationBehaviorLink;
            this.ParentWindowLink = ParentWindowLink;
            InitializeComponent();
            if (this.ParentWindowLink.LinkedUITask != null)
            {
                base.Title = base.Title.Replace("PROJECTNAME", this.ParentWindowLink.LinkedUITask.ProjectName);
            }
            else
            {
                base.Title = "Website authentication behavior";
            }
            overlayController = new UIOverlayController(this, ShadowOverlay);
            if (AuthenticationBehaviorLink != null)
            {
                LoadAuthBehaviorLink();
            }
        }

        public void TransferLinksFromCommonCrawlingProperties(string RespectedPageLinksURLContent, string RestrictedPageLinksURLContent)
        {
            _ = RespectedPageLinksURLContent.Length;
            if (RespectedPageLinksURLContent.Length == 0)
            {
                RespectedPageLinksURLContent = "*";
            }
            CEFAuthBehaviorCheckAuthURLsTextArea.Text = RespectedPageLinksURLContent;
            CEFAuthBehaviorRestrictCheckAuthURLsTextArea.Text = RestrictedPageLinksURLContent;
        }

        private void LoadAuthBehaviorLink()
        {
            string text = string.Empty;
            if (AuthenticationBehaviorLink.PagesURLSubstringsToCheckLogin != null)
            {
                text = string.Join(",", AuthenticationBehaviorLink.PagesURLSubstringsToCheckLogin);
            }
            CEFAuthBehaviorCheckAuthURLsTextArea.Text = text;
            string text2 = string.Empty;
            if (AuthenticationBehaviorLink.PagesURLSubstringsCheckRestrictions != null)
            {
                text2 = string.Join(",", AuthenticationBehaviorLink.PagesURLSubstringsCheckRestrictions);
            }
            CEFAuthBehaviorRestrictCheckAuthURLsTextArea.Text = text2;
            CEFAuthBehaviorAuthPageAddressTextArea.Text = AuthenticationBehaviorLink.WebsiteLoginPageURL;
            CEFAuthBehaviorLoggedOnSubstringTextArea.Text = AuthenticationBehaviorLink.CheckUserLoggedInDocumentHTMLSubstring;
            CEFAuthBehaviorWaitAfterLoginTextArea.Text = AuthenticationBehaviorLink.WaitInSecondsBeforeAndAfterLoginScript.ToString();
            CEFAuthBehaviorLoginJSScriptTextArea.Document.Blocks.Clear();
            CEFAuthBehaviorLoginJSScriptTextArea.Document.Blocks.Add(new Paragraph(new Run(AuthenticationBehaviorLink.UserLoginJSScript)));
        }

        private void SaveAuthBehaviorButton_Click(object sender, RoutedEventArgs e)
        {
            HwndSource rectangleHWND = null;
            base.Dispatcher.Invoke(delegate
            {
                rectangleHWND = (HwndSource)PresentationSource.FromVisual(this);
            });
            WaitLoader.Visibility = Visibility.Visible;
            WaitLoader.ClearLogs();
            Task.Run(delegate
            {
                WaitLoader.AddLogEntry(string.Format("{0}: {1}", DateTime.Now.ToString(), "Checking auth params..."));
                try
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        CEFAuthBehaviorCheckAuthURLsTextArea.MarkAsCorrectlyCompleted();
                        CEFAuthBehaviorRestrictCheckAuthURLsTextArea.MarkAsCorrectlyCompleted();
                        CEFAuthBehaviorAuthPageAddressTextArea.MarkAsCorrectlyCompleted();
                        CEFAuthBehaviorLoggedOnSubstringTextArea.MarkAsCorrectlyCompleted();
                        CEFAuthBehaviorWaitAfterLoginTextArea.MarkAsCorrectlyCompleted();
                        CEFAuthBehaviorLoginJSScriptTextArea.MarkAsCorrectlyCompleted();
                    });
                    string[] AllowedURLs = null;
                    string[] RestrictedURLs = null;
                    string CheckAuthURLsTextAreaValue = string.Empty;
                    string RestrictURLsToCheckAuth = string.Empty;
                    string WebsiteLoginURL = string.Empty;
                    string IsLoggedInSubstring = string.Empty;
                    string WaitingTimeBeforeAndAfterLogin = string.Empty;
                    string JSLoginScript = string.Empty;
                    int WaitingTimeBeforeAndAfterLogin_IntValue = -1;
                    base.Dispatcher.Invoke(delegate
                    {
                        CheckAuthURLsTextAreaValue = CEFAuthBehaviorCheckAuthURLsTextArea.Text.Trim();
                        if (CheckAuthURLsTextAreaValue != null && CheckAuthURLsTextAreaValue.Length > 0)
                        {
                            AllowedURLs = CheckAuthURLsTextAreaValue.Split(',');
                        }
                        RestrictURLsToCheckAuth = CEFAuthBehaviorRestrictCheckAuthURLsTextArea.Text.Trim();
                        if (RestrictURLsToCheckAuth != null && RestrictURLsToCheckAuth.Length > 0)
                        {
                            RestrictedURLs = RestrictURLsToCheckAuth.Split(',');
                        }
                        WebsiteLoginURL = CEFAuthBehaviorAuthPageAddressTextArea.Text.Trim();
                        IsLoggedInSubstring = CEFAuthBehaviorLoggedOnSubstringTextArea.Text.Trim();
                        WaitingTimeBeforeAndAfterLogin = CEFAuthBehaviorWaitAfterLoginTextArea.Text.Trim();
                        int.TryParse(WaitingTimeBeforeAndAfterLogin, out WaitingTimeBeforeAndAfterLogin_IntValue);
                        try
                        {
                            JSLoginScript = new TextRange(CEFAuthBehaviorLoginJSScriptTextArea.Document.ContentStart, CEFAuthBehaviorLoginJSScriptTextArea.Document.ContentEnd).Text.Trim();
                        }
                        catch (Exception)
                        {
                        }
                    });
                    bool flag = false;
                    bool flag2 = true;
                    if (AllowedURLs == null && RestrictedURLs == null)
                    {
                        flag = true;
                        base.Dispatcher.Invoke(delegate
                        {
                            CEFAuthBehaviorCheckAuthURLsTextArea.MarkAsUncorrectlyCompleted();
                            CEFAuthBehaviorRestrictCheckAuthURLsTextArea.MarkAsUncorrectlyCompleted();
                        });
                    }
                    if (WebsiteLoginURL.Length == 0)
                    {
                        flag = true;
                        flag2 = false;
                        base.Dispatcher.Invoke(delegate
                        {
                            CEFAuthBehaviorAuthPageAddressTextArea.MarkAsUncorrectlyCompleted();
                        });
                    }
                    else if (!Uri.IsWellFormedUriString(WebsiteLoginURL, UriKind.Absolute))
                    {
                        flag = true;
                        flag2 = false;
                        base.Dispatcher.Invoke(delegate
                        {
                            CEFAuthBehaviorAuthPageAddressTextArea.MarkAsUncorrectlyCompleted();
                        });
                    }
                    if (IsLoggedInSubstring.Length == 0)
                    {
                        flag = true;
                        base.Dispatcher.Invoke(delegate
                        {
                            CEFAuthBehaviorLoggedOnSubstringTextArea.MarkAsUncorrectlyCompleted();
                        });
                    }
                    if (WaitingTimeBeforeAndAfterLogin_IntValue == -1)
                    {
                        flag = true;
                        base.Dispatcher.Invoke(delegate
                        {
                            CEFAuthBehaviorWaitAfterLoginTextArea.MarkAsUncorrectlyCompleted();
                        });
                    }
                    if (JSLoginScript.Length == 0)
                    {
                        flag = true;
                        base.Dispatcher.Invoke(delegate
                        {
                            CEFAuthBehaviorLoginJSScriptTextArea.MarkAsUncorrectlyCompleted();
                        });
                    }
                    if (flag)
                    {
                        base.Dispatcher.Invoke(delegate
                        {
                            WaitLoader.Visibility = Visibility.Hidden;
                        });
                    }
                    else
                    {
                        ParentWindowLink.WebsiteAuthenticaionBehavior = new CEFWebsiteAuthBehavior(AllowedURLs, RestrictedURLs, WebsiteLoginURL, WaitingTimeBeforeAndAfterLogin_IntValue, JSLoginScript, IsLoggedInSubstring);
                        base.Dispatcher.Invoke(delegate
                        {
                            ParentWindowLink.DETaskCrawlingServer_CEFAuthenticationBehavior_TextBlock.Text = "Authentication is used";
                            Close();
                        });
                    }
                }
                catch (Exception ex)
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoader.Visibility = Visibility.Hidden;
                    });
                    MessageBox.Show("Something went wrong. Please, contact support. Error text = " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    Logger.LogError("Error in saving Autg behavior", ex);
                    App.TrySendAppCrashReport(ex, "Error in saving Auth behavior");
                }
            });
        }

        private void DeleteAuthBehaviorButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationBehaviorLink = null;
            ParentWindowLink.DETaskCrawlingServer_CEFAuthenticationBehavior_TextBlock.Text = "";
            Close();
        }

        private void HelpWindow_Closed(object sender, EventArgs e)
        {
            HideOverlay();
        }

        private void HelpButton_CEFAuthBehaviorCheckAuthURLs_Click(object sender, RoutedEventArgs e)
        {
            Window window = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthRespectedURLs");
            window.Closed += HelpWindow_Closed;
            ShowOverlay(window);
        }

        private void HelpButton_CEFAuthBehaviorRestrictCheckAuthURLs_Click(object sender, RoutedEventArgs e)
        {
            Window window = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthRestrictedURLs");
            window.Closed += HelpWindow_Closed;
            ShowOverlay(window);
        }

        private void HelpButton_CEFAuthBehaviorAuthPageAddress_Click(object sender, RoutedEventArgs e)
        {
            Window window = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthLoginPageAddress");
            window.Closed += HelpWindow_Closed;
            ShowOverlay(window);
        }

        private void HelpButton_CEFAuthBehaviorLoggedOnSubstring_Click(object sender, RoutedEventArgs e)
        {
            Window window = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthBehaviorLoggedOnSubstring");
            window.Closed += HelpWindow_Closed;
            ShowOverlay(window);
        }

        private void HelpButton_CEFAuthBehaviorWaitAfterLogin_Click(object sender, RoutedEventArgs e)
        {
            Window window = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthBehaviorWaitBeforeAndAfterLogin");
            window.Closed += HelpWindow_Closed;
            ShowOverlay(window);
        }

        private void HelpButton_CEFAuthBehaviorLoginJSScript_Click(object sender, RoutedEventArgs e)
        {
            Window window = HelpingButtonsDataStorage.ShowHelpWindow("CEFAuthBehaviorLoginScriptItself");
            window.Closed += HelpWindow_Closed;
            ShowOverlay(window);
        }

        public void ShowOverlay(Window overlayWindowLink)
        {
            overlayController.ShowOverlay(overlayWindowLink);
        }

        public void HideOverlay()
        {
            overlayController.HideOverlay();
        }
    }
}
