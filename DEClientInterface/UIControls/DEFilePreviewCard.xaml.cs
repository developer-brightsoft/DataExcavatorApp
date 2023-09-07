using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DEClientInterface.InterfaceLogic;
using DEClientInterface.Logic;
using DEClientInterface.UIExtensions;
using FontAwesome5;
using FontAwesome5.WPF;

namespace DEClientInterface.UIControls
{
    /// <summary>
    /// Interaction logic for DEFilePreviewCard.xaml
    /// </summary>
    public partial class DEFilePreviewCard : UserControl
    {
        private string FilePath { get; set; }

        private string FileExtension { get; set; }

        private long FileSize { get; set; }

        private byte[] FileData { get; set; }

        public DEFilePreviewCard()
        {
            InitializeComponent();
        }
        public DEFilePreviewCard(string FilePath)
        {
            InitializeComponent();
            this.FilePath = FilePath;
            FileInfo fileInfo = new FileInfo(this.FilePath);
            FileExtension = fileInfo.Extension;
            FileSize = fileInfo.Length;
            FileData = null;
            LoadFilePreview();
        }

        public DEFilePreviewCard(string FileExtension, byte[] FileData)
        {
            InitializeComponent();
            this.FileExtension = FileExtension;
            FileSize = FileData.Length;
            this.FileData = FileData;
            FilePath = string.Empty;
            LoadFilePreview();
        }

        private void LoadFilePreview()
        {
            if (IOCommon.IsImageFormat(FileExtension))
            {
                try
                {
                    Image image = new Image();
                    image.Width = 180.0;
                    image.Height = 180.0;
                    image.Margin = new Thickness(4.0);
                    if (FilePath != string.Empty)
                    {
                        image.Source = new BitmapImage(new Uri(FilePath));
                    }
                    else if (FileData != null)
                    {
                        BitmapImage source = UICommonExtensions.BitmapImageFromBytesArray(FileData);
                        image.Source = source;
                    }
                    ContentGrid.Children.Add(image);
                }
                catch (Exception thrownException)
                {
                    Logger.LogError("Error during overview of binary data", thrownException);
                }
            }
            else
            {
                ImageAwesome imageAwesome = new ImageAwesome();
                imageAwesome.Icon = EFontAwesomeIcon.Regular_File;
                imageAwesome.Width = 150.0;
                imageAwesome.Height = 150.0;
                imageAwesome.VerticalAlignment = VerticalAlignment.Center;
                imageAwesome.HorizontalAlignment = HorizontalAlignment.Center;
                imageAwesome.Foreground = Brushes.DeepSkyBlue;
                switch (FileExtension)
                {
                    case ".pdf":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FilePdf;
                        break;
                    case ".doc":
                    case ".docx":
                    case ".odt":
                    case ".odtx":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileWord;
                        break;
                    case ".exe":
                    case ".msi":
                    case ".apk":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileCode;
                        break;
                    case ".xls":
                    case ".xlsx":
                    case ".ods":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileExcel;
                        break;
                    case ".csv":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileExcel;
                        break;
                    case ".html":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileCode;
                        break;
                    case ".txt":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileAlt;
                        break;
                    case ".ppt":
                    case ".pttx":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FilePowerpoint;
                        break;
                    case ".zip":
                    case ".7z":
                    case ".rar":
                    case ".tar":
                    case ".gzip":
                    case ".xar":
                    case ".gz":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileArchive;
                        break;
                    case ".avi":
                    case ".mkv":
                    case ".mov":
                    case ".flv":
                    case ".vob":
                    case ".wmv":
                    case ".mp4":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileVideo;
                        break;
                    case ".aud":
                    case ".wav":
                    case ".vqf":
                    case ".wma":
                    case ".flac":
                    case ".alac":
                    case ".mp2":
                    case ".mp3":
                    case ".ogg":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileAudio;
                        break;
                    case ".ico":
                        imageAwesome.Icon = EFontAwesomeIcon.Regular_FileImage;
                        break;
                }
                ContentGrid.Children.Add(imageAwesome);
            }
            FileNameTextBlock.Text = $"{FileExtension} file, {FileSize >> 10} Kb";
        }

        private void OpenFileWithDefaultApplication(string FileLocation)
        {
            try
            {
                using Process process = new Process();
                process.StartInfo.FileName = FileLocation;
                process.Start();
            }
            catch (Exception thrownException)
            {
                Logger.LogError($"Cannot open a file, location = {FileLocation}", thrownException);
            }
        }

        private void ContentGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FilePath != string.Empty)
            {
                OpenFileWithDefaultApplication(FilePath);
            }
            else if (FileData != null)
            {
                IOCommon.ClearOldTempFilesInFilesTempDirectory();
                string dataExcavatorTempDataFolderPath = IOCommon.GetDataExcavatorTempDataFolderPath();
                string text = $"{dataExcavatorTempDataFolderPath}/{Guid.NewGuid().ToString()}.{FileExtension}";
                File.WriteAllBytes(text, FileData);
                OpenFileWithDefaultApplication(text);
            }
        }

    }
}
