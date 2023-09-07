// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Logic.Logger
using System;
using System.IO;
using System.Text;
using System.Threading;
using DEClientInterface.InterfaceLogic;

namespace DEClientInterface.Logic
{
	public class Logger
	{
		private static string LogFileFullAddress = string.Empty;

		private static Mutex UILogFileMutex = new Mutex();

		public static void InitLogger()
		{
			string text = $"{IOCommon.GetDataExcavatorUIFolderPath()}/uilogs";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			LogFileFullAddress = string.Format("{0}/{1}", text, "uilogs.txt");
		}

		public static void LogInformation(string Message)
		{
			AppendDataToLogFile($"INFO: {Message}");
		}

		public static void LogError(string Message, Exception ThrownException)
		{
			string text = $"ERROR: Application fatal exception thrown:\r\nMessase: {Message};\r\nStack trase: {ThrownException.StackTrace}; Error text: {ThrownException.ToString()}";
			if (ThrownException.InnerException != null)
			{
				text += $"\r\nInner exception: {ThrownException.InnerException.ToString()};\r\nStack trace: {ThrownException.InnerException.StackTrace};";
			}
			AppendDataToLogFile(text);
		}

		private static void AppendDataToLogFile(string Data)
		{
			UILogFileMutex.WaitOne();
			try
			{
				File.AppendAllText(LogFileFullAddress, string.Format("[{0}]: {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Data), Encoding.Default);
			}
			catch (Exception)
			{
			}
			UILogFileMutex.ReleaseMutex();
		}
	}
}