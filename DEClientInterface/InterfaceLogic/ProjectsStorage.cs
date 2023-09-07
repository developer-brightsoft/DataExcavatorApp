// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.InterfaceLogic.ProjectsStorage
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using DEClientInterface;
using DEClientInterface.Controls;
using DEClientInterface.Logic;
using DEClientInterface.Objects;
using ExcavatorSharp.Excavator;
using ExcavatorSharp.Licensing;
using Newtonsoft.Json;

namespace DEClientInterface.InterfaceLogic
{
	public class ProjectsStorage
	{
		public List<DEProjectLink> SavedProjectsLinks = new List<DEProjectLink>();

		public void FlushIndexFileToHDD()
		{
			string contents = JsonConvert.SerializeObject(SavedProjectsLinks, Formatting.Indented);
			try
			{
				if (File.Exists(IOCommon.GetDataExcavatorUIIndexFilePath()))
				{
					File.Delete(IOCommon.GetDataExcavatorUIIndexFilePath());
				}
				File.WriteAllText(IOCommon.GetDataExcavatorUIIndexFilePath(), contents);
			}
			catch (Exception thrownException)
			{
				MessageBox.Show("It is not possible to save the project index file. The list of your projects is not saved. Please try again or contact support.", "Error in data saving", MessageBoxButton.OK, MessageBoxImage.Hand);
				Logger.LogError($"Cannot save projects index file. File location = {IOCommon.GetDataExcavatorUIIndexFilePath()}", thrownException);
			}
		}

		public void RemoveTaskFromIndexFile(DEProjectLink RemovingProject)
		{
			SavedProjectsLinks.Remove(RemovingProject);
			FlushIndexFileToHDD();
		}

		public void LoadIndexFileFromHDD(MainWindow MainWindowLink)
		{
			MainWindowLink.ClearWorkingAreaAndProjectsStorage();
			string dataExcavatorUIIndexFilePath = IOCommon.GetDataExcavatorUIIndexFilePath();
			if (!File.Exists(dataExcavatorUIIndexFilePath))
			{
				return;
			}
			string empty = string.Empty;
			try
			{
				empty = File.ReadAllText(dataExcavatorUIIndexFilePath);
			}
			catch (Exception thrownException)
			{
				Logger.LogError($"Cannot open projects file, file path = {dataExcavatorUIIndexFilePath}", thrownException);
				return;
			}
			DEProjectLink dEProjectLink = new DEProjectLink();
			List<DEProjectLink> list = new List<DEProjectLink>();
			int num = 0;
			try
			{
				list = dEProjectLink.UnserializeListFromJSON(empty);
			}
			catch (Exception thrownException2)
			{
				Logger.LogError("Project storage: index file reading exception", thrownException2);
			}
			LicenseKey actualLicenseKeyCopy = MainWindowLink.DETasksFactoryCoreStorage.GetActualLicenseKeyCopy();
			for (int i = 0; i < list.Count; i++)
			{
				DataExcavatorTask dataExcavatorTask = null;
				try
				{
					dataExcavatorTask = DataExcavatorTask.ReadTaskFromHDD(list[i].ProjectPath);
				}
				catch (Exception thrownException3)
				{
					Logger.LogError("Project storage: cannot read project data", thrownException3);
					dataExcavatorTask = null;
				}
				try
				{
					if (dataExcavatorTask != null)
					{
						num++;
						list[i].TaskLink = dataExcavatorTask;
						DEProjectCube dEProjectCube = MainWindowLink.ProcessAddDataExcavatorTaskIntoUI(list[i]);
						if (actualLicenseKeyCopy == null || (actualLicenseKeyCopy.KeyProjectsLimit != -1 && num > actualLicenseKeyCopy.KeyProjectsLimit))
						{
							dEProjectCube.ShowProjectsLimitiationOverlay();
						}
					}
				}
				catch (Exception thrownException4)
				{
					Logger.LogError("Cannot process project file into UI", thrownException4);
				}
			}
			if (num > 0)
			{
				MainWindowLink.NoProjectsYetOverlay.Visibility = Visibility.Hidden;
			}
		}
	}
}
