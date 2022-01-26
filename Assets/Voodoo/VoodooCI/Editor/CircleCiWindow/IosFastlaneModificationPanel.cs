using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class IosFastlaneModificationPanel : BuildPanelBase
    {
        private static string _iosBundleId = "";
        private static string _appleTeamId = "";

        private static readonly string AppFilePath =
            Path.Combine(Application.dataPath, "../", "VoodooCI", "ios", "fastlane", "AppFile");

        private static readonly string MatchFilePath =
            Path.Combine(Application.dataPath, "../", "VoodooCI", "ios", "fastlane", "MatchFile");

        private static readonly string AppFileAppIdPattern = "(?<=app_identifier \")(.*)(?=\")";
        private static readonly string AppFileTeamIdPattern = "(?<=team_id \")(.*)(?=\")";
        private static readonly string MatchFileGitBranchPatten = "(?<=git_branch.\")(.*)(?=\")";
        private static readonly string MatchFileAppIdentifierPattern = "(?<=app_identifier.\")(.*)(?=\")";

        public static void ShowPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            GUILayout.Label($"IOS", EditorStyles.largeLabel);
            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Get Current Bundle Id from CiSettings", GUILayout.Height(30)))
            {
                _iosBundleId = CiSettings.IosBundleId;
                _appleTeamId = CiSettings.AppleTeamId;
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            ShowTextField("iOS Bundle ID: ", ref _iosBundleId, 400);
            EditorGUILayout.Space(5);
            ShowTextField("Apple Team ID: ", ref _appleTeamId, 400);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Update iOS Bundle Id in AppFile and MatchFile", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(_iosBundleId) || string.IsNullOrEmpty(_appleTeamId))
                {
                    Debug.LogError($"Bundle Id or Apple Team Id can't be empty");
                    return;
                }

                var (newAppFile, newMatchFile) = GetNewFile(_iosBundleId, _appleTeamId);

                ShowConfirmationPopup("Please verify the new Testflight Files",
                    "See the logs to see updated files.\n" +
                    $"New App Bundle Id: {_iosBundleId}\n" +
                    $"New Apple Team Id: {_appleTeamId}",
                    () =>
                    {
                        CiSettings.IosBundleId = _iosBundleId;
                        CiSettings.AppleTeamId = _appleTeamId;

                        UpdateFile(newAppFile, newMatchFile);

                        Debug.Log(
                            $"Data was saved successfully at the location: {AppFilePath} and {MatchFilePath}. Please commit the changes to git.");
                    });
            }

            EditorGUILayout.Space(5);
            GUILayout.EndVertical();
        }

        public static void UpdateFiles()
        {
            var (newAppFile, newMatchFile) = GetNewFile(CiSettings.IosBundleId, CiSettings.AppleTeamId);
            UpdateFile(newAppFile, newMatchFile);
        }

        private static void UpdateFile(string newAppFile, string newMatchFile)
        {
            File.WriteAllText(AppFilePath, newAppFile);
            File.WriteAllText(MatchFilePath, newMatchFile);
        }

        private static (string newAppFile, string newMatchFile) GetNewFile(string iosBundleId, string appleTeamId)
        {
            var appFile = File.ReadAllText(AppFilePath);

            var newAppFile = new Regex(AppFileAppIdPattern).Replace(appFile, iosBundleId);
            newAppFile = new Regex(AppFileTeamIdPattern).Replace(newAppFile, appleTeamId);

            Debug.Log($"IOS: New AppFile:\n {newAppFile}");

            var matchFile = File.ReadAllText(MatchFilePath);

            var newMatchFile = new Regex(MatchFileAppIdentifierPattern).Replace(matchFile, iosBundleId);
            newMatchFile = new Regex(MatchFileGitBranchPatten).Replace(newMatchFile, iosBundleId);

            Debug.Log($"IOS: new Matchfile:\n {newMatchFile}");

            return (newAppFile, newMatchFile);
        }
    }
}