// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.UIExtensions.UICommonExtensions
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DEClientInterface.Logic;

namespace DEClientInterface.UIExtensions
{
	public static class UICommonExtensions
	{
		public const string UncorrectlyCompletedControlBackground = "#ffc9c9";

		public static void MarkAsUncorrectlyCompleted(this TextBox TextBoxLink)
		{
			TextBoxLink.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc9c9");
		}

		public static void MarkAsCorrectlyCompleted(this TextBox TextBoxLink)
		{
			TextBoxLink.Background = System.Windows.Media.Brushes.White;
		}

		public static void MarkAsUncorrectlyCompleted(this ScrollViewer ScrollViewerLink)
		{
			ScrollViewerLink.Background = System.Windows.Media.Brushes.White;
		}

		public static void MarkAsCorrectlyCompleted(this ScrollViewer ScrollViewerLink)
		{
			ScrollViewerLink.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc9c9");
		}

		public static void MarkAsUncorrectlyCompleted(this RichTextBox RichTextBoxLink)
		{
			RichTextBoxLink.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc9c9");
		}

		public static void MarkAsCorrectlyCompleted(this RichTextBox RichTextBoxLink)
		{
			RichTextBoxLink.Background = System.Windows.Media.Brushes.White;
		}

		public static void MarkAsUncorrectlyCompleted(this ComboBox ComboBoxLink)
		{
			ComboBoxLink.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc9c9");
		}

		public static void MarkAsCorrectlyCompleted(this StackPanel ComboBoxLink)
		{
			ComboBoxLink.Background = System.Windows.Media.Brushes.White;
		}

		public static void MarkAsUncorrectlyCompleted(this StackPanel ComboBoxLink)
		{
			ComboBoxLink.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc9c9");
		}

		public static void MarkAsCorrectlyCompleted(this ComboBox ComboBoxLink)
		{
			ComboBoxLink.Background = System.Windows.Media.Brushes.White;
		}

		public static void MarkAsCorrectlyCompleted(this DatePicker DatePickerLink)
		{
			DatePickerLink.Background = System.Windows.Media.Brushes.White;
		}

		public static void MarkAsUncorrectlyCompleted(this DatePicker DatePickerLink)
		{
			DatePickerLink.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc9c9");
		}

		public static SolidColorBrush BrushFromHex(string Hex)
		{
			return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Hex));
		}

		public static void SelectItemByTag(this ComboBox ComboBoxLink, object ItemTagValue)
		{
			for (int i = 0; i < ComboBoxLink.Items.Count; i++)
			{
				if ((ComboBoxLink.Items[i] as ComboBoxItem).Tag != null && string.Compare((ComboBoxLink.Items[i] as ComboBoxItem).Tag.ToString(), ItemTagValue.ToString(), ignoreCase: true) == 0)
				{
					ComboBoxLink.SelectedItem = ComboBoxLink.Items[i];
					break;
				}
			}
		}

		public static T GetSelectedValueTag<T>(this ComboBox ComboBoxLink)
		{
			T result = default(T);
			if (ComboBoxLink.SelectedItem == null)
			{
				return result;
			}
			try
			{
				result = (T)Convert.ChangeType((ComboBoxLink.SelectedItem as ComboBoxItem).Tag, typeof(T));
			}
			catch (Exception thrownException)
			{
				Logger.LogError("UI Common extensions: tags convertation error", thrownException);
			}
			return result;
		}

		public static BitmapImage BitmapImageFromBytesArray(byte[] array)
		{
			MemoryStream memoryStream = null;
			try
			{
				memoryStream = new MemoryStream(array);
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				return bitmapImage;
			}
			catch (Exception)
			{
				memoryStream?.Dispose();
				return null;
			}
		}

		public static BitmapImage BitmapImageFromBitmapObject(Bitmap bitmap)
		{
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				bitmap.Save(memoryStream, ImageFormat.Bmp);
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				memoryStream.Seek(0L, SeekOrigin.Begin);
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.EndInit();
				return bitmapImage;
			}
			catch (Exception)
			{
				memoryStream.Dispose();
				return null;
			}
		}

		public static string RemoveAllPunctuation(this string TargetString)
		{
			return new string(TargetString.Where((char c) => !char.IsPunctuation(c)).ToArray());
		}
	}
}