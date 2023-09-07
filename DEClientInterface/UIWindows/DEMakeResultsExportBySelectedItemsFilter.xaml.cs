using DEClientInterface.Controls;
using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Exporter;
using ExcavatorSharp.Grabber;
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
    /// Interaction logic for DEMakeResultsExportBySelectedItemsFilter.xaml
    /// </summary>
    public partial class DEMakeResultsExportBySelectedItemsFilter : Window
    {

        private DEProjectCube DEProjectCubeLink { get; set; }

        private List<GrabbedPageMetaInformationDataEntry> SelectedEntriesToExport { get; set; }

        public DEMakeResultsExportBySelectedItemsFilter(DEProjectCube DEProjectCubeLink, List<GrabbedPageMetaInformationDataEntry> SelectedEntriesToExport)
        {
            InitializeComponent();
            this.SelectedEntriesToExport = SelectedEntriesToExport;
            this.DEProjectCubeLink = DEProjectCubeLink;
            DEExportingResultsCountTextArea.Text = $"{this.SelectedEntriesToExport.Count} results to export";
            base.Title = base.Title.Replace("PROJECTNAME", this.DEProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
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

        private void MakeDataExportButton_Click(object sender, RoutedEventArgs e)
        {
            DEExportFormatSelector.MarkAsCorrectlyCompleted();
            DEExportDataTypeSelector.MarkAsCorrectlyCompleted();
            DEExportSequencesSeparator.MarkAsCorrectlyCompleted();
            DEExportPathTextArea.MarkAsCorrectlyCompleted();
            bool flag = false;
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
            DataExportingFormat ExportingFormat = EnumToItemsSource.GetValueFromDescription<DataExportingFormat>(DEExportFormatSelector.SelectedValue.ToString());
            DataExportingType ExportingType = EnumToItemsSource.GetValueFromDescription<DataExportingType>(DEExportDataTypeSelector.SelectedValue.ToString());
            string DataSeparator = DEExportSequencesSeparator.Text;
            string ExportPath = DEExportPathTextArea.Text;
            bool IsErrorOccured = false;
            Task.Run(delegate
            {
                try
                {
                    Action<DataExportingProcessStat> dataExportingProcessCallback = delegate (DataExportingProcessStat ProcessingResult)
                    {
                        ExportWaitLoader.RefreshCounters(ProcessingResult);
                    };
                    DEProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.ExportSelectedGrabbedData(ExportPath, ExportingFormat, ExportingType, DataSeparator, SelectedEntriesToExport, dataExportingProcessCallback);
                }
                catch (Exception ex)
                {
                    IsErrorOccured = true;
                    Logger.LogError("Error in data exporting", ex);
                    App.TrySendAppCrashReport(ex, "Error in data exporting");
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "Data export error", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                /*catch (Exception ex2)
				{
					IsErrorOccured = true;
					Logger.LogError("Error in data exporting", ex2);
					App.TrySendAppCrashReport(ex2, "Error in data exporting");
					System.Windows.MessageBox.Show("Error during data exporting. See logs.", "Data export error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}*/
                finally
                {
                    if (!IsErrorOccured)
                    {
                        System.Windows.MessageBox.Show("Data export completed", "Data export", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    base.Dispatcher.Invoke(delegate
                    {
                        ExportWaitLoader.Visibility = Visibility.Hidden;
                        Close();
                    });
                }
            });
        }

        private void ChangeSelectedResultsButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
