// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Logic.CSSHelpers
namespace DEClientInterface.Logic
{
	public class CSSHelpers
	{
		public static bool CheckIsCSSPseudoSelectorUsed(string Selector)
		{
			string text = Selector.Replace("http:", "_").Replace("https:", "_").Replace("ftp:", "_")
				.Replace("ftps:", "_");
			if (text.IndexOf(":") != -1)
			{
				return true;
			}
			return false;
		}
	}
}
