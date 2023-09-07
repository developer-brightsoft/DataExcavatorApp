using DEClientInterface.Controls;
using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using ExcavatorSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for DELinksBufferOverview.xaml
    /// </summary>
    public partial class DELinksBufferOverview : Window
    {
        private ConcurrentQueue<PageLink> LinksToCrawlActualBuffer = new ConcurrentQueue<PageLink>();

        private ConcurrentBag<PageLink> CrawledLinksBuffer = new ConcurrentBag<PageLink>();

        
        private DEProjectCube ProjectCubeLink { get; set; }

        public DELinksBufferOverview(DEProjectCube ProjectCubeLink)
        {
            this.ProjectCubeLink = ProjectCubeLink;
            InitializeComponent();
            base.Title = base.Title.Replace("PROJECTNAME", this.ProjectCubeLink.DataExcavatorUIProjectLink.ProjectName);
            RefreshCrawledLinksAndLinksToCrawl();
        }

        private void RefreshCrawledLinksBuffer()
        {
            WaitLoader.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    CrawlingServerLinksBuffer LinksBufferCopy = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetCrawlingServerLinksBufferCopy();
                    base.Dispatcher.Invoke(delegate
                    {
                        CrawledLinksBuffer = LinksBufferCopy.CrawledLinks;
                        CrawledLinksDataGrid.ItemsSource = CrawledLinksBuffer;
                        CrawledLinksDataGrid.Items.Refresh();
                        ProjectCubeLink.InvalidateCounters();
                        WaitLoader.Visibility = Visibility.Hidden;
                        CrawledLinksTabHeader.Text = $"Crawled links ({CrawledLinksBuffer.Count})";
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in refreshing crawled links", ex);
                    App.TrySendAppCrashReport(ex, "Error in crawled links refreshing");
                }
            });
        }

        private void RefreshLinksToCrawlBuffer()
        {
            WaitLoader.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    CrawlingServerLinksBuffer LinksBufferCopy = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetCrawlingServerLinksBufferCopy();
                    base.Dispatcher.Invoke(delegate
                    {
                        LinksToCrawlActualBuffer = LinksBufferCopy.LinksToCrawl;
                        LinksToCrawlDataGridQueue.ItemsSource = LinksToCrawlActualBuffer;
                        LinksToCrawlDataGridQueue.Items.Refresh();
                        ProjectCubeLink.InvalidateCounters();
                        WaitLoader.Visibility = Visibility.Hidden;
                        LinksToCrawlTabHeader.Text = $"Links to crawl queue ({LinksToCrawlActualBuffer.Count})";
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in refreshing links to crawl", ex);
                    App.TrySendAppCrashReport(ex, "Error in refreshing links to crawl");
                }
            });
        }

        private void RefreshCrawledLinksAndLinksToCrawl()
        {
            WaitLoader.Visibility = Visibility.Visible;
            Task.Run(delegate
            {
                try
                {
                    CrawlingServerLinksBuffer LinksBufferCopy = ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.GetCrawlingServerLinksBufferCopy();
                    base.Dispatcher.Invoke(delegate
                    {
                        LinksToCrawlActualBuffer = LinksBufferCopy.LinksToCrawl;
                        CrawledLinksBuffer = LinksBufferCopy.CrawledLinks;
                        CrawledLinksDataGrid.ItemsSource = CrawledLinksBuffer;
                        CrawledLinksDataGrid.Items.Refresh();
                        LinksToCrawlDataGridQueue.ItemsSource = LinksToCrawlActualBuffer;
                        LinksToCrawlDataGridQueue.Items.Refresh();
                        ProjectCubeLink.InvalidateCounters();
                        WaitLoader.Visibility = Visibility.Hidden;
                        CrawledLinksTabHeader.Text = $"Crawled links ({CrawledLinksBuffer.Count})";
                        LinksToCrawlTabHeader.Text = $"Links to crawl queue ({LinksToCrawlActualBuffer.Count})";
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in refreshing crawled links and links to crawl", ex);
                    App.TrySendAppCrashReport(ex, "Error in refreshing crawled links and links to crawl");
                }
            });
        }

        private void RefreshCrawledLinksListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RefreshCrawledLinksBuffer();
        }

        private void RefreshLinksToCrawlQueue_Click(object sender, RoutedEventArgs e)
        {
            RefreshLinksToCrawlBuffer();
        }

        private void ReindexLinksFromCrawledBufferMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RecrawlSelectedLinks();
        }

        private void RecrawlSelectedLinks()
        {
            for (int i = 0; i < CrawledLinksDataGrid.SelectedItems.Count; i++)
            {
                string normalizedOriginalLink = (CrawledLinksDataGrid.SelectedItems[i] as PageLink).NormalizedOriginalLink;
                ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.ForceLinkRecrawling(normalizedOriginalLink);
            }
            if (CrawledLinksDataGrid.SelectedItems.Count > 0)
            {
                ProjectCubeLink.MarkProjectCubeAsCanBeProcessed();
            }
            RefreshCrawledLinksAndLinksToCrawl();
        }

        private void DeleteLinksFromCrawledBufferMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedCrawledLinks();
        }

        private void DeleteSelectedCrawledLinks()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < CrawledLinksDataGrid.SelectedItems.Count; i++)
            {
                string normalizedOriginalLink = (CrawledLinksDataGrid.SelectedItems[i] as PageLink).NormalizedOriginalLink;
                list.Add(normalizedOriginalLink);
            }
            ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.DeleteLinksFromCrawling(list);
            RefreshCrawledLinksAndLinksToCrawl();
        }

        private void DeleteLinksFromLinksToCrawlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedLinksToCrawl();
        }

        private void DeleteSelectedLinksToCrawl()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < LinksToCrawlDataGridQueue.SelectedItems.Count; i++)
            {
                string normalizedOriginalLink = (LinksToCrawlDataGridQueue.SelectedItems[i] as PageLink).NormalizedOriginalLink;
                list.Add(normalizedOriginalLink);
            }
            ProjectCubeLink.DataExcavatorUIProjectLink.TaskLink.DeleteLinksFromCrawling(list);
            RefreshCrawledLinksAndLinksToCrawl();
        }

        private void LinksToCrawl_SelectLinksByKeyword_Button_Click(object sender, RoutedEventArgs e)
        {
            SelectLinksToCrawlByKeyword();
        }

        private void LinksToCrawl_SelectLinksByKeyword_TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SelectLinksToCrawlByKeyword();
            }
        }

        private void SelectLinksToCrawlByKeyword()
        {
            LinksToCrawlDataGridQueue.SelectedItems.Clear();
            string text = LinksToCrawl_SelectLinksByKeyword_TextArea.Text;
            if (text.Length <= 0)
            {
                return;
            }
            foreach (PageLink item in LinksToCrawlActualBuffer)
            {
                if (item.NormalizedOriginalLink.IndexOf(text) != -1)
                {
                    LinksToCrawlDataGridQueue.SelectedItems.Add(item);
                }
            }
        }

        private void CrawledLinks_SelectLinksByKeyword_Button_Click(object sender, RoutedEventArgs e)
        {
            SelectCrawledLinksByKeyword();
        }

        private void CrawledLinks_SelectLinksByKeyword_TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SelectCrawledLinksByKeyword();
            }
        }

        private void SelectCrawledLinksByKeyword()
        {
            CrawledLinksDataGrid.SelectedItems.Clear();
            string text = CrawledLinks_SelectLinksByKeyword_TextArea.Text;
            if (text.Length <= 0)
            {
                return;
            }
            foreach (PageLink item in CrawledLinksBuffer)
            {
                if (item.NormalizedOriginalLink.IndexOf(text) != -1)
                {
                    CrawledLinksDataGrid.SelectedItems.Add(item);
                }
            }
        }

        private void CrawledLinks_CopyLinkAddressFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CrawledLinksDataGrid.SelectedItems.Count > 0)
            {
                PageLink pageLink = CrawledLinksDataGrid.SelectedItems[0] as PageLink;
                Clipboard.SetText(pageLink.NormalizedOriginalLink);
            }
        }

        private void CrawledLinks_DeleteSelectedLink_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedCrawledLinks();
        }

        private void CrawledLinks_RecrawlLinkFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            RecrawlSelectedLinks();
        }

        private void CrawledLinks_OpenLinkFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CrawledLinksDataGrid.SelectedItems.Count > 0)
            {
                PageLink pageLink = CrawledLinksDataGrid.SelectedItems[0] as PageLink;
                try
                {
                    Process.Start(pageLink.NormalizedOriginalLink);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in link observing", ex);
                    App.TrySendAppCrashReport(ex, "Error in link observing");
                }
            }
        }

        private void LinksToCrawl_CopyLinkAddressFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CrawledLinksDataGrid.SelectedItems.Count > 0)
            {
                PageLink pageLink = LinksToCrawlDataGridQueue.SelectedItems[0] as PageLink;
                Clipboard.SetText(pageLink.NormalizedOriginalLink);
            }
        }

        private void LinksToCrawl_DeleteSelectedLink_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedLinksToCrawl();
        }

        private void LinksToCrawl_OpenLinkFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (LinksToCrawlDataGridQueue.SelectedItems.Count > 0)
            {
                PageLink pageLink = LinksToCrawlDataGridQueue.SelectedItems[0] as PageLink;
                try
                {
                    Process.Start(pageLink.NormalizedOriginalLink);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in link observing", ex);
                    App.TrySendAppCrashReport(ex, "Error in link observing");
                }
            }
        }
    }
}
