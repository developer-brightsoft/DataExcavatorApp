using DEClientInterface.Logic;
using DEClientInterface.Objects;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for DELogs.xaml
    /// </summary>
    public partial class DELogs : Window
    {

        private DEProjectLink ProjectLink { get; set; }

        public DELogs(DEProjectLink ProjectLink)
        {
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", ProjectLink.ProjectName);
            this.ProjectLink = ProjectLink;
        }

        public void Initialize()
        {
            ProjectLink.TaskLink.LogMessageAdded += TaskLink_LogMessageAdded;
            RefreshHistoryLogFilesList();
        }

        private void RefreshHistoryLogFilesList()
        {
            DataExcavatorTasksLogger dataExcavatorTasksLogger = new DataExcavatorTasksLogger(ProjectLink.ProjectPath);
            List<string> logFilesList = dataExcavatorTasksLogger.GetLogFilesList();
            for (int i = 0; i < logFilesList.Count; i++)
            {
                FileInfo fileInfo = new FileInfo(logFilesList[i]);
                ListBoxItem listBoxItem = new ListBoxItem
                {
                    Tag = logFilesList[i],
                    Content = fileInfo.Name
                };
                listBoxItem.MouseDoubleClick += NextListBoxItem_MouseDoubleClick;
                LogFilesHistoryList.Items.Add(listBoxItem);
            }
        }

        private void NextListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;
            string path = listBoxItem.Tag.ToString();
            string text = string.Empty;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during reading common logs file", ex);
                App.TrySendAppCrashReport(ex, "Error while reading logs file");
            }
            LogFileContent.Text = text;
        }

        private void TaskLink_LogMessageAdded(DataExcavatorTaskEventCallback Callback)
        {
            base.Dispatcher.Invoke(delegate
            {
                if (CurrentSessionLogsListBox.Items.Count > 200)
                {
                    CurrentSessionLogsListBox.Items.RemoveAt(0);
                }
                CurrentSessionLogsListBox.Items.Add(new ListBoxItem
                {
                    Content = Callback.EventMessage
                });
                CurrentSessionLogsListBox.SelectedIndex = CurrentSessionLogsListBox.Items.Count - 1;
                CurrentSessionLogsListBox.ScrollIntoView(CurrentSessionLogsListBox.SelectedItem);
            });
        }

        public void ReleaseResources()
        {
            ProjectLink.TaskLink.LogMessageAdded -= TaskLink_LogMessageAdded;
            foreach (ListBoxItem item in (IEnumerable)LogFilesHistoryList.Items)
            {
                item.MouseDoubleClick -= NextListBoxItem_MouseDoubleClick;
            }
        }
    }
}
