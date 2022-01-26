using UnityEditor;
using UnityEngine;

namespace Voodoo.Distribution
{
    public class LocalizationEditor : EditorWindow
	{
		int _sheetIndex;
		string[] _sheetNames;

		int _forcedIndex;
		string[] _languageCodes;
		string[] _translationKeys;

		Vector2 _scroll;

		[MenuItem("Voodoo/Localization/Setup")]
		public static void Open()
		{
			LocalizationEditor window = GetWindow<LocalizationEditor>();
			window.Show();
		}

		void OnEnable()
		{
			LocalizationIO.sheetChanged += Mount;
			Mount();
		}

        private void OnDisable()
        {
			if (EditorApplication.isPlaying == false)
			{
				Localization.Dispose();
				LocalizationIO.Dispose();
			}

			LocalizationIO.sheetChanged -= Mount;
		}

        void Mount()
        {
			_sheetNames = LocalizationIO.sheetNames();
			_sheetIndex = -1;

			int count = _sheetNames?.Length ?? 0;
			for (int i = 0; i < count; i++)
			{
				if (_sheetNames[i] == LocalizationIO.Sheet.name)
				{
					_sheetIndex = i;
					break;
				}
			}

			if (_sheetIndex < 0)
			{
				return;
			}

			_languageCodes = new string[LocalizationIO.Sheet.languageCodes.Length + 1];
			_languageCodes[0] = "None";
			LocalizationIO.Sheet.languageCodes.CopyTo(_languageCodes, 1);
			_forcedIndex = 0;

			_translationKeys = new string[LocalizationIO.Sheet.translationRows.Length];
            for (int i = 0; i < _translationKeys.Length; i++)
            {
				_translationKeys[i] = LocalizationIO.Sheet.translationRows[i].key;
            }
		}

        public void OnGUI()
		{
			if (_sheetIndex < 0)
			{
				GUI.color = Color.yellow;
				EditorGUILayout.LabelField("No sheet selected please import one and retry.");
				GUI.color = Color.white;

				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("Import"))
				{
					LocalizationImporterEditor.Open();
				}

				if (GUILayout.Button("?", GUILayout.Width(20f)))
				{
					Application.OpenURL("https://voodoo.atlassian.net/wiki/x/bYBnB");
				}

				EditorGUILayout.EndHorizontal();

				return;
			}

			GUILayout.Space(10);
			
			EditorGUI.BeginChangeCheck();
			_sheetIndex = EditorGUILayout.Popup("Sheet:", _sheetIndex, _sheetNames);
			if (EditorGUI.EndChangeCheck())
			{
				LocalizationIO.SelectSheet(_sheetIndex);
			}
			
			GUILayout.Space(10);
			
			//Use fuzzy search
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField("Forced Language");
			_forcedIndex = EditorGUILayout.Popup(_forcedIndex, _languageCodes);
			if (EditorGUI.EndChangeCheck())
			{
				Localization.forcedLanguage = _forcedIndex == 0 ? null : _languageCodes[_forcedIndex] ;
			}

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Translate All", GUILayout.Width(100)))
				{
					Localization.TranslateAll();
				}
				
				GUILayout.FlexibleSpace();	
			}
			EditorGUILayout.EndHorizontal();

			OnLocalizationGUI();
		}

		void OnLocalizationGUI()
		{
			GUIStyle text = new GUIStyle(EditorStyles.textField);

			_scroll = EditorGUILayout.BeginScrollView(_scroll);

			GUILayout.Space(20);

			for (int i = 0; i < _translationKeys.Length; i++)
			{
				if ((i & 1) == 0)
				{
					GUI.backgroundColor = Color.gray;
				}
				else
				{
					GUI.backgroundColor = Color.white;
				}

				string key = _translationKeys[i];

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(key, text);
				EditorGUILayout.LabelField(Localization.GetTranslation(key), text);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);
		}
	}
}