using UnityEditor;
using UnityEngine;
using Voodoo.Utils;

namespace Voodoo.Distribution
{
    [CustomEditor(typeof(Translator))]
	public class TranslatorEditor : Editor
	{
		FuzzySearchWidget _searchWidget;
		
		Translator _target;
		
		string[] _languageCodes;
		int		 _forcedIndex;
		
		void OnEnable()
		{
			_target = (Translator) target;

			LocalizationIO.sheetChanged += Mount;
			Mount();
		}

		private void OnDisable()
		{
			LocalizationIO.sheetChanged -= Mount;

			if (EditorApplication.isPlaying == false)
			{
				Localization.Dispose();
				LocalizationIO.Dispose();
			}
		}

		void Mount()
		{
			if (LocalizationIO.HasData == false)
			{
				_languageCodes = new string[] { "None" };
				_forcedIndex = 0;
				return;
			}
			
			_languageCodes = new string[LocalizationIO.Sheet.languageCodes.Length + 1];
			_languageCodes[0] = "None";
			LocalizationIO.Sheet.languageCodes.CopyTo(_languageCodes, 1);
			_forcedIndex = 0;

			var translationKeys = new string[LocalizationIO.Sheet.translationRows.Length];
			for (int i = 0; i < translationKeys.Length; i++)
			{
				translationKeys[i] = LocalizationIO.Sheet.translationRows[i].key;
			}

			_searchWidget = new FuzzySearchWidget(translationKeys, null, OnItemSelected, CloseFuzzySearch);
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			
			if (_target == null)
			{
				_target = target as Translator;
			}
			
			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			
			_target.AtStart = EditorGUILayout.ToggleLeft("At Start",_target.AtStart, GUILayout.Width(100));
			_target.AtEnable = EditorGUILayout.ToggleLeft("On Enable",_target.AtEnable, GUILayout.Width(100));
			
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			if (LocalizationIO.HasData == false)
			{
				EditorGUILayout.LabelField("There is no localization sheet please import one and retry.");

				if (GUILayout.Button("Import"))
                {
					LocalizationImporterEditor.Open();
                }

				return;
			}

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Key", GUILayout.Width(50));

			_searchWidget.ShowAsPopup(_target.key);

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Translate", GUILayout.Width(100)))
			{
				_target.Translate(_forcedIndex == 0 ? null : _languageCodes[_forcedIndex]);
			}

			_forcedIndex = EditorGUILayout.Popup(_forcedIndex, _languageCodes, GUILayout.Width(50));
			
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(10);
			
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_target);
			}
		}

		void OnItemSelected(int index)
		{
			if (index != int.MinValue)
			{
				_target.key = Localization.TranslationKeys[index];
			}

			CloseFuzzySearch();
		}

		private void CloseFuzzySearch()
		{
			EditorWindow.focusedWindow.Close();
		}
	}
}