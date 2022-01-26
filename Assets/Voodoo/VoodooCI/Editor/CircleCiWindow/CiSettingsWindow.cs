using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class CiSettingsWindow : EditorWindow
    {
        private static string _jsonFilePath = "";
        private const string JsonFileName = "vcisettings.json";

        private static CiSettingsData _ciSettingsData;
        private Vector2 _scrollPos;

        public static CiSettingsData GetSettings()
        {
            LoadFromJsonFile();
            return _ciSettingsData;
        }

        public static void Initialize()
        {
            GetWindow(typeof(CiSettingsWindow), false, "CI Settings");
            GetSettings();
        }

        private static void ShowTextField(string title, ref string textFieldString, int width = -1)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, EditorStyles.largeLabel);
            textFieldString = width != -1
                ? GUILayout.TextField(textFieldString, GUILayout.Width(width))
                : GUILayout.TextField(textFieldString);
            GUILayout.EndHorizontal();
        }

        private static void ShowToggleField(string title, ref bool textFieldString, int width = -1)
        {
            GUILayout.BeginHorizontal();
            textFieldString = GUILayout.Toggle(textFieldString, title, EditorStyles.toggle);
            GUILayout.EndHorizontal();
        }

        private static void ShowLabel(string title)
        {
            GUILayout.Label(title, EditorStyles.whiteLargeLabel);
        }

        private static void ShowSpace(int space)
        {
            EditorGUILayout.Space(space);
        }

        private static bool ShowButton(string text)
        {
            return GUILayout.Button(text, GUILayout.Height(50));
        }

        private static void LoadFromJsonFile()
        {
            _jsonFilePath = Path.Combine(Application.dataPath, "../", $"{JsonFileName}");
            try
            {
                var serializedObject = File.ReadAllText(_jsonFilePath);
                var newObject = JsonConvert.DeserializeObject<CiSettingsData>(serializedObject);
                Debug.Log($"{newObject.AndroidBundleId}");
                if (_ciSettingsData == null)
                {
                    _ciSettingsData = new CiSettingsData();
                }

                _ciSettingsData.CopyFrom(newObject);
                Debug.Log(
                    $"CiSettings File loaded from {_jsonFilePath}.");
            }
            catch (Exception e)
            {
                _ciSettingsData = new CiSettingsData();
                Debug.LogError($"{e}");
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
            ShowSpace(10);

            if (ShowButton("Load Data from JSON"))
            {
                LoadFromJsonFile();
            }

            if (_ciSettingsData == null) return;

            ShowSpace(10);
            ShowLabel("Docker Images");
            ShowTextField("Android Docker:", ref _ciSettingsData.GameCiDockerTagAndroid, 250);
            ShowTextField("iOS Docker:", ref _ciSettingsData.GameCiDockerTagIos, 250);

            ShowSpace(20);
            ShowLabel("Apple and Android Fastlane Information");
            ShowTextField("Android Bundle Id:", ref _ciSettingsData.AndroidBundleId, 250);
            ShowTextField("iOS Bundle Id:", ref _ciSettingsData.IosBundleId, 250);
            ShowTextField("Apple Team Id:", ref _ciSettingsData.AppleTeamId, 250);
            ShowTextField("Apple Team Context:", ref _ciSettingsData.AppleTeamContext, 250);

            ShowSpace(20);
            ShowLabel("S3 Bucket Folder");
            ShowTextField("S3 Bucket Folder:", ref _ciSettingsData.S3BucketFolder, 250);

            ShowSpace(20);
            ShowLabel("Android Keystore file");
            ShowToggleField("Use Android Keystore file:", ref _ciSettingsData.UseKeyStore, 250);
            if (_ciSettingsData.UseKeyStore)
                ShowTextField("Android Keystore file path:", ref _ciSettingsData.KeyStoreFileAddress, 250);

            ShowSpace(20);
            ShowLabel("Unity Project Path Directory");
            ShowToggleField("Use extra project path for Unity:", ref _ciSettingsData.UseExtraUnityProjectPath, 250);

            if (_ciSettingsData.UseExtraUnityProjectPath)
            {
                _ciSettingsData.UnityProjectPath =
                    GUILayout.TextField(_ciSettingsData.UnityProjectPath, GUILayout.Width(250));
            }

            ShowSpace(20);
            ShowLabel("Test Runner and Report Generators");
            ShowToggleField("Enable Test runner and HTML Report generator:", ref _ciSettingsData.EnableTestRunner, 250);

            if (_ciSettingsData.EnableTestRunner)
            {
                ShowToggleField("Enable EditMode Test:", ref _ciSettingsData.EnableEditModeTest, 250);
                ShowToggleField("Enable PlayMode Test:", ref _ciSettingsData.EnablePlayModeTest, 250);
            }

            ShowSpace(20);
            ShowLabel("Enable Caching (Experimental)");
            ShowToggleField("Enable Caching of Library folder:", ref _ciSettingsData.EnableCaching, 250);

            ShowSpace(20);
            ShowLabel("Enable Auto Build on PR to master or main (This will rewrite CircleCI config file)");
            ShowToggleField("Enable Auto Build:", ref _ciSettingsData.EnableAutoBuild, 250);

            ShowSpace(10);

            ShowSpace(20);
            ShowLabel("Additional Options");
            ShowToggleField("Use git version:", ref _ciSettingsData.UseGitVersionNumber, 250);
            ShowToggleField("Use CircleCI SHA-1:", ref _ciSettingsData.UseCircleCiSha1, 250);
            ShowToggleField("Use Automated Build Version:", ref _ciSettingsData.UseAutomatedBundleVersionNumber, 250);
            ShowToggleField("Save Android Symbols Zip:", ref _ciSettingsData.CreateSymbolsZipFile, 250);

            ShowSpace(20);

            ShowLabel("(!) Commit vcisettings.json file to git to see the changes in effect!");

            if (_ciSettingsData.EnableAutoBuild)
            {
                ShowLabel(
                    "(!) Auto build on PR to main branch is enabled. Commit .circleci/config.yml file to git to see the changes in effect!");
            }

            ShowSpace(20);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (ShowButton("Save and Close"))
            {
                if (_ciSettingsData.EnableAutoBuild)
                {
                    if (CircleCiConfigFileEditor.TryUpdateConfigFile(_ciSettingsData))
                    {
                        SaveSettingsAsJson();
                        Close();
                    }
                }
                else
                {
                    SaveSettingsAsJson();
                    Close();
                }
            }

            if (ShowButton("Close without saving"))
            {
                Close();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }


        private void SaveSettingsAsJson()
        {
            var serializedObject = JsonConvert.SerializeObject(_ciSettingsData, Formatting.Indented);
            _jsonFilePath = Path.Combine(Application.dataPath, "../", $"{JsonFileName}");

            if (File.Exists(_jsonFilePath))
            {
                File.Delete(_jsonFilePath);
            }

            File.WriteAllText(_jsonFilePath, serializedObject);
            Debug.Log($"CiSettings File saved at {_jsonFilePath}. Consider commiting this file in Version Control.");
        }
    }
}