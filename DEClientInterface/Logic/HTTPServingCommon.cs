// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Logic.HTTPServingCommon
using System.Diagnostics;

namespace DEClientInterface.Logic
{
	public class HTTPServingCommon
	{
		public static void OpenURLInBrowser(string Url)
		{
			Process.Start(Url);
		}
	}
}