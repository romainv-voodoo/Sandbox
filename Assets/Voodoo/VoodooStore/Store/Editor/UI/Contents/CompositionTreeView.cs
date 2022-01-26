using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Voodoo.Store
{
	public class CompositionTreeView : PackageItemTreeView
	{
		private PackagePreset packagePreset;

		public override bool CanMultiSelect => true;
		public string Name => packagePreset.Name;

		private static readonly MultiColumnHeader header = new MultiColumnHeader(new MultiColumnHeaderState(new[]
		{
			new MultiColumnHeaderState.Column {headerContent = new GUIContent(ContentHelper.UITrash), width = 20, minWidth = 20, maxWidth = 20},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Name"), width = 250, minWidth = 150, maxWidth = 500},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent(ContentHelper.UIStatusAscending), width = 45, minWidth = 45, maxWidth = 45},
		}));
		
		public sealed override MultiColumnHeader Header => header;

		private int sortedColumnIndex;
 
		public CompositionTreeView(PackagePreset packagePreset)
		{
			this.packagePreset = packagePreset;
			SetRawContent(packagePreset);
			Header.sortingChanged += OnSortChanged;
			sortedColumnIndex = Header.sortedColumnIndex;
		}

		public sealed override void SetRawContent(PackagePreset preset)
		{
			packagePreset = preset;
			UpdateContent(preset.Content.Cast<IDownloadable>().ToList());
		}
 
		public override void Add(IDownloadable pkg)
		{
			if (pkg == null)
				return;
			
			PackageItem packageItem = raw.Find(x => x.package.GetHashCode() == pkg.GetHashCode());

			if (packageItem != null)
				return;
			
			PackageItem newPackageItem = new PackageItem
			{
				id          = pkg.GetHashCode(),
				displayName = pkg.Name,
				package     = pkg as Package
			};
			
			if (packagePreset.Content.Contains(pkg))
			{
				raw.Add(newPackageItem);
			}
		}

		protected override IEnumerable<PackageItem> GetSortedData0(int columnIndex, bool isAscending)
		{
			switch (columnIndex)
			{
				case 1:
					return isAscending
						? raw.OrderBy(item => item.displayName)
						: raw.OrderByDescending(item => item.displayName);
				case 2:
					return isAscending
						? raw.OrderBy(item => item.package.VersionStatus)
						: raw.OrderByDescending(item => item.package.VersionStatus);
				default:
					return raw;
			}
		}

		private void OnSortChanged(MultiColumnHeader multiColumnHeader)
		{
			int index = multiColumnHeader.sortedColumnIndex;
			if (index == 0 )
			{
				if (packagePreset.Count > 0)
				{
					string message = "You are going to remove all the packages of the preset \"" + packagePreset.Name + "\". Are you sure ?";
					bool confirmation = EditorUtility.DisplayDialog("Warning", message, "Yes", "Cancel");

					if (confirmation)
					{
						packagePreset.Clear();
					}
				}

				multiColumnHeader.SetSorting(sortedColumnIndex, multiColumnHeader.IsSortedAscending(sortedColumnIndex));
				return;
			}

			sortedColumnIndex = multiColumnHeader.sortedColumnIndex;
		}
 
		public override void Draw(Rect rect, int columnIndex, PackageItem data, bool selected)
		{
			GUIStyle labelStyle = selected ? new GUIStyle(EditorStyles.whiteLabel) : new GUIStyle(EditorStyles.label);
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.margin = new RectOffset(4, 4, 1, 1);
			labelStyle.padding = new RectOffset(4, 0, 1, 1);

			switch (columnIndex)
			{
				case 0:
					Rect buttonRect = EditorGUIHelper.AlignRect(rect, GUI.skin.button, SpriteAlignment.Center);
					
					if (GUI.Button(buttonRect, ContentHelper.UITrash, new GUIStyle()))
					{
						bool confirmation = true;
						if (packagePreset.Name == "Selection")
						{
							VoodooStore.selection.Remove(data.package);
							packagePreset.Remove(data.package);
						}
						else
						{
							string message = "You are going to remove \"" + data.displayName + "\" from the preset \"" + packagePreset.Name + "\". Are you sure ?";
							confirmation = EditorUtility.DisplayDialog("Warning", message, "Yes", "Cancel");
						}

						if (confirmation)
						{
							packagePreset.Remove(data.package);
						}
						
					}
					break;
				case 1:
					EditorGUI.LabelField(rect, data.displayName, labelStyle);
					break;
				case 2:
					EditorGUIHelper.ShowPackagesButtons(rect, data.package);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
			}
		}

		public override void SelectionChanged(List<PackageItem> items)
		{
			if (items.Count == 0)
			{
				selectedPackages = new List<PackageItem>();
				return;
			}
			
			selectedPackages = new List<PackageItem>(items);
		}

		public override void OnContextClick()
		{
			if (selectedPackages == null || selectedPackages.Count == 0)
			{
				return;
			}
			
			GenericMenu menu = new GenericMenu();
			GUIContent contentRemove = new GUIContent("Remove");
			
			if (selectedPackages.Count == 1)
			{
				contentRemove.text += " " + selectedPackages[0].displayName;
			}
			else
			{
				contentRemove.text += " All";
			}
			
			menu.AddItem(contentRemove, false, RemoveAllSelectedPackages);

			DownloadPackages(menu);
			
			menu.ShowAsContext();
		}

		private void RemoveAllSelectedPackages()
		{
			if (packagePreset.Name != "Selection")
			{
				string message;
				if (selectedPackages.Count == 1)
				{
					message = "You are going to remove \"" + selectedPackages[0].displayName + "\" from the preset \"" + packagePreset.Name + "\". Are you sure ?";
				}
				else
				{
					message = "You are going to remove the following packages from the preset \"" + packagePreset.Name + "." + Environment.NewLine + Environment.NewLine;

					for (var i = 0; i < selectedPackages.Count; i++)
					{
						message += selectedPackages[i].package.displayName + Environment.NewLine;
					}

					message += Environment.NewLine;
					message += Environment.NewLine + "Are you sure ?";
				}

				bool confirmation = EditorUtility.DisplayDialog("Warning", message, "Yes", "Cancel");

				if (!confirmation)
					return;
			}
			
			foreach (PackageItem packageItem in selectedPackages)
			{
				packagePreset.Remove(packageItem.package);
				raw.Remove(packageItem);
				VoodooStore.selection.Remove(packageItem.package);
			}
			selectedPackages.Clear();
		}
	}
}