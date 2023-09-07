using ExcavatorSharp.Exporter;
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
    /// Interaction logic for DEDataExportProcessingLoader.xaml
    /// </summary>
    public partial class DEDataExportProcessingLoader : UserControl
    {
        public double PreviousCompletionPercent = 0.0;

        public DEDataExportProcessingLoader()
        {
            InitializeComponent();
        }

        public void DropCountersToInitialState()
        {
            base.Dispatcher.Invoke(delegate
            {
                DataExporterPercentageTextBox.Text = "0%";
                DataExportProgressBar.Value = 0.0;
                DataExportStatTextBlock.Text = "Calculating results count...";
                PreviousCompletionPercent = 0.0;
            });
        }

        public void RefreshCounters(DataExportingProcessStat ActualExportingStat)
        {
            base.Dispatcher.Invoke(delegate
            {
                if (PreviousCompletionPercent < ActualExportingStat.ExportCompletionPercentage)
                {
                    DataExporterPercentageTextBox.Text = $"{ActualExportingStat.ExportCompletionPercentage}%";
                    DataExportProgressBar.Value = ActualExportingStat.ExportCompletionPercentage;
                    DataExportStatTextBlock.Text = $"{ActualExportingStat.ActualEntryNr} / {ActualExportingStat.TotalEntriesCount}";
                    PreviousCompletionPercent = ActualExportingStat.ExportCompletionPercentage;
                }
            });
        }
    }
}
