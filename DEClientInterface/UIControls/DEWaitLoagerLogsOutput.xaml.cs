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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEClientInterface.UIControls
{
    /// <summary>
    /// Interaction logic for DEWaitLoagerLogsOutput.xaml
    /// </summary>
    public partial class DEWaitLoagerLogsOutput : UserControl
    {
        private List<ListBoxItem> LogItemsListItemsSource = new List<ListBoxItem>();

        public DEWaitLoagerLogsOutput()
        {
            InitializeComponent();
        }

        public void ClearLogs()
        {
            base.Dispatcher.Invoke(delegate
            {
                LogsOutputListBox.ItemsSource = null;
                LogsOutputListBox.Items.Refresh();
                LogItemsListItemsSource.Clear();
                LogsOutputListBox.ItemsSource = LogItemsListItemsSource;
                LogsOutputListBox.Items.Refresh();
            });
        }

        public void AddLogEntry(string LogEntryName)
        {
            base.Dispatcher.Invoke(delegate
            {
                LogItemsListItemsSource.Add(new ListBoxItem
                {
                    Content = LogEntryName
                });
                LogsOutputListBox.Items.Refresh();
                LogsOutputListBox.SelectedIndex = LogsOutputListBox.Items.Count - 1;
                LogsOutputListBox.ScrollIntoView(LogsOutputListBox.SelectedItem);
            });
        }
    }
}
