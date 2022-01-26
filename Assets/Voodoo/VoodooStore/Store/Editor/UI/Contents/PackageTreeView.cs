using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Voodoo.Store
{
	public class PackageTreeView : PackageItemTreeView
	{

		private static readonly MultiColumnHeader header = new MultiColumnHeader(new MultiColumnHeaderState(new[]
		{
			new MultiColumnHeaderState.Column {headerContent = new GUIContent(ContentHelper.UIFavorite), width = 20, minWidth = 20, maxWidth = 20, canSort = true},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Name"), width = 210, minWidth = 50, maxWidth = 210},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent(ContentHelper.UIStatusAscending), width = 45, minWidth = 45, maxWidth = 45},
		}));

		public override MultiColumnHeader Header => header;
		public override bool CanMultiSelect => true;

		public PackageTreeView(List<Package> packages)
		{
			UpdateContent(packages.Cast<IDownloadable>().ToList());
		}

		public override void SetRawContent(PackagePreset preset)
		{
			raw.Clear();
			if (preset?.Content == null)
				return;

			UpdateContent(preset.Content.Cast<IDownloadable>().ToList());
		}
 
		public override void Add(IDownloadable pkg)
		{
			if (pkg == null)
				return;
			
			PackageItem packageItem = raw.Find(x => x.id == pkg.GetHashCode());

			if (packageItem != null)
				return;
			
			PackageItem newPackageItem = new PackageItem
			{
				id          = pkg.GetHashCode(),
				displayName = pkg.Name,
				package     = pkg as Package
			};
 
			raw.Add(newPackageItem);
		}

		protected override IEnumerable<PackageItem> GetSortedData0(int columnIndex, bool isAscending)
		{
			GUIContent newHeaderContent;
			switch (columnIndex)
			{
				case 0:
					newHeaderContent = new GUIContent(isAscending ? ContentHelper.UIFavoriteEmpty : ContentHelper.UIFavorite);
					header.state.columns[columnIndex].headerContent = newHeaderContent;
					return isAscending
						? raw.OrderBy(item => item.IsFavorite).ThenBy(item => item.displayName)
						: raw.OrderByDescending(item => item.IsFavorite).ThenBy(item => item.displayName);
				case 1:
					return isAscending
						? raw.OrderBy(item => item.displayName)
						: raw.OrderByDescending(item => item.displayName);
				case 2:
					newHeaderContent = new GUIContent(isAscending ? ContentHelper.UIStatusAscending : ContentHelper.UIStatusDescending);
					header.state.columns[columnIndex].headerContent = newHeaderContent;
					return isAscending
						? raw.OrderBy(item => item.package.VersionStatus).ThenBy(item => item.displayName)
						: raw.OrderByDescending(item => item.package.VersionStatus).ThenBy(item => item.displayName);
				default:
					throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
			}
		}
 
		public override void Draw(Rect rect, int columnIndex, PackageItem data, bool selected)
		{
			GUIStyle labelStyle = selected ? new GUIStyle(EditorStyles.whiteLabel) : new GUIStyle(EditorStyles.label);
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.margin = new RectOffset(4, 4, 1, 1);
			labelStyle.padding = new RectOffset(1, 0, 1, 1);

			switch (columnIndex)
			{
				case 0:
					rect = EditorGUIHelper.AlignRect(rect, GUI.skin.button, SpriteAlignment.Center);
					
					bool isFavorite = VoodooStore.favorites.Contains(data.package);
					Color initialContentColor = GUI.contentColor;
					GUI.contentColor = isFavorite ? ContentHelper.VSTYellow : Color.white;
					GUIContent favorite = new GUIContent(isFavorite ? ContentHelper.UIFavorite : ContentHelper.UIFavoriteEmpty, isFavorite ? "Remove from Favorites" : "Add to Favorites");
					if (GUI.Button(rect, favorite, GUIStyle.none))
					{
						if (isFavorite)
						{
							VoodooStore.favorites.Remove(data.package);
						}
						else
						{
							VoodooStore.favorites.Add(data.package);
						}
					}
					GUI.contentColor = initialContentColor;
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
			VoodooStore.selection.Clear();
			foreach (PackageItem item in items)
			{
				VoodooStore.selection.Add(item.package);
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
			if (selectedPackages.Count == 1)
			{
				menu.AddDisabledItem(new GUIContent(selectedPackages[0].displayName));
				menu.AddSeparator("");
			}
			
			AddToMenuItem(menu, VoodooStore.cart);
			AddToMenuItem(menu, VoodooStore.favorites);
			
			foreach (PackagePreset preset in VoodooStore.presets)
			{
				AddToMenuItem(menu, preset);
			}

			DownloadPackages(menu);
			
			menu.ShowAsContext();
		}
		
		private void AddToMenuItem(GenericMenu menu, PackagePreset preset)
		{
			int packageInCompo = 0;
			foreach (PackageItem selectedPackage in selectedPackages)
			{
				if (preset.Contains(selectedPackage.package))
				{
					packageInCompo++;
				}
			}

			bool checkboxOn = packageInCompo == selectedPackages.Count;
			menu.AddItem(EditorGUIUtility.TrTextContent("Add to " + preset.Name), checkboxOn , () => AddToComposition(selectedPackages, preset, checkboxOn));
		}
		
		private void AddToComposition(List<PackageItem> packages, PackagePreset preset, bool isInPreset)
		{
			foreach (PackageItem packageItem in packages)
			{
				if (isInPreset)
				{
					preset.Remove(packageItem.package);
				}
				else
				{
					preset.Add(packageItem.package);
				}
			}
		}
	}
}