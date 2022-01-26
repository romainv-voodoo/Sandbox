using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Voodoo.Store
{
	public abstract class PackageItemTreeView : IDownloadableTreeView<PackageItem>
	{
		protected readonly List<PackageItem> raw = new List<PackageItem>();
		protected List<PackageItem> selectedPackages = new List<PackageItem>();
		
		public abstract bool CanMultiSelect { get; }
		public abstract MultiColumnHeader Header { get; }
		
		public List<PackageItem> UpdateContent(List<IDownloadable> packages)
		{
			raw.Clear();
			foreach (IDownloadable package in packages)
			{
				Add(package);
			}

			return raw;
		}

		public abstract void Add(IDownloadable package);

		public virtual void Remove(IDownloadable package)
		{
			PackageItem packageItem = raw.Find(x => x.package.GetHashCode() == package.GetHashCode());
			
			if (packageItem != null)
				raw.Remove(packageItem);
		}

		public abstract void SetRawContent(PackagePreset preset);

		public virtual List<TreeViewItem> GetData()
		{
			return raw.Cast<TreeViewItem>().ToList();
		}
		
		public virtual List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
		{
			return GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();
		}

		protected abstract IEnumerable<PackageItem> GetSortedData0(int columnIndex, bool isAscending);

		public abstract void Draw(Rect rect, int columnIndex, PackageItem data, bool selected);

		public virtual void OnItemClick(PackageItem item) { }

		public virtual void OnItemDoubleClick(PackageItem item) { }

		public abstract void SelectionChanged(List<PackageItem> items);

		public abstract void OnContextClick();

		protected void DownloadPackages(GenericMenu menu)
		{
			int versionStatus = -1;
			foreach (PackageItem selectedPackage in selectedPackages)
			{
				if (selectedPackage.package.VersionStatus == VersionState.NotPresent)
				{
					versionStatus = VersionState.NotPresent;
					break;
				}

				if (selectedPackage.package.VersionStatus == VersionState.OutDated || selectedPackage.package.VersionStatus == VersionState.Manually || selectedPackage.package.VersionStatus == VersionState.Invalid)
				{
					versionStatus = VersionState.OutDated;
				}
			}

			if (versionStatus != -1)
			{
				GUIContent contentDownload = new GUIContent(versionStatus == VersionState.NotPresent ? "Download" : "Update");
				if (selectedPackages.Count == 1)
				{
					contentDownload.text += " " + selectedPackages[0].displayName;
				}
				else
				{
					contentDownload.text += " All";
				}
				
				menu.AddSeparator("");
				menu.AddItem(contentDownload, false, DownloadAllSelectedPackages);
			}
			else
			{
				GUIContent contentDownload = new GUIContent("Uninstall");
				if (selectedPackages.Count == 1)
				{
					contentDownload.text += " " + selectedPackages[0].displayName;
				}
				else
				{
					contentDownload.text += " All";
				}
				
				menu.AddItem(contentDownload, false, UninstallAllSelectedPackages);
			}
		}

		private void DownloadAllSelectedPackages()
		{
			List<Package> packages = new List<Package>();
			foreach (PackageItem packageItem in selectedPackages)
			{
				packages.Add(packageItem.package);
			}
			
			List<Package> dependencies = new List<Package>();
			foreach (Package package in packages)
			{
				dependencies.AddRange(package.GetDependencies(VoodooStore.packages));
			}

			dependencies = dependencies.Distinct().ToList();
			DownloadProcessor.DownloadPackages(dependencies);
		}

		private void UninstallAllSelectedPackages()
		{
			List<Package> packages = new List<Package>();
			foreach (PackageItem packageItem in selectedPackages)
			{
				if (packageItem.package.VersionStatus != VersionState.UpToDate)
					continue;
				
				packages.Add(packageItem.package);
			}
			
			List<Package> requirements = new List<Package>();
			foreach (Package package in packages)
			{
				requirements.AddRange(package.GetRequirements(VoodooStore.packages));
			}
			
			requirements = requirements.Distinct().ToList();
			DownloadProcessor.UninstallProcess(requirements);
		}
	}
}