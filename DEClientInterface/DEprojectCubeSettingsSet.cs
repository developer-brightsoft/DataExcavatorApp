// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.DEprojectCubeSettingsSet
using System.Collections.Generic;
using ExcavatorSharp.Objects;

namespace DEClientInterface
{
	public class DEprojectCubeSettingsSet
	{
		public string TaskName { get; private set; }

		public string WebsiteRootUrl { get; private set; }

		public string WebsitePageToScrapeExampleUrl { get; private set; }

		public string TaskDescription { get; private set; }

		public List<DataGrabbingPattern> GrabbingPatternsList { get; private set; }

		public CrawlingServerProperties CrawlerProperties { get; private set; }

		public GrabbingServerProperties GrabberProperties { get; private set; }

		public string TaskOperatingDirectory { get; private set; }

		public DEprojectCubeSettingsSet(string TaskName, string WebsiteRootUrl, string WebsitePageToScrapeExampleUrl, string TaskDescription, List<DataGrabbingPattern> GrabbingPatternsList, CrawlingServerProperties CrawlerProperties, GrabbingServerProperties GrabberProperties, string TaskOperatingDirectory)
		{
			this.TaskName = TaskName;
			this.WebsiteRootUrl = WebsiteRootUrl;
			this.WebsitePageToScrapeExampleUrl = WebsitePageToScrapeExampleUrl;
			this.TaskDescription = TaskDescription;
			this.GrabbingPatternsList = GrabbingPatternsList;
			this.CrawlerProperties = CrawlerProperties;
			this.GrabberProperties = GrabberProperties;
			this.TaskOperatingDirectory = TaskOperatingDirectory;
		}
	}
}