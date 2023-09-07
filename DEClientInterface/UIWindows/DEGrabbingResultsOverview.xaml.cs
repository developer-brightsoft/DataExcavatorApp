using DEClientInterface.Controls;
using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using ExcavatorSharp.Common;
using ExcavatorSharp.Grabber;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for DEGrabbingResultsOverview.xaml
    /// </summary>
    public partial class DEGrabbingResultsOverview : Window
    {
        public const bool DefaultCheckstateForEntriesInGrid = true;

        private DEMakeResultsExportBySelectedItemsFilter exportResultsBySelectedItemsModal = null;

        public List<GrabbedPageMetaInformationDataEntry_DataGridViewRow> GrabbedDataOverview = new List<GrabbedPageMetaInformationDataEntry_DataGridViewRow>();

        private bool LoadDataIntoOverviewMutexEmulator = false;

        private bool ReloadResultsTableMutexEmulator = false;

        private DEProjectCube ProjectCubeLink { get; set; }

        private UIOverlayController overlayController { get; set; }

        public DEGrabbingResultsOverview(DEProjectCube ParentCubeLink)
        {
            ProjectCubeLink = ParentCubeLink;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", ProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
            overlayController = new UIOverlayController(this, MainWindowShadowOverlay);
            ReloadGrabbingResults();
        }

        private void ReloadGrabbingResults()
        {
            if (ReloadResultsTableMutexEmulator)
            {
                return;
            }
            ReloadResultsTableMutexEmulator = true;
            WaitLoader.Visibility = Visibility.Visible;
            ResultsOverviewGrid.ItemsSource = null;
            ResultsOverviewGrid.Items.Refresh();
            GrabbedDataOverview.Clear();
            ViewDataDetailsOverlay.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    List<GrabbedPageMetaInformationDataEntry> grabbedDataListOverview = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetGrabbedDataListOverview();
                    List<GrabbedPageMetaInformationDataEntry_DataGridViewRow> list = new List<GrabbedPageMetaInformationDataEntry_DataGridViewRow>();
                    int NotEmptyEntriesCount = 0;
                    for (int i = 0; i < grabbedDataListOverview.Count; i++)
                    {
                        GrabbedDataOverview.Add(new GrabbedPageMetaInformationDataEntry_DataGridViewRow(grabbedDataListOverview[i]));
                        if (grabbedDataListOverview[i].HasResults)
                        {
                            NotEmptyEntriesCount++;
                        }
                    }
                    base.Dispatcher.Invoke(delegate
                    {
                        HeadingCheckbox.IsChecked = true;
                        ResultsOverviewGrid.ItemsSource = GrabbedDataOverview;
                        ResultsOverviewGrid.Items.Refresh();
                        WaitLoader.Visibility = Visibility.Hidden;
                        ResultsCounterTextBlock.Text = $"There are {GrabbedDataOverview.Count} total entries and {NotEmptyEntriesCount} not empty entries";
                    });
                    ReloadResultsTableMutexEmulator = false;
                }
                catch (Exception ex)
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoader.Visibility = Visibility.Hidden;
                    });
                    ReloadResultsTableMutexEmulator = false;
                    Logger.LogError("Error during scraping results loading", ex);
                    App.TrySendAppCrashReport(ex, "Error in scraping results loading");
                }
            });
        }

        private void RefreshResultsButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadGrabbingResults();
        }

        private void ResultsOverviewGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IInputElement directlyOver = e.MouseDevice.DirectlyOver;
            if (directlyOver != null && directlyOver is FrameworkElement && ((FrameworkElement)directlyOver).Parent is DataGridCell && sender is DataGrid dataGrid && dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count == 1 && dataGrid.SelectedItem != null)
            {
                ViewDataDetailsOverlay.Visibility = Visibility.Collapsed;
                LoadEntryDataIntoOverview(dataGrid.SelectedItem as GrabbedPageMetaInformationDataEntry);
            }
        }

        private void LoadEntryDataIntoOverview(GrabbedPageMetaInformationDataEntry DataEntry)
        {
            if (LoadDataIntoOverviewMutexEmulator)
            {
                return;
            }
            LoadDataIntoOverviewMutexEmulator = true;
            WaitLoader.Visibility = Visibility.Visible;
            base.Dispatcher.BeginInvoke((Action)delegate
            {
                try
                {
                    string text = $"{ProjectCubeLink.DataExcavatorUIProjectLink.ProjectPath}/{DataEntry.GetParsedDataFileLink()}";
                    string text2 = $"{ProjectCubeLink.DataExcavatorUIProjectLink.ProjectPath}/{DataEntry.GetBinaryFolderFolderLink()}";
                    string text3 = string.Empty;
                    if (File.Exists(text))
                    {
                        text3 = DEClientInterface.InterfaceLogic.IOCommon.GetFileContentAsStringThreadSafe(text);
                    }
                    GrabbedResultsData.Text = text3;
                    GrabbedResultsMediaStackPanel.Children.Clear();
                    DEObjectiveViewResultsStackPanel.Children.Clear();
                    if (Directory.Exists(text2))
                    {
                        string[] files = Directory.GetFiles(text2, "*");
                        string[] array = files;
                        foreach (string NextBinaryFileFullPath in array)
                        {
                            base.Dispatcher.Invoke(delegate
                            {
                                DEFilePreviewCard element = new DEFilePreviewCard(NextBinaryFileFullPath);
                                GrabbedResultsMediaStackPanel.Children.Add(element);
                            });
                        }
                    }
                    GrabbedDataGroup grabbedDataGroup = new GrabbedDataGroup();
                    List<GrabbedDataGroup> list = grabbedDataGroup.UnserializeListFromJSON(text3);
                    GrabbedDataGroup grabbedDataGroup2 = list[0];
                    List<GroupedDataItem> list2 = grabbedDataGroup2.GrabbingResults[0];
                    for (int j = 0; j < list2.Count; j++)
                    {
                        try
                        {
                            DEFoundDataRow dEFoundDataRow = new DEFoundDataRow();
                            dEFoundDataRow.LoadResultsData(list2[j], text2, null);
                            DEObjectiveViewResultsStackPanel.Children.Add(dEFoundDataRow);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    WaitLoader.Visibility = Visibility.Collapsed;
                    LoadDataIntoOverviewMutexEmulator = false;
                }
                catch (Exception ex2)
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        WaitLoader.Visibility = Visibility.Hidden;
                        LoadDataIntoOverviewMutexEmulator = false;
                    });
                    Logger.LogError("Error during data overview", ex2);
                    App.TrySendAppCrashReport(ex2, "Error during data overview");
                }
            });
        }

        private void ReloadGrabbingResultsData_Click(object sender, RoutedEventArgs e)
        {
            ReloadGrabbingResults();
        }

        private void DeleteGrabbingResultRow_Click(object sender, RoutedEventArgs e)
        {
            DeleteGrabbingResultsFromList();
        }

        private void DeleteGrabbingResultsFromList()
        {
            WaitLoaderWithProgressBar.ClearLogs();
            WaitLoaderWithProgressBar.Visibility = Visibility.Visible;
            GrabbedResultsData.Text = string.Empty;
            GrabbedResultsMediaStackPanel.Children.Clear();
            Action<ActionProcessingContainer> clearingProcessCallback = delegate (ActionProcessingContainer counterData)
            {
                WaitLoaderWithProgressBar.AddLogEntry(counterData.OutputMessage, counterData.CompletionPercent);
            };
            Task.Run(delegate
            {
                try
                {
                    List<GrabbedPageMetaInformationDataEntry> list = new List<GrabbedPageMetaInformationDataEntry>();
                    for (int i = 0; i < GrabbedDataOverview.Count; i++)
                    {
                        if (GrabbedDataOverview[i].IsChecked)
                        {
                            list.Add(GrabbedDataOverview[i]);
                        }
                    }
                    if (list.Count == 0)
                    {
                        MessageBox.Show("There are no rows for which the checkbox is set. Please mark the required rows with the checkbox and try again.", "No rows selected", MessageBoxButton.OK, MessageBoxImage.Hand);
                        base.Dispatcher.Invoke(delegate
                        {
                            WaitLoaderWithProgressBar.Visibility = Visibility.Hidden;
                        });
                    }
                    else
                    {
                        ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.DeleteSpecifiedGrabbedDataEntries(list, clearingProcessCallback);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Something went wrong. Please, contact support", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    Logger.LogError("Error in deleting scrape results", ex);
                    App.TrySendAppCrashReport(ex, "Error in deleting scrape results");
                }
                finally
                {
                    base.Dispatcher.Invoke(delegate
                    {
                        ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.ReloadNativeMetrics();
                        ProjectCubeLink.InvalidateCounters();
                        WaitLoaderWithProgressBar.Visibility = Visibility.Hidden;
                        ReloadGrabbingResults();
                    });
                }
            });
        }

        private void ExportResultsByDateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DEMakeResultsExportByDatesFilter dEMakeResultsExportByDatesFilter = new DEMakeResultsExportByDatesFilter(ProjectCubeLink);
            dEMakeResultsExportByDatesFilter.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dEMakeResultsExportByDatesFilter.Closed += MakeResultsExportModal_Closed;
            ProjectCubeLink.ParentWindowLink.PreventShadowOverlayHiding();
            Close();
            ProjectCubeLink.ParentWindowLink.ShowMainWindowShadowOverlay(dEMakeResultsExportByDatesFilter);
            dEMakeResultsExportByDatesFilter.Show();
        }

        private void MakeResultsExportModal_Closed(object sender, EventArgs e)
        {
            ProjectCubeLink.ParentWindowLink.HideMainWindowShadowOverlay();
        }

        private void ExportResultsByCheckedRowsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<GrabbedPageMetaInformationDataEntry> list = new List<GrabbedPageMetaInformationDataEntry>();
            for (int i = 0; i < GrabbedDataOverview.Count; i++)
            {
                if (GrabbedDataOverview[i].IsChecked)
                {
                    list.Add(GrabbedDataOverview[i]);
                }
            }
            if (list.Count == 0)
            {
                MessageBox.Show("There are no lines for which the checkbox is set. Please mark the required lines with the checkbox and try again.", "No rows selected", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (exportResultsBySelectedItemsModal != null)
            {
                exportResultsBySelectedItemsModal.Focus();
                return;
            }
            exportResultsBySelectedItemsModal = new DEMakeResultsExportBySelectedItemsFilter(ProjectCubeLink, list);
            exportResultsBySelectedItemsModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            exportResultsBySelectedItemsModal.Closed += ExportResultsBySelectedItemsModal_Closed;
            ShowMainWindowShadowOverlay(exportResultsBySelectedItemsModal);
            exportResultsBySelectedItemsModal.Show();
        }

        private void ExportResultsBySelectedItemsModal_Closed(object sender, EventArgs e)
        {
            HideMainWindowShadowOverlay();
            exportResultsBySelectedItemsModal = null;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ResultsOverviewGrid.ItemsSource = null;
            ResultsOverviewGrid.Items.Refresh();
            for (int i = 0; i < GrabbedDataOverview.Count; i++)
            {
                GrabbedDataOverview[i].IsChecked = true;
            }
            ResultsOverviewGrid.ItemsSource = GrabbedDataOverview;
            ResultsOverviewGrid.Items.Refresh();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ResultsOverviewGrid.ItemsSource = null;
            ResultsOverviewGrid.Items.Refresh();
            for (int i = 0; i < GrabbedDataOverview.Count; i++)
            {
                GrabbedDataOverview[i].IsChecked = false;
            }
            ResultsOverviewGrid.ItemsSource = GrabbedDataOverview;
            ResultsOverviewGrid.Items.Refresh();
        }

        private void CheckNonEmptyResults_Click(object sender, RoutedEventArgs e)
        {
            ResultsOverviewGrid.ItemsSource = null;
            ResultsOverviewGrid.Items.Refresh();
            for (int i = 0; i < GrabbedDataOverview.Count; i++)
            {
                if (GrabbedDataOverview[i].HasResults)
                {
                    GrabbedDataOverview[i].IsChecked = true;
                }
            }
            ResultsOverviewGrid.ItemsSource = GrabbedDataOverview;
            ResultsOverviewGrid.Items.Refresh();
        }

        private void GrabbedResultsOverviewSelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectResultsByURLKeyword();
        }

        private void GrabbedResultsOVerviewKeywordText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SelectResultsByURLKeyword();
            }
        }

        private void SelectResultsByURLKeyword()
        {
            string text = GrabbedResultsOVerviewKeywordText.Text.Trim();
            if (text.Length <= 0)
            {
                return;
            }
            ResultsOverviewGrid.ItemsSource = null;
            ResultsOverviewGrid.Items.Refresh();
            for (int i = 0; i < GrabbedDataOverview.Count; i++)
            {
                if (GrabbedDataOverview[i].GrabbedPageUrl.IndexOf(text) != -1)
                {
                    GrabbedDataOverview[i].IsChecked = true;
                }
            }
            ResultsOverviewGrid.ItemsSource = GrabbedDataOverview;
            ResultsOverviewGrid.Items.Refresh();
        }

        private void GrabbedResults_CopyLinkAddressFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsOverviewGrid.SelectedItems.Count > 0)
            {
                GrabbedPageMetaInformationDataEntry_DataGridViewRow grabbedPageMetaInformationDataEntry_DataGridViewRow = ResultsOverviewGrid.SelectedItems[0] as GrabbedPageMetaInformationDataEntry_DataGridViewRow;
                Clipboard.SetText(grabbedPageMetaInformationDataEntry_DataGridViewRow.GrabbedPageUrl);
            }
        }

        private void GrabbedResults_DeleteSelectedLink_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsOverviewGrid.SelectedItems.Count != 0)
            {
                for (int i = 0; i < GrabbedDataOverview.Count; i++)
                {
                    GrabbedDataOverview[i].IsChecked = false;
                }
                if (ResultsOverviewGrid.SelectedItems.Count > 0)
                {
                    (ResultsOverviewGrid.SelectedItems[0] as GrabbedPageMetaInformationDataEntry_DataGridViewRow).IsChecked = true;
                }
                DeleteGrabbingResultsFromList();
            }
        }

        private void GrabbedResults_OpenLinkFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsOverviewGrid.SelectedItems.Count > 0)
            {
                GrabbedPageMetaInformationDataEntry_DataGridViewRow grabbedPageMetaInformationDataEntry_DataGridViewRow = ResultsOverviewGrid.SelectedItems[0] as GrabbedPageMetaInformationDataEntry_DataGridViewRow;
                try
                {
                    Process.Start(grabbedPageMetaInformationDataEntry_DataGridViewRow.GrabbedPageUrl);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in link observing", ex);
                    App.TrySendAppCrashReport(ex, "Error in link observing");
                }
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
    }
}
