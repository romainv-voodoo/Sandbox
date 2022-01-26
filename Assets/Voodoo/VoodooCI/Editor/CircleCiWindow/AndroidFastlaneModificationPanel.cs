using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Voodoo.CI
{
    public class AndroidFastlaneModificationPanel : BuildPanelBase
    {
        private static string _androidBundleId = "";

        private static readonly string AppFilePath =
            Path.Combine(Application.dataPath, "../", "VoodooCI", "android", "fastlane", "AppFile");

        private const string AppFileAppIdPattern = "(?<=package_name.\")(.*)(?=\")";

        public static void ShowPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            GUILayout.Label($"Android", EditorStyles.largeLabel);
            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Get Current Bundle Id from CiSettings", GUILayout.Height(30)))
            {
                _androidBundleId = CiSettings.AndroidBundleId;
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            ShowTextField("Android Bundle ID: ", ref _androidBundleId, 400);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Update Android Bundle Id in Android Fastlane AppFile", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(_androidBundleId))
                {
                    Debug.LogError($"Bundle Id can't be empty");
                    return;
                }

                var newAppFile = CreateNewAppFile(_androidBundleId);

                ShowConfirmationPopup("Please verify the new Firebase Fastlane AppFile",
                    "See the logs to see updated files.\n" +
                    $"New App Bundle Id: {_androidBundleId}\n",
                    () =>
                    {
                        UpdateFile(newAppFile);
                        CiSettings.AndroidBundleId = _androidBundleId;
                        Debug.Log(
                            $"Data was saved successfully at the location: {AppFilePath}. Please commit the changes to git.");
                    });
            }

            GUILayout.EndVertical();
        }

        public static void UpdateFiles()
        {
            var newAppFile = CreateNewAppFile(CiSettings.AndroidBundleId);
            UpdateFile(newAppFile);
        }

        private static void UpdateFile(string newAppFile) => File.WriteAllText(AppFilePath, newAppFile);

        private static string CreateNewAppFile(string bundleId)
        {
            var appFile = File.ReadAllText(AppFilePath);
            var newAppFile = new Regex(AppFileAppIdPattern).Replace(appFile, bundleId);
            Debug.Log($"Android: New AppFile:\n {newAppFile}");
            return newAppFile;
        }
    }
}