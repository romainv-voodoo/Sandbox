using System;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Voodoo.CI
{
    public class BuildPanelBase
    {
        protected static string CircleProjectName;
        protected static string Branch;
        protected static CiSettingsData CiSettings;
        protected static string Comment = "";
        private static string _lastResponse = "";

        public static bool RefreshSettings()
        {
            try
            {
                var output = GitUtils.GetRepoName();

                var branchName = GitUtils.GetBranchName();
                Debug.Log($"{output}");
                Debug.Log($"{branchName}");
                Branch = GitUtils.GetBranchName();
                CircleProjectName = GitUtils.GetRepoName();
                Debug.Log($"Information from Git Repo updated successfully!");

                CiSettings = CiSettingsWindow.GetSettings();
                var settingsError = CiSettings.GetSettingsError();
                if (string.IsNullOrEmpty(settingsError) == false)
                {
                    throw new ArgumentNullException(
                        $"${nameof(CiSettings)} has empty values. Check log for details.\n{settingsError}");
                }

                CircleCiApiTokenPanel.Initialize();
                BuildVersionsPanel.Initialize();
                PackageManagementPanel.Initialize();
                FastlaneConfigPanel.Initialize();
                Debug.Log($"Product name: {BuildPipelineBase.ProductName}");

                return true;
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case ArgumentException _:
                        ShowDisplayDialog(e, "CiSettings file has missing field!",
                            "Ci Settings file is missing some information. Check the console for details.");
                        break;
                    case GitException _:
                        ShowDisplayDialog(e, "Git Exception!",
                            "Possible Reason: GIT CLI not installed or the branch is not tagged");
                        break;
                    default:
                        ShowDisplayDialog(e, "Exception Occured!", "Check console for details");
                        break;
                }
            }

            void ShowDisplayDialog(Exception e, string heading, string message)
            {
                if (EditorUtility.DisplayDialog(heading, message, "Ok"))
                {
                    Debug.LogError($"{e}");
                }
            }

            return false;
        }

        protected static bool IsBuildApproved()
        {
            var isCommentEmpty = IsCommentEmpty();
            var arePendingChanges = AreTherePendingGitChanges();
            return isCommentEmpty && arePendingChanges;
        }

        private static bool AreTherePendingGitChanges()
        {
            var gitStatus = GitUtils.GetGitStatus();
            
            Debug.Log($"Git status: = {gitStatus}");

            if (gitStatus.Contains("modified") || gitStatus.Contains("deleted") || gitStatus.Contains("added"))
            {
                if (EditorUtility.DisplayDialog("You have pending changes in git",
                    $"Do you want to continue anyway?" +
                    $"\n" +
                    "Check the status in log", "Yes", "Cancel"))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsCommentEmpty()
        {
            if (string.IsNullOrEmpty(Comment) == false) return false;

            if (EditorUtility.DisplayDialog("Comment Missing!", "Do you want to continue anyway?", "Yes", "Cancel"))
            {
                return false;
            }

            return true;
        }

        public static void GoToCircleCiPage()
        {
            Application.OpenURL($"https://app.circleci.com/pipelines/github/VoodooTeam/{CircleProjectName}?branch=" +
                                WebUtility.UrlEncode(Branch));
        }

        protected static void SendBuildRequest(CircleCiBuildParameters cciBuildParameters)
        {
            var www = UnityWebRequest.Post(
                $"https://circleci.com/api/v2/project/github/VoodooTeam/{CircleProjectName}/pipeline", "");

            byte[] bytes = Encoding.UTF8.GetBytes(cciBuildParameters.GetParametersAsJson());
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            www.uploadHandler = uH;

            www.SetRequestHeader("Circle-Token", CircleCiApiTokenPanel.CircleApiToken);
            www.SetRequestHeader("Content-Type", "application/json");

            var res = www.SendWebRequest();
            _lastResponse = "Waiting for server response";
            res.completed += operation =>
            {
                if (www.isNetworkError)
                {
                    Debug.LogError($"there was an error!!!");
                    Debug.Log(www.error);
                }

                else
                {
                    Debug.Log(www.downloadHandler.text);
                    if (www.responseCode >= 200 && www.responseCode < 400)
                    {
                        _lastResponse = "SUCCESS\n";
                        GoToCircleCiPage();
                    }
                    else
                    {
                        _lastResponse = "ERROR\n";
                        Debug.Log($"Error Code: {www.responseCode}");
                    }
                }
            };
        }

        protected static void ShowTextField(string title, ref string textFieldString, int width = -1)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, EditorStyles.largeLabel);
            textFieldString = width != -1
                ? GUILayout.TextField(textFieldString, GUILayout.Width(width))
                : GUILayout.TextField(textFieldString);
            GUILayout.EndHorizontal();
        }

        protected static void ShowConfirmationPopup(string title, string message, Action action)
        {
            if (EditorUtility.DisplayDialog(title, $"{message}", "Yes", "Cancel"))
            {
                action.Invoke();
            }
        }
    }
}