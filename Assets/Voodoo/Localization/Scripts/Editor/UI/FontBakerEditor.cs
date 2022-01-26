using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Distribution
{
    public class FontBakerEditor : EditorWindow
    {
        static Vector2 _scroll;
        
        string[] _sheetNames;
        int _sheetIndex;


        [MenuItem("Voodoo/Localization/Bake fonts")]
        static void Init()
        {
            FontBakerEditor window = GetWindow<FontBakerEditor>();
            window.Show();
        }

        private void OnEnable()
        {
            LocalizationIO.sheetChanged += Mount;
            Mount();
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
        }

        private void OnDisable()
        {
            LocalizationIO.sheetChanged -= Mount;

            if (EditorApplication.isPlaying == false)
            {
                LocalizationIO.Dispose();
            }
        }

        void OnGUI()
        {
            OnInputsGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(FontBaker.AreInputsValid == false);
            if (GUILayout.Button("Bake"))
            {
                FontBaker.LocalizationToFont();
            }
            EditorGUI.EndDisabledGroup();
        }

        public void OnInputsGUI()
        {
            if (OnSheetSelectionGUI() == false)
            {
                return;
            }

            OnFontSelectionGUI();
        }

        public bool OnSheetSelectionGUI()
        {
            if (_sheetIndex < 0)
            {
                GUI.color = Color.red;
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

                return false;
            }

            EditorGUI.BeginChangeCheck();
            _sheetIndex = EditorGUILayout.Popup("Sheet:", _sheetIndex, _sheetNames);
            if (EditorGUI.EndChangeCheck())
            {
                LocalizationIO.SelectSheet(_sheetIndex);
            }

            EditorGUILayout.Space();

            return true;
        }

        public static void OnFontSelectionGUI()
        {
            GUILayout.BeginHorizontal();
            FontBaker.bakingFolder =  EditorGUILayout.TextField("Font folder:", FontBaker.bakingFolder);
            if (GUILayout.Button("...", GUILayout.Width(30f)))
            {
                var folderPath = FontBaker.bakingFolder;
                if (string.IsNullOrEmpty(folderPath))
                {
                    folderPath = PathHelper.ToolAssetPath(Path.Combine("Localization", "Fonts"));
                }
                
                FontBaker.bakingFolder = EditorUtility.OpenFolderPanel("Select the folder containing the Font to bake", folderPath, "asset");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            int count = FontBaker.candidateFonts.Count;
            for (int i = 0; i < count; i++)
            {
                GUILayout.BeginHorizontal("Box");

                EditorGUI.BeginChangeCheck();
                bool shouldBake = EditorGUILayout.Toggle(FontBaker.candidateFonts[i].Item1.name, FontBaker.candidateFonts[i].Item2);
                if (EditorGUI.EndChangeCheck())
                {
                    FontBaker.candidateFonts[i] = (FontBaker.candidateFonts[i].Item1, shouldBake);
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;

            if (FontBaker.candidateFonts.Exists(font => font.Item2) == false)
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("The baking folder is invalid or there is no selected fonts please fix it and retry.");
                GUI.color = Color.white;
            }
        }
    }
}
