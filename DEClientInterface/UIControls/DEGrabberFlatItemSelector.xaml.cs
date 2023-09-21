using DEClientInterface.Logic;
using DEClientInterface.UIExtensions;
using DEClientInterface.UIWindows;
using ExcavatorSharp.Objects;
using FontAwesome5.WPF;
using Newtonsoft.Json;
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
    /// Interaction logic for DEGrabberFlatItemSelector.xaml
    /// </summary>
    public partial class DEGrabberFlatItemSelector : UserControl
    {
        internal List<ParsingBinaryAttributePattern> ParsingAttributesList = new List<ParsingBinaryAttributePattern>();

        private DEProjectEntityJSONProperties EditParsingAttributesModal = null;

        private bool IsNodeDataInitialized = false;

        public bool IsSelectorUpdatedFromBrowser = false;

        private string SelectorOldNodeValueText = string.Empty;

        private DEProjectCubeProperties ParentWindowLink { get; set; }

        private bool IsNodeExpanded { get; set; }

        public string SelectorGUID { get; set; }

        public DEGrabberFlatItemSelector()
        {
            InitializeComponent();
        }

        public DEGrabberFlatItemSelector(DEProjectCubeProperties parentWindow)
        {
            InitializeComponent();
            ParentWindowLink = parentWindow;
            ScrapingNodeSelectorType.SelectedItem = EnumToItemsSource.GetDescriptionFromValue(DataGrabbingSelectorType.CSS_Selector);
            SelectorGUID = GetHashCode().ToString();
            base.Loaded += DEGrabberFlatItemSelector_Loaded;
            IsNodeExpanded = false;
        }

        public DEGrabberFlatItemSelector(DEProjectCubeProperties parentWindow, DataGrabbingPatternItem patternItem)
        {
            InitializeComponent();
            ScrapingNodeNameTextArea.Text = patternItem.ElementName;
            ScrapingNodeSelectorType.SelectedItem = EnumToItemsSource.GetDescriptionFromValue(patternItem.DataSelector.SelectorType);
            ScrapingNodeSelectorData.Text = patternItem.DataSelector.Selector;
            ParsingAttributesList = ((patternItem.ParsingBinaryAttributes != null) ? patternItem.ParsingBinaryAttributes.ToList() : new List<ParsingBinaryAttributePattern>());
            SelectorGUID = patternItem.GetHashCode().ToString();
            SpecialSettings_NodesToScrape.Text = ParsingAttributesList.Count.ToString();
            if (ParsingAttributesList.Count > 0)
            {
                SpecialSettings_NodesToScrape.FontWeight = FontWeights.Bold;
            }
            ParentWindowLink = parentWindow;
            base.Loaded += DEGrabberFlatItemSelector_Loaded;
            IsNodeExpanded = false;
        }

        private void DEGrabberFlatItemSelector_Loaded(object sender, RoutedEventArgs e)
        {
            IsNodeDataInitialized = true;
            if (ParentWindowLink != null && ParentWindowLink.PageNodesPicker != null)
            {
                ExpandElement.Visibility = Visibility.Visible;
            }
            CheckSelectorAndLightNodeIfSelectorIsWrong();
        }

        public DataGrabbingPatternItem TryGetPatternItemFromUIData()
        {
            ScrapingNodeNameTextArea.MarkAsCorrectlyCompleted();
            ScrapingNodeSelectorType.MarkAsCorrectlyCompleted();
            ScrapingNodeSelectorData.MarkAsCorrectlyCompleted();
            string text = ScrapingNodeNameTextArea.Text.Trim();
            DataGrabbingSelectorType dataGrabbingSelectorType = ((ScrapingNodeSelectorType.SelectedItem != null) ? EnumToItemsSource.GetValueFromDescription<DataGrabbingSelectorType>(ScrapingNodeSelectorType.SelectedItem.ToString()) : DataGrabbingSelectorType.CSS_Selector);
            string text2 = ScrapingNodeSelectorData.Text.Trim();
            bool flag = false;
            if (text == string.Empty)
            {
                ScrapingNodeNameTextArea.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (ScrapingNodeSelectorType.SelectedItem == null)
            {
                ScrapingNodeSelectorType.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            if (text2 == string.Empty)
            {
                ScrapingNodeSelectorData.MarkAsUncorrectlyCompleted();
                flag = true;
            }
            // if (dataGrabbingSelectorType == DataGrabbingSelectorType.CSS_Selector && text2.IndexOf(":") != -1 && CSSHelpers.CheckIsCSSPseudoSelectorUsed(text2))
            // {
            //     MessageBox.Show($"CSS pseudo-selectors are not supported. Selector value = {text2}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            //     ScrapingNodeSelectorData.MarkAsUncorrectlyCompleted();
            //     flag = true;
            // }
            /*else if (ParentWindowLink != null && !ParentWindowLink.CSSSelectorsTester.TestSelector(text2, dataGrabbingSelectorType))
			{
				MessageBox.Show($"Wrong or not-supported node selector. Selector value = {text2}", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				ScrapingNodeSelectorData.MarkAsUncorrectlyCompleted();
				flag = true;
			}*/
            if (flag)
            {
                return null;
            }
            if (ParentWindowLink != null)
            {
                bool flag2 = false;
                for (int i = 0; i < ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Count; i++)
                {
                    if (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] as DEGrabberFlatItemSelector != this && (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] as DEGrabberFlatItemSelector).ScrapingNodeNameTextArea.Text.Trim() == text)
                    {
                        MessageBox.Show($"It is not possible to save the node to scrape with the specified name. Node with the name «{text}» already exists.", "Error in nodes to scrape list", MessageBoxButton.OK, MessageBoxImage.Hand);
                        ScrapingNodeNameTextArea.MarkAsUncorrectlyCompleted();
                        (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] as DEGrabberFlatItemSelector).ScrapingNodeNameTextArea.MarkAsUncorrectlyCompleted();
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    return null;
                }
            }
            bool parseBinaryAttributes = ParsingAttributesList.Count > 0;
            return new DataGrabbingPatternItem(text, new GrabberSelector(text2, dataGrabbingSelectorType), parseBinaryAttributes, ParsingAttributesList.ToArray());
        }

        private void TryOpenAttributesToScrapeSettingsModal()
        {
            if (EditParsingAttributesModal != null)
            {
                EditParsingAttributesModal.Focus();
                return;
            }
            List<ParsingBinaryAttributePattern> value = new List<ParsingBinaryAttributePattern>
            {
                new ParsingBinaryAttributePattern("src", IsAttributeAreLinkToSomeResouce: true, IsWeMustDownloadContentUnderAttributeLink: true)
            };
            string arg = JsonConvert.SerializeObject(value, Formatting.Indented);
            string exampleTextAndComment = string.Format("{0}\r\n{1}", "/** Define a set of parsing attributes to download binary data, like items images or other files, linked from HTML **/", arg);
            string actualPropertiesSet = ((ParsingAttributesList != null && ParsingAttributesList.Count > 0) ? JsonConvert.SerializeObject(ParsingAttributesList, Formatting.Indented) : string.Empty);
            EditParsingAttributesModal = new DEProjectEntityJSONProperties("Edit parsing attributes", exampleTextAndComment, actualPropertiesSet, this, delegate (string DefinedArgumentsList, ContentControl ParentWindowLink)
            {
                DEGrabberFlatItemSelector dEGrabberFlatItemSelector = ParentWindowLink as DEGrabberFlatItemSelector;
                DefinedArgumentsList = DefinedArgumentsList.Trim();
                if (DefinedArgumentsList == string.Empty)
                {
                    dEGrabberFlatItemSelector.ParsingAttributesList = new List<ParsingBinaryAttributePattern>();
                }
                else
                {
                    dEGrabberFlatItemSelector.ParsingAttributesList = JsonConvert.DeserializeObject<List<ParsingBinaryAttributePattern>>(DefinedArgumentsList);
                }
                dEGrabberFlatItemSelector.SpecialSettings_NodesToScrape.Text = dEGrabberFlatItemSelector.ParsingAttributesList.Count.ToString();
                if (dEGrabberFlatItemSelector.ParsingAttributesList.Count > 0)
                {
                    dEGrabberFlatItemSelector.SpecialSettings_NodesToScrape.FontWeight = FontWeights.Bold;
                }
                else
                {
                    dEGrabberFlatItemSelector.SpecialSettings_NodesToScrape.FontWeight = FontWeights.Normal;
                }
                dEGrabberFlatItemSelector.EditParsingAttributesModal.Close();
            }, delegate (string DataToValidate)
            {
                DataToValidate = DataToValidate.Trim();
                if (!(DataToValidate == string.Empty))
                {
                    try
                    {
                        List<ParsingBinaryAttributePattern> list = JsonConvert.DeserializeObject<List<ParsingBinaryAttributePattern>>(DataToValidate);
                        return true;
                    }
                    catch (Exception thrownException)
                    {
                        Logger.LogError("Error during unserializing element parsing attributes", thrownException);
                        return false;
                    }
                }
                return true;
            });
            EditParsingAttributesModal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            EditParsingAttributesModal.Closed += EditParsingAttributesModal_Closed;
            if (ParentWindowLink != null)
            {
                ParentWindowLink.ShowShadowOverlay(EditParsingAttributesModal);
            }
            EditParsingAttributesModal.Show();
        }

        private void EditParsingAttributesModal_Closed(object sender, EventArgs e)
        {
            EditParsingAttributesModal = null;
            if (ParentWindowLink != null)
            {
                ParentWindowLink.HideShadowOverlay();
            }
        }

        private void RemoveSelectorNode_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindowLink != null)
            {
                ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Remove(this);
                ParentWindowLink.UpdateDataToScrapeNodesCount();
                ParentWindowLink.HandleCSSSelectorUpdatedFromUINodesList();
            }
        }

        private void MoveElementUp_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindowLink != null)
            {
                int num = ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.IndexOf(this);
                num--;
                if (num < 0)
                {
                    num = 0;
                }
                ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Remove(this);
                ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Insert(num, this);
            }
        }

        private void MoveElementDown_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindowLink != null)
            {
                int num = ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.IndexOf(this);
                num++;
                if (num >= ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Count)
                {
                    num = ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Count - 1;
                }
                ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Remove(this);
                ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Insert(num, this);
            }
        }

        private void EditSpecialSettings_Click(object sender, RoutedEventArgs e)
        {
            TryOpenAttributesToScrapeSettingsModal();
        }

        private void ScrapingNodeNameTextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsNodeDataInitialized)
            {
                if (IsSelectorUpdatedFromBrowser)
                {
                    IsSelectorUpdatedFromBrowser = false;
                }
                else if (ParentWindowLink != null)
                {
                    ParentWindowLink.HandleCSSSelectorNameUpdatedFromUINodesList(SelectorGUID, ScrapingNodeNameTextArea.Text);
                }
            }
        }

        private void ScrapingNodeSelectorData_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ScrapingNodeSelectorData.Text.Trim();
            if (text == SelectorOldNodeValueText)
            {
                return;
            }
            SelectorOldNodeValueText = text;
            if (IsNodeDataInitialized)
            {
                if (IsSelectorUpdatedFromBrowser)
                {
                    IsSelectorUpdatedFromBrowser = false;
                }
                else
                {
                    HandleNodeDataUpdated();
                }
            }
        }

        private void HandleNodeDataUpdated()
        {
            string selectorGUID = SelectorGUID;
            string text = ScrapingNodeNameTextArea.Text.Trim();
            string text2 = ScrapingNodeSelectorData.Text.Trim();
            if (text != string.Empty && text2 != string.Empty && !CSSHelpers.CheckIsCSSPseudoSelectorUsed(text2) && ParentWindowLink != null)
            {
                ParentWindowLink.HandleCSSSelectorUpdatedFromUINodesList();
            }
        }

        private void ExpandElement_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindowLink == null)
            {
                return;
            }
            for (int i = 0; i < ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Count; i++)
            {
                if (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] != this)
                {
                    if ((ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] as DEGrabberFlatItemSelector).IsNodeExpanded)
                    {
                        ParentWindowLink.PageNodesPicker.HandleCollapseAllNodesIntoUI();
                    }
                    ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i].Opacity = 1.0;
                    (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] as DEGrabberFlatItemSelector).ExpandCollapseElementSVGAwesomeIcon.Foreground = UICommonExtensions.BrushFromHex("#2b71bc");
                    (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[i] as DEGrabberFlatItemSelector).IsNodeExpanded = false;
                }
            }
            if (IsNodeExpanded)
            {
                for (int j = 0; j < ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Count; j++)
                {
                    ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[j].Opacity = 1.0;
                }
                if (ParentWindowLink.PageNodesPicker != null)
                {
                    ParentWindowLink.PageNodesPicker.HandleCollapseAllNodesIntoUI();
                }
                ExpandCollapseElementSVGAwesomeIcon.Foreground = UICommonExtensions.BrushFromHex("#2b71bc");
                IsNodeExpanded = false;
                return;
            }
            IsNodeExpanded = true;
            ExpandCollapseElementSVGAwesomeIcon.Foreground = Brushes.Green;
            base.Opacity = 1.0;
            for (int k = 0; k < ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children.Count; k++)
            {
                if (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[k] != this)
                {
                    (ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[k] as DEGrabberFlatItemSelector).IsNodeExpanded = false;
                    ParentWindowLink.ParsingElementsScrollViewerContentPanel.Children[k].Opacity = 0.5;
                }
            }
            if (ParentWindowLink.PageNodesPicker != null)
            {
                ParentWindowLink.Dispatcher.Invoke(delegate
                {
                    ParentWindowLink.PageNodesPicker.HandleNodeExpandedIntoUI(SelectorGUID);
                });
            }
        }

        private void ScrapingNodeSelectorData_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSelectorAndLightNodeIfSelectorIsWrong();
        }

        public bool CheckSelectorAndLightNodeIfSelectorIsWrong()
        {
            if (ParentWindowLink == null)
            {
                return true;
            }
            string text = ScrapingNodeSelectorData.Text.Trim();
            if (text.Length > 0)
            {
                /*if (!ParentWindowLink.CSSSelectorsTester.TestSelector(text, DataGrabbingSelectorType.CSS_Selector))
				{
					CSSSelectorIsWrong.Visibility = Visibility.Visible;
					ScrapingNodeSelectorData.MarkAsUncorrectlyCompleted();
					return false;
				}*/
                CSSSelectorIsWrong.Visibility = Visibility.Collapsed;
                ScrapingNodeSelectorData.MarkAsCorrectlyCompleted();
                return true;
            }
            return false;
        }
    }
}
