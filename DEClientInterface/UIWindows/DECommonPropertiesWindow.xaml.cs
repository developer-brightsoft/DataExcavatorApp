using DEClientInterface.Logic;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Common;
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
    /// Interaction logic for DECommonPropertiesWindow.xaml
    /// </summary>
    public partial class DECommonPropertiesWindow : Window
    {
        private MainWindow MainWindowLink { get; set; }

        public DECommonPropertiesWindow(MainWindow MainWindowLink)
        {
            InitializeComponent();
            this.MainWindowLink = MainWindowLink;
            Dictionary<string, object> dEPropertiesFromCore = CorePropertiesStorage.GetDEPropertiesFromCore();
            DECEFUserAgentTextArea.Text = dEPropertiesFromCore["CEFUserAgentCommon"].ToString();
            DEProxyTestingLink.Text = dEPropertiesFromCore["ProxyServerTestingResourceLink"].ToString();
            DEProxyResponseTest.Text = dEPropertiesFromCore["ProxyServerTestingExpectedSubstringInSourceCode"].ToString();
            if (dEPropertiesFromCore["TrustedHostsGlobal"] != null)
            {
                DETrustedHostsGlobal.Text = string.Join(", ", dEPropertiesFromCore["TrustedHostsGlobal"] as string[]);
            }
            DEHttpConnectionsCount.Text = dEPropertiesFromCore["HttpConnectionsMaxCount"].ToString();
            DEExpectOneHundredContinue.SelectItemByTag(dEPropertiesFromCore["HttpWebRequest_Expect100Continue"].ToString());
            DECheckCertificateRevocList.SelectItemByTag(dEPropertiesFromCore["HttpWebRequest_CheckCertificateRevocationList"].ToString());
            DEMaxCrawlDelayWaitSeconds.Text = dEPropertiesFromCore["MaximumCrawlDelayValueInSeconds"].ToString();
            DEAllowToSendCrashReports.SelectItemByTag(dEPropertiesFromCore["AllowToSendCrashReports"].ToString());
        }

        private void ApplyCommonProperties_Click(object sender, RoutedEventArgs e)
        {
            DECEFUserAgentTextArea.MarkAsCorrectlyCompleted();
            DEHttpConnectionsCount.MarkAsCorrectlyCompleted();
            DEExpectOneHundredContinue.MarkAsCorrectlyCompleted();
            DECheckCertificateRevocList.MarkAsCorrectlyCompleted();
            DEMaxCrawlDelayWaitSeconds.MarkAsCorrectlyCompleted();
            string text = DECEFUserAgentTextArea.Text.Trim();
            string proxyServerTestingResourceLink = DEProxyTestingLink.Text.Trim();
            string proxyServerTestingExpectedSubstringInSourceCode = DEProxyResponseTest.Text.Trim();
            string text2 = DETrustedHostsGlobal.Text.Trim();
            string text3 = DEHttpConnectionsCount.Text.Trim();
            ComboBoxItem comboBoxItem = DEExpectOneHundredContinue.SelectedValue as ComboBoxItem;
            ComboBoxItem comboBoxItem2 = DECheckCertificateRevocList.SelectedValue as ComboBoxItem;
            ComboBoxItem comboBoxItem3 = DEAllowToSendCrashReports.SelectedValue as ComboBoxItem;
            string text4 = DEMaxCrawlDelayWaitSeconds.Text.Trim();
            bool flag = false;
            if (text.Length == 0)
            {
                DECEFUserAgentTextArea.MarkAsUncorrectlyCompleted();
            }
            if (text3.Length == 0)
            {
                DEHttpConnectionsCount.MarkAsUncorrectlyCompleted();
            }
            if (!int.TryParse(text3, out var result))
            {
                DEHttpConnectionsCount.MarkAsUncorrectlyCompleted();
            }
            if (comboBoxItem == null)
            {
                DEExpectOneHundredContinue.MarkAsUncorrectlyCompleted();
            }
            if (comboBoxItem2 == null)
            {
                DECheckCertificateRevocList.MarkAsUncorrectlyCompleted();
            }
            if (text4.Length == 0)
            {
                DEMaxCrawlDelayWaitSeconds.MarkAsUncorrectlyCompleted();
            }
            if (!int.TryParse(text4, out result))
            {
                DEMaxCrawlDelayWaitSeconds.MarkAsUncorrectlyCompleted();
            }
            if (!flag)
            {
                string[] trustedHostsGlobal = text2.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
                DEConfig.CEFUserAgentCommon = text;
                DEConfig.ProxyServerTestingResourceLink = proxyServerTestingResourceLink;
                DEConfig.ProxyServerTestingExpectedSubstringInSourceCode = proxyServerTestingExpectedSubstringInSourceCode;
                DEConfig.TrustedHostsGlobal = trustedHostsGlobal;
                DEConfig.HttpConnectionsMaxCount = int.Parse(text3);
                DEConfig.HttpWebRequest_Expect100Continue = Convert.ToBoolean(comboBoxItem.Tag.ToString());
                DEConfig.HttpWebRequest_CheckCertificateRevocationList = Convert.ToBoolean(comboBoxItem2.Tag.ToString());
                DEConfig.MaximumCrawlDelayValueInSeconds = int.Parse(text4);
                DEConfig.AllowToSendCrashReports = Convert.ToBoolean(comboBoxItem3.Tag.ToString());
                DEConfig.ApplyPropertiesSet();
                MainWindowLink.CorePropertiesLoader.SaveCoreProperties();
                Close();
            }
        }
    }
}
