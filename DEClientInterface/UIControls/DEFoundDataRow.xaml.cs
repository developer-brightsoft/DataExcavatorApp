using ExcavatorSharp.Grabber;
using ExcavatorSharp.Objects;
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
    /// Interaction logic for DEFoundDataRow.xaml
    /// </summary>
    public partial class DEFoundDataRow : UserControl
    {
        public DEFoundDataRow()
        {
            InitializeComponent();
        }

        public void LoadResultsData(GroupedDataItem GroupedItemResult, string BinaryFilesFolderLink, List<DataGrabbingResultItemBinaryAttributeData> BinaryDataList)
        {
            FoundDataSelectorName.Text = GroupedItemResult.DataGrabbingPatternItemElementName;
            if (GroupedItemResult.GrabbedItemsData == null || GroupedItemResult.GrabbedItemsData.Length == 0)
            {
                base.Opacity = 0.6;
                NoDataFound.Visibility = Visibility.Visible;
                return;
            }
            NoDataFound.Visibility = Visibility.Collapsed;
            for (int i = 0; i < GroupedItemResult.GrabbedItemsData.Length; i++)
            {
                DEDataRowInnerItem dEDataRowInnerItem = new DEDataRowInnerItem();
                dEDataRowInnerItem.LoadData(GroupedItemResult.GrabbedItemsData[i], GroupedItemResult.GrabbedItemsData.Length == 1, i, BinaryFilesFolderLink, BinaryDataList);
                FoundDataItemsList.Children.Add(dEDataRowInnerItem);
            }
            if (GroupedItemResult.GrabbedItemsData.Length > 1)
            {
                MultipleValuesFound.Visibility = Visibility.Visible;
                MultipleValuesFoundCounterTextArea.Text = "(" + GroupedItemResult.GrabbedItemsData.Length + ")";
            }
            else
            {
                MultipleValuesFound.Visibility = Visibility.Collapsed;
                MultipleValuesFoundCounterTextArea.Text = string.Empty;
            }
        }

    }
}
