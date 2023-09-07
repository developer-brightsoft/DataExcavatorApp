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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DECEFBehaviorEditModal.xaml
    /// </summary>
    public partial class DECEFBehaviorEditModal : Window
    {

        private CEFCrawlingBehavior behaviorLink { get; set; }

        private DEProjectCubeProperties parentWindow { get; set; }

        public DECEFBehaviorEditModal()
        {
            InitializeComponent();
            base.Title = "Edit CEF behavior";
        }

        public DECEFBehaviorEditModal(DEProjectCubeProperties parentWindow, CEFCrawlingBehavior behaviorLink = null)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;
            this.behaviorLink = behaviorLink;
            LoadBehavior();
            if (this.parentWindow.LinkedUITask != null)
            {
                base.Title = base.Title.Replace("PROJECTNAME", this.parentWindow.LinkedUITask.ProjectName);
            }
            else
            {
                base.Title = "Edit CEF behavior";
            }
        }

        private void LoadBehavior()
        {
            if (behaviorLink != null)
            {
                URLsToApplyCEFCrawlingBehaviorSubstringsPattern.Text = behaviorLink.PageUrlSubstringPattern;
                BehaviorStep1_WaitAfterPageLoadedSecondsTextArea.Text = behaviorLink.WaitAfterPageLoaded_InSeconds_Step1.ToString();
                BehaviorStep2_JSScriptToExecute.Document = new FlowDocument(new Paragraph(new Run(behaviorLink.JSScriptToExecute_Step2)));
                BehaviorStep3_WaitAfterJSScriptExecutedSeconds.Text = behaviorLink.WaitAfterpageLoaded_InSeconds_Step3.ToString();
                BehaviorStep4_JSScriptToExecuteAndCheckResults.Document = new FlowDocument(new Paragraph(new Run(behaviorLink.JSScriptToExecuteAfterPageHTMLCodeGrabbed)));
                CheckBehaviorFinishRuleSelectBox.SelectedItem = EnumToItemsSource.GetDescriptionFromValue(behaviorLink.LeavePageRule);
                BehaviorStep6_WaitAfterJSScriptExecutedSeconds.Text = behaviorLink.LeavePageRuleValue;
            }
        }

        private void ApplyBehaviorButton_Click(object sender, RoutedEventArgs e)
        {
            WaitLoader.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    string BehaviorApplyingURL = string.Empty;
                    string Step1BehaviorWaitAfterPageLoaded = string.Empty;
                    string Step2JSScriptToExecuteWithoutRetults = string.Empty;
                    string Step3BehaviorWaitAfterFirstJSScriptExecuted = string.Empty;
                    string Step4JSScriptToExecuteWithResultsCheck = string.Empty;
                    CEFCrawlingPageLeaveEventType PageLeaveRule = CEFCrawlingPageLeaveEventType.LeavePageAfterIndexing;
                    string LeavePageRuleCheckValue = string.Empty;
                    int result = 0;
                    int result2 = 0;
                    base.Dispatcher.Invoke(delegate
                    {
                        BehaviorApplyingURL = URLsToApplyCEFCrawlingBehaviorSubstringsPattern.Text.Trim();
                        Step1BehaviorWaitAfterPageLoaded = BehaviorStep1_WaitAfterPageLoadedSecondsTextArea.Text.Trim();
                        Step2JSScriptToExecuteWithoutRetults = new TextRange(BehaviorStep2_JSScriptToExecute.Document.ContentStart, BehaviorStep2_JSScriptToExecute.Document.ContentEnd).Text.Trim();
                        Step3BehaviorWaitAfterFirstJSScriptExecuted = BehaviorStep3_WaitAfterJSScriptExecutedSeconds.Text.Trim();
                        Step4JSScriptToExecuteWithResultsCheck = new TextRange(BehaviorStep4_JSScriptToExecuteAndCheckResults.Document.ContentStart, BehaviorStep4_JSScriptToExecuteAndCheckResults.Document.ContentEnd).Text.Trim();
                        if (CheckBehaviorFinishRuleSelectBox.SelectedItem != null)
                        {
                            PageLeaveRule = EnumToItemsSource.GetValueFromDescription<CEFCrawlingPageLeaveEventType>(CheckBehaviorFinishRuleSelectBox.SelectedItem.ToString());
                        }
                        LeavePageRuleCheckValue = BehaviorStep6_WaitAfterJSScriptExecutedSeconds.Text.Trim();
                        URLsToApplyCEFCrawlingBehaviorSubstringsPattern.MarkAsCorrectlyCompleted();
                        CheckBehaviorFinishRuleSelectBox.MarkAsCorrectlyCompleted();
                        BehaviorStep1_WaitAfterPageLoadedSecondsTextArea.MarkAsCorrectlyCompleted();
                        BehaviorStep3_WaitAfterJSScriptExecutedSeconds.MarkAsCorrectlyCompleted();
                    });
                    bool IsErrorDetected = false;
                    if (BehaviorApplyingURL == string.Empty)
                    {
                        IsErrorDetected = true;
                        base.Dispatcher.Invoke(delegate
                        {
                            URLsToApplyCEFCrawlingBehaviorSubstringsPattern.MarkAsUncorrectlyCompleted();
                        });
                    }
                    base.Dispatcher.Invoke(delegate
                    {
                        if (CheckBehaviorFinishRuleSelectBox.SelectedItem == null)
                        {
                            IsErrorDetected = true;
                            CheckBehaviorFinishRuleSelectBox.MarkAsUncorrectlyCompleted();
                        }
                    });
                    if (Step1BehaviorWaitAfterPageLoaded.Length == 0)
                    {
                        result = 0;
                    }
                    else
                    {
                        if (!int.TryParse(Step1BehaviorWaitAfterPageLoaded, out result))
                        {
                            IsErrorDetected = true;
                            base.Dispatcher.Invoke(delegate
                            {
                                BehaviorStep1_WaitAfterPageLoadedSecondsTextArea.MarkAsUncorrectlyCompleted();
                            });
                        }
                        if (result < 0)
                        {
                            IsErrorDetected = true;
                            base.Dispatcher.Invoke(delegate
                            {
                                BehaviorStep1_WaitAfterPageLoadedSecondsTextArea.MarkAsUncorrectlyCompleted();
                            });
                        }
                    }
                    if (Step3BehaviorWaitAfterFirstJSScriptExecuted.Length == 0)
                    {
                        result2 = 0;
                    }
                    else
                    {
                        if (!int.TryParse(Step3BehaviorWaitAfterFirstJSScriptExecuted, out result2))
                        {
                            IsErrorDetected = true;
                            base.Dispatcher.Invoke(delegate
                            {
                                BehaviorStep3_WaitAfterJSScriptExecutedSeconds.MarkAsUncorrectlyCompleted();
                            });
                        }
                        if (result2 < 0)
                        {
                            IsErrorDetected = true;
                            base.Dispatcher.Invoke(delegate
                            {
                                BehaviorStep3_WaitAfterJSScriptExecutedSeconds.MarkAsUncorrectlyCompleted();
                            });
                        }
                    }
                    if (IsErrorDetected)
                    {
                        MessageBox.Show("Some of fields have wrong values. Please, fix the errors.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                        base.Dispatcher.Invoke(delegate
                        {
                            WaitLoader.Visibility = Visibility.Hidden;
                        });
                    }
                    else
                    {
                        CEFCrawlingBehavior item = new CEFCrawlingBehavior(BehaviorApplyingURL, result, Step2JSScriptToExecuteWithoutRetults, result2, PageLeaveRule, Step4JSScriptToExecuteWithResultsCheck, LeavePageRuleCheckValue);
                        parentWindow.ActualCEFCrawlingBehaviors.Clear();
                        parentWindow.ActualCEFCrawlingBehaviors.Add(item);
                        parentWindow.ActualCEFCrawlingBehaviors = new List<CEFCrawlingBehavior> { item };
                        base.Dispatcher.Invoke(delegate
                        {
                            parentWindow.DETaskCrawlingServer_CEFCrawlingBehaviors_TextBlock.Text = "JS behavior specified";
                            WaitLoader.Visibility = Visibility.Hidden;
                            Close();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error during CEF behavior saving", ex);
                    App.TrySendAppCrashReport(ex, "Error during CEF behavior saving");
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoader.Visibility = Visibility.Hidden;
                    });
                    MessageBox.Show("Something went wrong. Please, contact support", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            });
        }

        private void DeleteBehaviorButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.ActualCEFCrawlingBehaviors = new List<CEFCrawlingBehavior>();
            parentWindow.DETaskCrawlingServer_CEFCrawlingBehaviors_TextBlock.Text = string.Empty;
            Close();
        }
    }
}
