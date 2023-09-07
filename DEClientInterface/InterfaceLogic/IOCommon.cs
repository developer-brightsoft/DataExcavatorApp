// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.InterfaceLogic.IOCommon
using System;
using System.IO;
using System.Threading;
using DEClientInterface.Logic;
using ExcavatorSharp.Common;

namespace DEClientInterface.InterfaceLogic
{
	public class IOCommon
	{
		internal const string ImageFileExtensions = ".jpg|.jpeg|.png|.bmp|.gif|.tiff|.webp";

		public static string GetExecutingFileLocation()
		{
			return Directory.GetCurrentDirectory();
		}

		public static string GetDataExcavatorCoreIOPath()
		{
			return ExcavatorSharp.Common.IOCommon.GetDataExcavatorCommonIOPath();
		}

		public static string GetDataExcavatorUIFolderPath()
		{
			string text = string.Format("{0}/{1}", GetDataExcavatorCoreIOPath(), "DECIData");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		public static string GetDataExcavatorUIIndexFilePath()
		{
			return string.Format("{0}/{1}", GetDataExcavatorUIFolderPath(), "data-excavator-projects.json");
		}

		public static string GetDataExcavatorCommonPropertiesFilePath()
		{
			return string.Format("{0}/{1}", GetDataExcavatorUIFolderPath(), "core-properties.json");
		}

		public static string GetDataExcavatorCommonProjectsFolderPath()
		{
			string text = string.Format("{0}/{1}", GetDataExcavatorUIFolderPath(), "projects");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		public static string GetDataExcavatorTempDataFolderPath()
		{
			string text = string.Format("{0}/{1}", GetDataExcavatorUIFolderPath(), "temp-data");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		public static string GetFileContentAsStringThreadSafe(string FilePath, int Attempts = 3)
		{
			bool flag = false;
			if (!File.Exists(FilePath))
			{
				return string.Empty;
			}
			int num = 0;
			while (!flag && num < Attempts)
			{
				try
				{
					return File.ReadAllText(FilePath);
				}
				catch (Exception thrownException)
				{
					Logger.LogError("Error during file reading in thread safe manere", thrownException);
					Thread.Sleep(50);
				}
				finally
				{
					num++;
				}
			}
			return string.Empty;
		}

		public static bool IsImageFormat(string FileExtension)
		{
			return ".jpg|.jpeg|.png|.bmp|.gif|.tiff|.webp".IndexOf(FileExtension.ToLower()) != -1;
		}

		public static void ClearOldTempFilesInFilesTempDirectory()
		{
			string dataExcavatorTempDataFolderPath = GetDataExcavatorTempDataFolderPath();
			string[] files = Directory.GetFiles(dataExcavatorTempDataFolderPath, "*.*");
			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					File.Delete(files[i]);
				}
				catch (Exception thrownException)
				{
					Logger.LogError($"Cannot delete temp file {files[i]}", thrownException);
				}
			}
		}
	}
}
