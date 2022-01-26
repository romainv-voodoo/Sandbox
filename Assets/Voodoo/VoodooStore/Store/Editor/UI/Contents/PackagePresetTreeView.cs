using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Voodoo.Store
{
	public class PackagePresetTreeView : IDownloadableTreeView<PackagePresetItem>
	{
		private readonly List<PackagePresetItem> raw = new List<PackagePresetItem>();

		public bool CanMultiSelect => false;
		private List<PackagePresetItem> selectedPackages = new List<PackagePresetItem>();

		private static readonly MultiColumnHeader header = new MultiColumnHeader(new MultiColumnHeaderState(new[]
		{
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Name"), width = 230, minWidth = 50, maxWidth = 230},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent(ContentHelper.UIStatusAscending), width = 45, minWidth = 45, maxWidth = 45},
		}));

		public MultiColumnHeader Header => header;

		public PackagePresetTreeView(List<PackagePreset> packages)
		{
			UpdateContent(packages.Cast<IDownloadable>().ToList());
		}

		public List<PackagePresetItem> UpdateContent(List<IDownloadable> packages)
		{
			raw.Clear();
			foreach (IDownloadable package in packages)
			{
				Add(package as PackagePreset);
			}

			return raw;
		}

		public void SetRawContent(PackagePreset preset)
		{
			raw.Clear();
			if (preset?.Content == null)
				return;

			UpdateContent(preset.Content.Cast<IDownloadable>().ToList());
		}
 
		public void Add(IDownloadable compo)
		{
			if (compo == null)
				return;
			
			PackagePresetItem packageItem = raw.Find(x => x.id == compo.GetHashCode());

			if (packageItem != null)
				return;
			
			PackagePresetItem newPackageItem = new PackagePresetItem
			{
				id          = compo.GetHashCode(),
				displayName = compo.Name,
				preset      = compo as PackagePreset
			};
 
			raw.Add(newPackageItem);
		}
 
		public void Remove(IDownloadable compo)
		{
			PackagePresetItem packageItem = raw.Find(x => x.preset.GetHashCode() == compo.GetHashCode());
			
			if (packageItem != null)
				raw.Remove(packageItem);
		}
 
		public List<TreeViewItem> GetData()
		{
			return raw.Cast<TreeViewItem>().ToList();
		}
 
		public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
		{
			return GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();
		}
 
		private IEnumerable<PackagePresetItem> GetSortedData0(int columnIndex, bool isAscending)
		{
			List<PackagePresetItem> sortedData = new List<PackagePresetItem>();
			
			switch (columnIndex)
			{
				case 0:
					sortedData = isAscending
						? raw.OrderBy(item => item.displayName).ToList()
						: raw.OrderByDescending(item => item.displayName).ToList();
					break;
				case 1:
					GUIContent newHeaderContent = new GUIContent(isAscending ? ContentHelper.UIStatusAscending : ContentHelper.UIStatusDescending);
					header.state.columns[columnIndex].headerContent = newHeaderContent;
					sortedData = isAscending
						? raw.OrderBy(item => item.preset.VersionStatus).ThenBy(item => item.displayName).ToList()
						: raw.OrderByDescending(item => item.preset.VersionStatus).ThenBy(item => item.displayName).ToList();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
			}
			
			PackagePresetItem favorite = raw.Find(x => x.displayName == "Favorites");
			sortedData.Remove(favorite);
			sortedData.Insert(0,favorite);
			
			PackagePresetItem cart = raw.Find(x => x.displayName == "Cart");
			sortedData.Remove(cart);
			sortedData.Insert(1,cart);
			
			return sortedData;
		}
 
		public void Draw(Rect rect, int columnIndex, PackagePresetItem data, bool selected)
		{
			GUIStyle labelStyle = selected ? new GUIStyle(EditorStyles.whiteLabel) : new GUIStyle(EditorStyles.label);
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.margin = new RectOffset(4, 4, 1, 1);
			labelStyle.padding = new RectOffset(1, 0, 1, 1);
			// labelStyle.normal.textColor = data.preset.Color;

			switch (columnIndex)
			{
				case 0:
					if (data.preset.Icon != null)
					{
						Rect iconRect = new Rect(rect.x,rect.y + (rect.height - 16)/2, 16, 16);
						Color tempColor = GUI.color;
						GUI.color = data.preset.Color;
						GUI.DrawTexture(iconRect, data.preset.Icon);
						GUI.color = tempColor;
						rect.width -= 20;
						rect.x += 20;
					}
					
					EditorGUI.LabelField(rect, data.displayName, labelStyle);
					break;
				case 1:
					EditorGUIHelper.ShowPackagesButtons(rect, data.preset);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
			}
		}

		public void OnItemClick(PackagePresetItem item)
		{
			// Debug.Log("Clicked on " + item.composition.Name);
			// VoodooStore.selection = item.package;
		}
 
		public void OnItemDoubleClick(PackagePresetItem item)
		{
			// Debug.Log("Double clicked on " + item.composition.Name);
			// VoodooStore.selection = item.package;
		}

		public void SelectionChanged(List<PackagePresetItem> items)
		{
			selectedPackages = new List<PackagePresetItem>(items);
			VoodooStore.selection.Clear();
			foreach (PackagePresetItem item in items)
			{
				VoodooStore.selection.Add(item.preset);
			}
		}

		public void OnContextClick()
		{
			//TODO : Define what to do on context click 
		}
	}
}