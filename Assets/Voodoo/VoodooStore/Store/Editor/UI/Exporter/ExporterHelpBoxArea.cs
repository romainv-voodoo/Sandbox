using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
	public class ExporterHelpBoxArea
	{
		public Action<ExporterHelpBox, bool> onFixAsked;

		private List<ExporterHelpBox> exporterHelpBoxesData = new List<ExporterHelpBox>();
		private Dictionary<string, int> idToQuantity = new Dictionary<string, int>();

		public bool IsEmpty => exporterHelpBoxesData == null || exporterHelpBoxesData.Count == 0;

		public void Add(ExporterHelpBox _exporterHelpBox)
		{
			exporterHelpBoxesData.Add(_exporterHelpBox);
			_exporterHelpBox.onClick += OnHelpBoxClicked;
			_exporterHelpBox.onHelpBoxExpand += OnHelpBoxUnfold;
			_exporterHelpBox.onFixAsked += OnFixAsked;

			if (idToQuantity.ContainsKey(_exporterHelpBox.Id))
			{
				idToQuantity[_exporterHelpBox.Id]++;
			}
			else
			{
				idToQuantity.Add(_exporterHelpBox.Id, 1);
			}
		}

		public void Remove(ExporterHelpBox _exporterHelpBox)
		{
			if (_exporterHelpBox == null)
				return;

			if (!exporterHelpBoxesData.Contains(_exporterHelpBox))
				return;

			exporterHelpBoxesData.Remove(_exporterHelpBox);
			_exporterHelpBox.onClick -= OnHelpBoxClicked;
			_exporterHelpBox.onHelpBoxExpand -= OnHelpBoxUnfold;

			idToQuantity[_exporterHelpBox.Id]--;
			if (idToQuantity[_exporterHelpBox.Id] == 0)
			{
				idToQuantity.Remove(_exporterHelpBox.Id);
			}
		}

		public void Display()
		{
			foreach (KeyValuePair<string, int> kvp in idToQuantity)
			{
				string id = kvp.Key;
				int quantity = kvp.Value;

				if (quantity == 1)
				{
					DisplayMonoHelpBox(id);
				}
				else
				{
					DisplayMultiHelpBox(kvp);
				}
			}
		}

		private void DisplayMonoHelpBox(string id)
		{
			ExporterHelpBox exporterHelpBox = exporterHelpBoxesData.Find(x => x.Id == id);

			exporterHelpBox?.Display();
		}

		private void DisplayMultiHelpBox(KeyValuePair<string, int> kvp)
		{
			ExporterHelpBox baseExporterHelpBox = exporterHelpBoxesData.Find(x => x.Id == kvp.Key);

			ExporterHelpBox exporterHelpBox = new ExporterHelpBox(baseExporterHelpBox);
			exporterHelpBox.message = kvp.Value + " " + exporterHelpBox.objectType;
			switch (exporterHelpBox.helpBoxType)
			{
				case HelpBoxType.IncorrectPath:
					exporterHelpBox.message += " have an incorrect path";
					break;
				case HelpBoxType.MissingNamespace:
					exporterHelpBox.message += " without namespace";
					break;
				case HelpBoxType.IncorrectNamespace:
					exporterHelpBox.message += " have an incorrect namespace";
					break;
			}

			exporterHelpBox.isExpandable = true;
			exporterHelpBox.onClick = OnHelpBoxClicked;
			exporterHelpBox.Display();
			baseExporterHelpBox.isExpand = exporterHelpBox.isExpand;
		}

		private void OnHelpBoxClicked(string id)
		{
			foreach (ExporterHelpBox exporterHelpBoxData in exporterHelpBoxesData)
			{
				if (exporterHelpBoxData.Id != id)
					continue;

				if (exporterHelpBoxData.isExpandable)
				{
//					Debug.Log(exporterHelpBoxData.message, exporterHelpBoxData.projectObject);
				}
				else
				{
					Selection.activeObject = exporterHelpBoxData.projectObject;
				}
			}
		}

		private void OnHelpBoxUnfold(string id)
		{
			foreach (ExporterHelpBox exporterHelpBoxData in exporterHelpBoxesData)
			{
				if (exporterHelpBoxData.Id == id)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(20);
					GUIStyle buttonStyle = new GUIStyle(EditorStyles.label) {wordWrap = true};
					if (GUILayout.Button(exporterHelpBoxData.message, buttonStyle))
					{
						Selection.activeObject = exporterHelpBoxData.projectObject;
					}
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		private void OnFixAsked(string id)
		{
			foreach (ExporterHelpBox exporterHelpBoxData in exporterHelpBoxesData)
			{
				if (exporterHelpBoxData.Id == id)
				{
					onFixAsked?.Invoke(exporterHelpBoxData, false);
				}
			}
		}

		public void FixAll(bool silently = false)
		{
			foreach (ExporterHelpBox exporterHelpBoxData in exporterHelpBoxesData)
			{
				onFixAsked?.Invoke(exporterHelpBoxData, silently);
			}
		}
	}
}