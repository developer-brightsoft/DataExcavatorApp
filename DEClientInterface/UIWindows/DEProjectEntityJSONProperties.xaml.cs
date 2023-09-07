using DEClientInterface.UIExtensions;
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
    /// Interaction logic for DEProjectEntityJSONProperties.xaml
    /// </summary>
    public partial class DEProjectEntityJSONProperties : Window
    {
        private Action<string, ContentControl> CallbackAction { get; set; }

        private Func<string, bool> ValidationAction { get; set; }

        private ContentControl ParentInvokingWindowLink { get; set; }

        public DEProjectEntityJSONProperties(string ModalTitle, string ExampleTextAndComment, string ActualPropertiesSet, ContentControl ParentInvokingWindowLink, Action<string, ContentControl> CallbackAction, Func<string, bool> ValidationAction)
        {
            InitializeComponent();
            ActualPropertiesSet = ActualPropertiesSet.Replace("\r\n", Environment.NewLine);
            base.Title = ModalTitle;
            ExampleTextBox.Text = ExampleTextAndComment;
            PropertiesTextBox.Document.Blocks.Clear();
            PropertiesTextBox.Document.Blocks.Add(new Paragraph(new Run(ActualPropertiesSet)));
            this.ValidationAction = ValidationAction;
            this.CallbackAction = CallbackAction;
            this.ParentInvokingWindowLink = ParentInvokingWindowLink;
            PropertiesTextBox.Focus();
        }

        private void ApplyJSONSettingsSet_Click(object sender, RoutedEventArgs e)
        {
            string text = new TextRange(PropertiesTextBox.Document.ContentStart, PropertiesTextBox.Document.ContentEnd).Text.Trim();
            PropertiesTextBox.MarkAsCorrectlyCompleted();
            if (ValidationAction(text))
            {
                CallbackAction(text, ParentInvokingWindowLink);
            }
            else
            {
                PropertiesTextBox.MarkAsUncorrectlyCompleted();
            }
        }

        private void CopyJSONSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            string text = ExampleTextBox.Text;
            PropertiesTextBox.Document = new FlowDocument(new Paragraph(new Run(text)));
        }

    }
}
