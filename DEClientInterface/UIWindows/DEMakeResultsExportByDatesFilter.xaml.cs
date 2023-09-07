using DEClientInterface.Controls;
using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Exporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEClientInterface.UIWindows
{
    /// <summary>
    /// Interaction logic for DEMakeResultsExportByDatesFilter.xaml
    /// </summary>
    public partial class DEMakeResultsExportByDatesFilter : Window
    {
        private DEProjectCube DEProjectCubeLink { get; set; }

        public DEMakeResultsExportByDatesFilter(DEProjectCube ProjectCubeLink)
        {
            DEProjectCubeLink = ProjectCubeLink;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", DEProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
        }

        private void MakeDataExportButton_Click(object sender, RoutedEventArgs e)
        {
            DEStartDateDatePicker.MarkAsCorrectlyCompleted();
            DEEndDateDatePicker.MarkAsCorrectlyCompleted();
            DEExportFormatSelector.MarkAsCorrectlyCompleted();
            DEExportDataTypeSelector.MarkAsCorrectlyCompleted();
            DEExportSequencesSeparator.MarkAsCorrectlyCompleted();
            DEExportPathTextArea.MarkAsCorrectlyCompleted();
            bool flag = false;
            if (!DEStartDateDatePicker.SelectedDate.HasValue)
            {
                flag = true;
                DEStartDateDatePicker.MarkAsUncorrectlyCompleted();
            }
            if (!DEEndDateDatePicker.SelectedDate.HasValue)
            {
                flag = true;
                DEEndDateDatePicker.MarkAsUncorrectlyCompleted();
            }
            if (DEExportFormatSelector.SelectedValue == null)
            {
                flag = true;
                DEExportFormatSelector.MarkAsUncorrectlyCompleted();
            }
            if (DEExportDataTypeSelector.SelectedValue == null)
            {
                flag = true;
                DEExportDataTypeSelector.MarkAsUncorrectlyCompleted();
            }
            if (DEExportSequencesSeparator.Text.Trim().Length == 0)
            {
                flag = true;
                DEExportSequencesSeparator.MarkAsUncorrectlyCompleted();
            }
            if (DEExportPathTextArea.Text.Trim().Length == 0 || !Directory.Exists(DEExportPathTextArea.Text.Trim()))
            {
                flag = true;
                DEExportPathTextArea.MarkAsUncorrectlyCompleted();
            }
            if (flag)
            {
                return;
            }
            ExportWaitLoader.Visibility = Visibility.Visible;
            ExportWaitLoader.DropCountersToInitialState();
            DateTime ExportStartDate = DEStartDateDatePicker.SelectedDate.Value;
            DateTime ExportEndDate = DEEndDateDatePicker.SelectedDate.Value;
            DataExportingFormat ExportingFormat = EnumToItemsSource.GetValueFromDescription<DataExportingFormat>(DEExportFormatSelector.SelectedValue.ToString());
            DataExportingType ExportingType = EnumToItemsSource.GetValueFromDescription<DataExportingType>(DEExportDataTypeSelector.SelectedValue.ToString());
            string DataSeparator = DEExportSequencesSeparator.Text;
            string ExportPath = DEExportPathTextArea.Text;
            Task.Run(delegate
            {
                try
                {
                    Action<DataExportingProcessStat> dataExportingProcessCallback = delegate (DataExportingProcessStat DataExportingProcessCallback)
                    {
                        ExportWaitLoader.RefreshCounters(DataExportingProcessCallback);
                    };
                    DEProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.ExportAllGrabbedData(ExportPath, ExportingFormat, ExportingType, DataSeparator, ExportStartDate, ExportEndDate, dataExportingProcessCallback);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in data exporting", ex);
                    App.TrySendAppCrashReport(ex, "Error in data exporting");
                    System.Windows.MessageBox.Show("Error during data exporting. See logs.", "Data export", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                finally
                {
                    System.Windows.MessageBox.Show("Data export completed", "Data export", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    base.Dispatcher.Invoke(delegate
                    {
                        ExportWaitLoader.Visibility = Visibility.Hidden;
                        Close();
                    });
                }
            });
        }

        private void SelectExportPathDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                if (Directory.Exists(selectedPath))
                {
                    DEExportPathTextArea.Text = selectedPath;
                }
            }
        }
    }
}
