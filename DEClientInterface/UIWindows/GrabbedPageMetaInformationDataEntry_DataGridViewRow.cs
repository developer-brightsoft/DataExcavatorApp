// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.UIWindows.GrabbedPageMetaInformationDataEntry_DataGridViewRow
using ExcavatorSharp.Grabber;

namespace DEClientInterface.UIWindows
{
	public class GrabbedPageMetaInformationDataEntry_DataGridViewRow : GrabbedPageMetaInformationDataEntry
	{
		public bool IsChecked { get; set; }

		public GrabbedPageMetaInformationDataEntry_DataGridViewRow(GrabbedPageMetaInformationDataEntry SourceEntry)
		{
			base.BinaryFilesCount = SourceEntry.BinaryFilesCount;
			base.DataSizeKb = SourceEntry.DataSizeKb;
			base.GrabbedPageUrl = SourceEntry.GrabbedPageUrl;
			base.HasResults = SourceEntry.HasResults;
			base.PageGrabbedDateTime = SourceEntry.PageGrabbedDateTime;
			base.ResultsFolderLink = SourceEntry.ResultsFolderLink;
			IsChecked = true;
		}
	}
}