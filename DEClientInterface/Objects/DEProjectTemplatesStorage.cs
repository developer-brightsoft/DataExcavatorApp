// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Objects.DEProjectTemplatesStorage
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DEClientInterface;
using DEClientInterface.InterfaceLogic;
using DEClientInterface.Logic;
using DEClientInterface.UIControls;
using ExcavatorSharp.Common;
using Newtonsoft.Json;

namespace DEClientInterface.Objects
{
	public class DEProjectTemplatesStorage
	{
		public List<DEProjectTemplate> ProjectTemplates = new List<DEProjectTemplate>();

		private MainWindow MainWindowLink { get; set; }

		public DEProjectTemplatesStorage(MainWindow MainWindowLink)
		{
			this.MainWindowLink = MainWindowLink;
		}

		public void Initialize()
		{
			Task.Run(delegate
			{
				ReadTemplatesFromHDD();
				RenderTemplatesIntoUI();
			});
		}

		private void ReadTemplatesFromHDD()
		{
			string path = string.Format(string.Format("{0}/{1}", DEClientInterface.InterfaceLogic.IOCommon.GetDataExcavatorUIFolderPath(), "templates"));
			if (!Directory.Exists(path))
			{
				DirectoryInfo folderFullAccessPermissions = Directory.CreateDirectory(path);
				ExcavatorSharp.Common.IOCommon.SetFolderFullAccessPermissions(folderFullAccessPermissions);
			}
			string executingFileLocation = DEClientInterface.InterfaceLogic.IOCommon.GetExecutingFileLocation();
			string path2 = string.Format("{0}/{1}", executingFileLocation, "templates");
			if (Directory.Exists(path2))
			{
				string[] files = Directory.GetFiles(path2, "*.json");
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						string value = File.ReadAllText(text);
						DEProjectTemplate item2 = JsonConvert.DeserializeObject<DEProjectTemplate>(value);
						ProjectTemplates.Add(item2);
					}
					catch (Exception thrownException)
					{
						Logger.LogError($"Cannot deserialize project template data. Path = {text}", thrownException);
					}
				}
			}
			ProjectTemplates = ProjectTemplates.OrderBy((DEProjectTemplate item) => item.TemplateName).ToList();
		}

		private void RenderTemplatesIntoUI()
		{
			int i;
			for (i = 0; i < ProjectTemplates.Count; i++)
			{
				try
				{
					MainWindowLink.Dispatcher.Invoke(delegate
					{
						DEProjectTemplateCard element = new DEProjectTemplateCard(MainWindowLink, ProjectTemplates[i]);
						MainWindowLink.DEScrapingTemplatesArea.Children.Add(element);
					});
				}
				catch (Exception thrownException)
				{
					Logger.LogError($"Cannot create project UI template card. Path = {ProjectTemplates[i].TemplateName}", thrownException);
				}
			}
		}
	}
}