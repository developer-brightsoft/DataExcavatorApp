// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Objects.DEProjectTemplate
using System;
using System.Collections.Generic;
using ExcavatorSharp.Objects;

namespace DEClientInterface.Objects
{
	public class DEProjectTemplate
	{
		public Dictionary<string, string> TemplateParams = new Dictionary<string, string>();

		public int Id { get; set; }

		public string TemplateName { get; set; }

		public string TemplateFileName { get; set; }

		public string TemplateWebsiteURL { get; set; }

		public string WebsitePageToScrapeExampleUrl { get; set; }

		public string TemplateDescription { get; set; }

		public DateTime TemplateUpdateDate { get; set; }

		public int IsDeleted { get; set; }

		public int TemplateCategory { get; set; }

		public DEProjectTemplateCardStyling ProjectTemplateUICardStyling { get; set; }

		public DataExcavatorTaskExportContainer ExcavatorProjectPackedData { get; set; }

		public DEProjectTemplate()
		{
		}

		public DEProjectTemplate(int Id, string TemplateName, string TemplateFileName, string TemplateWebsiteURL, string TemplateDescription, DateTime TemplateUpdateDate, int IsDeleted, DEProjectTemplateCardStyling ProjectTemplateUICardStyling, Dictionary<string, string> TemplateParams, DataExcavatorTaskExportContainer TemplatProjectData)
		{
			this.Id = Id;
			this.TemplateName = TemplateName;
			this.TemplateFileName = TemplateFileName;
			this.TemplateWebsiteURL = TemplateWebsiteURL;
			this.TemplateDescription = TemplateDescription;
			this.TemplateUpdateDate = TemplateUpdateDate;
			this.IsDeleted = IsDeleted;
			this.ProjectTemplateUICardStyling = ProjectTemplateUICardStyling;
			this.TemplateParams = TemplateParams;
			ExcavatorProjectPackedData = TemplatProjectData;
		}
	}
}