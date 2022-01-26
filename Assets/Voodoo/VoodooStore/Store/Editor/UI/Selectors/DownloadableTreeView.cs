using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Voodoo.Store
{
	public class DownloadableTreeView<T> : TreeView where T : TreeViewItem
	{
		const string sortedColumnIndexStateKey = "DownloadableTreeView_sortedColumnIndex";
		public IDownloadableTreeView<T> viewContent;
		private readonly Type delegateType;
		
		public DownloadableTreeView(IDownloadableTreeView<T> downloadableTreeView, bool subscribeToCollectionChanged = true) : this(new TreeViewState(), downloadableTreeView.Header, downloadableTreeView.GetType())
		{
			viewContent = downloadableTreeView;
			if(subscribeToCollectionChanged)
			{
				VoodooStore.selection.CollectionChanged += OnSelectionChanged;
			}
			FiltersEditor.onApplyFilter += OnApplyFilters;
		}

		public DownloadableTreeView(TreeViewState state, MultiColumnHeader header, Type delegateType) : base(state, header)
		{
			rowHeight = 28;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			header.sortingChanged += SortingChanged;
			
			header.ResizeToFit();
			Reload();

			this.delegateType = delegateType; //Storing delegateType because viewContent is null at this point
			int columnIndex = SessionState.GetInt(sortedColumnIndexStateKey + "_" + delegateType.Name + "_index", 1);
			bool columnValue = SessionState.GetBool(sortedColumnIndexStateKey + "_" + delegateType.Name + "_AscendingValue", true);
			header.GetColumn(columnIndex).sortedAscending = columnValue;
			header.sortedColumnIndex = columnIndex;
		}

		private void OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				state.selectedIDs = new List<int>();
				return;
			}

			List<T> visibleItems = viewContent.GetData().Cast<T>().ToList();
			UpdateSelectedIds(visibleItems);
		}

		private void OnApplyFilters(List<Package> packages)
		{
			List<T> visibleItems = viewContent.UpdateContent(packages.Cast<IDownloadable>().ToList());
			UpdateSelectedIds(visibleItems);
		}

		private void UpdateSelectedIds(List<T> visibleItems)
		{
			state.selectedIDs = new List<int>();
			foreach (T item in visibleItems)
			{
				if (item is PackageItem packageItem && VoodooStore.selection.Contains(packageItem.package))
				{
					state.selectedIDs.Add(packageItem.id);
				}
				
				if (item is PackagePresetItem packageCompositionItem && VoodooStore.selection.Contains(packageCompositionItem.preset))
				{
					state.selectedIDs.Add(packageCompositionItem.id);
				}
			}
		}

		protected override TreeViewItem BuildRoot()
		{
			var root = new TreeViewItem {depth = -1, children = new List<TreeViewItem>()};
			return root;
		}

		public void Refresh()
		{
			int sortedColumnIndex = multiColumnHeader.sortedColumnIndex;
			bool isAscending = multiColumnHeader.IsSortedAscending(sortedColumnIndex);
			
			rootItem.children = viewContent.GetSortedData(sortedColumnIndex,isAscending);
			BuildRows(rootItem);
		}

		private void SortingChanged(MultiColumnHeader header)
		{
			SessionState.SetInt(sortedColumnIndexStateKey + "_" + delegateType.Name + "_index", multiColumnHeader.sortedColumnIndex);
			SessionState.SetBool(sortedColumnIndexStateKey + "_" + delegateType.Name + "_AscendingValue", header.IsSortedAscending(header.sortedColumnIndex));

			if (viewContent == null)
			{
				rootItem.children = new List<TreeViewItem>();
				BuildRows(rootItem);
				return;
			}

			int index = multiColumnHeader.sortedColumnIndex;
			bool ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

			rootItem.children = viewContent.GetSortedData(index, ascending);
			BuildRows(rootItem);
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return viewContent.CanMultiSelect;
		}

		protected override void ContextClicked()
		{
			viewContent?.OnContextClick();
		}

		protected override void SingleClickedItem(int id)
		{
			T treeViewItem = viewContent.GetData().Find(x => x.id == id) as T;
			viewContent?.OnItemClick(treeViewItem);
		}

		protected override void DoubleClickedItem(int id)
		{
			T treeViewItem = viewContent.GetData().Find(x => x.id == id) as T;
			viewContent?.OnItemDoubleClick(treeViewItem);
		}
		
		protected override void SelectionChanged(IList<int> selectedIds)
		{
			List<T> treeViewItems = new List<T>();
			foreach (int selectedId in selectedIds)
			{
				T treeViewItem = viewContent.GetData().Find(x => x.id == selectedId) as T;
				treeViewItems.Add(treeViewItem);
			}
			viewContent?.SelectionChanged(treeViewItems);
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = args.item as T;

			for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
			{
				var rect = args.GetCellRect(visibleColumnIndex);
				var columnIndex = args.GetColumn(visibleColumnIndex);

				viewContent.Draw(rect, columnIndex, item, args.selected);
			}
		}
	}
	
	public class PackageItem : TreeViewItem
	{
		public Package package { get; set; }
		public bool IsFavorite => VoodooStore.favorites.Contains(package);
	}
	
	public class PackagePresetItem : TreeViewItem
	{
		public PackagePreset preset { get; set; }
	}
}