// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Logic.DEJSNodePickerEvent_EventData
using System.Collections.Generic;

namespace DEClientInterface.Logic
{
	public class DEJSNodePickerEvent_EventData
	{
		public string event_type { get; set; }

		public Dictionary<string, string> event_data { get; set; }

		public List<DEJSNodePirkcerEvent_AddedCSSSelector> found_selectors { get; set; }
	}
}