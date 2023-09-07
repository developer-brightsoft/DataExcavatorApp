using ExcavatorSharp.Crawler;
using ExcavatorSharp.Grabber;
using ExcavatorSharp.Objects;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DEClientInterface.UIControls
{
    /// <summary>
    /// Interaction logic for DEDataRowInnerItem.xaml
    /// </summary>
    public partial class DEDataRowInnerItem : UserControl
    {
        private bool IsTextResultsNodeExpanded = false;

        private bool IsHTMLResultsNodeExpanded = false;

        private bool IsAttributesNodeExpanded = false;

        public DEDataRowInnerItem()
        {
            InitializeComponent();
        }

        public void LoadData(GroupedDataItemDescendant DataRowDescendant, bool IsSingleItem, int ItemIndex, string BinaryFilesFolderLink, List<DataGrabbingResultItemBinaryAttributeData> BinaryDataList)
        {
            FoundItemNumberLabel.Content = $"Item #{ItemIndex + 1}";
            if (IsSingleItem)
            {
                FoundItemNumberLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                FoundItemNumberLabel.Visibility = Visibility.Visible;
            }
            DEInnerTextResultsTextArea.Document.Blocks.Clear();
            DEAttributesResultsTextArea.Document.Blocks.Clear();
            DEInnerHTMLResultsTextArea.Document.Blocks.Clear();
            DEInnerTextResultsTextArea.Document.Blocks.Add(new Paragraph(new Run(DataRowDescendant.ElementInnerText.Trim())));
            DEInnerHTMLResultsTextArea.Document.Blocks.Add(new Paragraph(new Run(DataRowDescendant.ElementOuterHtml.Trim())));
            if (DataRowDescendant.ElementAttributes != null && DataRowDescendant.ElementAttributes.Length != 0)
            {
                WebsiteInnerLinksAnalyser websiteInnerLinksAnalyser = new WebsiteInnerLinksAnalyser();
                NodeInnerMediaScrollViewer.Visibility = Visibility.Visible;
                LabelNoAssociatedMedia.Visibility = Visibility.Collapsed;
                for (int i = 0; i < DataRowDescendant.ElementAttributes.Length; i++)
                {
                    if (DataRowDescendant.ElementAttributes[i].ContentNotNullable)
                    {
                        if (DataRowDescendant.ElementAttributes[i].IsFileSuccesfullySaved && BinaryFilesFolderLink != string.Empty)
                        {
                            string text = BinaryFilesFolderLink + "/" + DataRowDescendant.ElementAttributes[i].AttributeSavedFileName;
                            if (File.Exists(text))
                            {
                                try
                                {
                                    DEFilePreviewCard element = new DEFilePreviewCard(text);
                                    NodeInnerMediaScrollViewerContentPanel.Children.Add(element);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        if (BinaryDataList != null)
                        {
                            string attributeValue = DataRowDescendant.ElementAttributes[i].AttributeValue;
                            string FileExtension = websiteInnerLinksAnalyser.GetFileExtension(attributeValue);
                            int bi;
                            for (bi = 0; bi < BinaryDataList.Count; bi++)
                            {
                                if (BinaryDataList[bi].AttributeValue == attributeValue)
                                {
                                    base.Dispatcher.BeginInvoke((Action)delegate
                                    {
                                        DEFilePreviewCard element2 = new DEFilePreviewCard(FileExtension, BinaryDataList[bi].ResourceContent);
                                        NodeInnerMediaScrollViewerContentPanel.Children.Add(element2);
                                    });
                                    break;
                                }
                            }
                        }
                    }
                    string text2 = DataRowDescendant.ElementAttributes[i].AttributeName + ": " + DataRowDescendant.ElementAttributes[i].AttributeValue + "\r\n";
                    DEAttributesResultsTextArea.Document.Blocks.Add(new Paragraph(new Run(text2)));
                }
            }
            else
            {
                DEDataMediaTab.Visibility = Visibility.Collapsed;
                DEDataAttributesTab.Visibility = Visibility.Collapsed;
            }
        }

        private void DEExpandCollapseInnerTextArea_Click(object sender, RoutedEventArgs e)
        {
            if (!IsTextResultsNodeExpanded)
            {
                DEInnerTextResultsTextArea.Height = 150.0;
                IsTextResultsNodeExpanded = true;
            }
            else
            {
                DEInnerTextResultsTextArea.Height = 94.0;
                IsTextResultsNodeExpanded = false;
            }
        }

        private void DEExpandCollapseHTMLResultsArea_Click(object sender, RoutedEventArgs e)
        {
            if (!IsHTMLResultsNodeExpanded)
            {
                DEInnerHTMLResultsTextArea.Height = 150.0;
                IsHTMLResultsNodeExpanded = true;
            }
            else
            {
                DEInnerHTMLResultsTextArea.Height = 94.0;
                IsHTMLResultsNodeExpanded = false;
            }
        }

        private void DEExpandCollapseAttributesTextArea_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAttributesNodeExpanded)
            {
                DEAttributesResultsTextArea.Height = 150.0;
                IsHTMLResultsNodeExpanded = true;
            }
            else
            {
                DEAttributesResultsTextArea.Height = 94.0;
                IsHTMLResultsNodeExpanded = false;
            }
        }
    }
}
