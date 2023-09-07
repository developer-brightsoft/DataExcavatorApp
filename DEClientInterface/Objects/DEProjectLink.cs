// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Objects.DEProjectLink
using System.Collections.Generic;
using System.Linq;
using DEClientInterface.Controls;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Interfaces;
using Newtonsoft.Json;

namespace DEClientInterface.Objects
{
	public class DEProjectLink : IJSONConvertible<DEProjectLink>, IJSONListImportable<DEProjectLink>
	{
		public string ProjectName { get; set; }

		public string ProjectPath { get; set; }

		public bool CrawlWebsiteLinksSettedWay { get; set; }

		[JsonIgnore]
		public DataExcavatorTask TaskLink { get; set; }

		[JsonIgnore]
		public DEProjectCube ProjectCubeLink { get; set; }

		public DEProjectLink()
		{
		}

		public DEProjectLink(DataExcavatorTask TaskLink)
		{
			ProjectName = TaskLink.TaskName;
			ProjectPath = TaskLink.TaskOperatingDirectory;
			this.TaskLink = TaskLink;
		}

		public string SerializeToJSON()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public DEProjectLink UnserializeFromJSON(string JSONData)
		{
			return JsonConvert.DeserializeObject<DEProjectLink>(JSONData);
		}

		public List<DEProjectLink> UnserializeListFromJSON(string JSONData)
		{
			return (from item in JsonConvert.DeserializeObject<List<DEProjectLink>>(JSONData)
				where item != null
				select item).ToList();
		}
	}
}