using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Voodoo.Store
{
	public interface IDownloadableTreeView<T> where T : TreeViewItem
	{
		bool CanMultiSelect { get; }
		List<T> UpdateContent(List<IDownloadable> packages);
		void SetRawContent(PackagePreset preset);
		MultiColumnHeader Header { get; }
		List<TreeViewItem> GetData();
		List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending);
		void Draw(Rect rect, int columnIndex, T data, bool selected);
		void OnItemClick(T item);
		void OnItemDoubleClick(T item);
		void SelectionChanged(List<T> items);
		void OnContextClick();
		void Add(IDownloadable package);
		void Remove(IDownloadable package);
	}
}