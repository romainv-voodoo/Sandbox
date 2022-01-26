using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Network;

namespace Voodoo.Distribution
{
    public class LocalizationImporterEditor : EditorWindow
    {
        private const string localizationPath = "Assets/Voodoo/GameData/Localization";
        string _optionsFile = $"{localizationPath}/Options.asset";

        string                _sheetName    = string.Empty;
        LocalizationOptions   _options      = null;
        
        Task<UnityWebRequest> _webRequest;

        [MenuItem("Voodoo/Localization/Import", false, 1000)]        
        public static void Open()
        {
            LocalizationImporterEditor window = GetWindow<LocalizationImporterEditor>();
            window.Show();
        }

        void OnEnable()
        {
            _options = AssetDatabase.LoadAssetAtPath<LocalizationOptions>(_optionsFile);
            if (_options == null)
            {
                Directory.CreateDirectory(localizationPath);
                _options = CreateInstance<LocalizationOptions>();
                AssetDatabase.CreateAsset(_options, _optionsFile);
                AssetDatabase.SaveAssets();
            }
        }

        private void OnDisable()
        {
            if (EditorApplication.isPlaying == false)
            {
                LocalizationIO.Dispose();
            }
        }

        void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(_webRequest != null && _webRequest.IsCompleted == false);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            _options.sheetUrl = EditorGUILayout.TextField("URL to Sheet (ref only)", _options.sheetUrl);
            if (GUILayout.Button("Open", GUILayout.Width(55f)))
            {
                Application.OpenURL(_options.sheetUrl);
            }

            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _options.getRequestUrl = EditorGUILayout.TextField("URL to WEB app", _options.getRequestUrl);
            if (GUILayout.Button("✓", GUILayout.Width(25f)))
            {
                Application.OpenURL(_options.getRequestUrl);
            }
            if (GUILayout.Button("?", GUILayout.Width(25f)))
            {
                Application.OpenURL("https://voodoo.atlassian.net/wiki/spaces/VST/pages/73891949/Localization");
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            _sheetName = EditorGUILayout.TextField("Sheet name", _sheetName);
            GUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_sheetName))
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Sheet needs a name to be saved.");
                GUI.color = Color.white;
            }

            if (LocalizationIO.SheetIndexOf(_sheetName) >= 0)
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Sheet name already exist if you use that name you will overwrite it.");
                GUI.color = Color.white;
            }

            EditorGUILayout.Space();

            _options.bakefallbackFonts = EditorGUILayout.Toggle("Bake multi-alphabets font afetr conversion", _options.bakefallbackFonts);
            if (_options.bakefallbackFonts)
            {
                FontBakerEditor.OnFontSelectionGUI();
            }
            
            EditorGUILayout.Space();
            
            _options.closeAfterConversion = EditorGUILayout.Toggle("Close window after conversion", _options.closeAfterConversion);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_sheetName));
            
            GUI.color = Color.green;
            if (GUILayout.Button("Import From Google Sheets"))
            {
                ImportGoogleSheet(_options.getRequestUrl);
            }
            GUI.color = Color.white;

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_options);
            }

            EditorGUI.EndDisabledGroup();
        }

        async void ImportGoogleSheet(string getRequest)
        {
            EditorUtility.DisplayProgressBar("Import Google Sheet", "Recovering google sheet please wait...", 0f);
            _webRequest = WebRequest.GetAsync(getRequest);
            await _webRequest;
            EditorUtility.ClearProgressBar();

            if (_webRequest.Result.isHttpError || _webRequest.Result.isNetworkError)
            {
                EditorUtility.DisplayDialog("Import Google Sheet", $"Import ended with the following error:\n{_webRequest.Result.error}", "ok");
            }

            OnImportRequestEnd(_webRequest.Result);
        }

        void OnImportRequestEnd(UnityWebRequest request)
        {
            LocalizationImporter.JsonToSheet(request.downloadHandler.text, _sheetName);
            
            if (_options.bakefallbackFonts)
            {
                FontBaker.LocalizationToFont();
            }

            if (_options.closeAfterConversion)
            {
                LocalizationImporterEditor window = GetWindow<LocalizationImporterEditor>();
                window.Close();
            }

            ShowNotification(new GUIContent($"Import success !"), 1d);
            Repaint();
        }
    }
}