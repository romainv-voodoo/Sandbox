using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class AndroidBuildTriggerPanel : BuildPanelBase
    {
        private static bool _showPanel;
        private static bool _debug;
        private static bool _incrementBundleVersion;

        public static void ShowPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(10);
            _showPanel = EditorGUILayout.BeginFoldoutHeaderGroup(_showPanel, "Android Builds");
            if (_showPanel)
            {
                GUILayout.Label("Android Builds - Debug", EditorStyles.largeLabel);

                EditorGUILayout.Space(10);
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.BeginVertical();
                _debug = EditorGUILayout.Toggle("Debug Build", _debug);
                _incrementBundleVersion = EditorGUILayout.Toggle("Increment Bundle Version", _incrementBundleVersion);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();


                if (GUILayout.Button("Make Android Build", GUILayout.Height(40)))
                {
                    if (_debug)
                    {
                        AndroidBuildButtonPressed(BuildPipelineDeploymentSettings.DebugDeployment,
                            _incrementBundleVersion);
                    }
                    else
                    {
                        AndroidBuildButtonPressed(BuildPipelineDeploymentSettings.ReleaseDeployment,
                            _incrementBundleVersion);
                    }
                }

                GUILayout.EndHorizontal();
                EditorGUILayout.Space(20);
            }


            EditorGUILayout.Space(10);
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
        }

        private static void AndroidBuildButtonPressed(string deploymentType, bool incrementBundle)
        {
            if (IsBuildApproved()) return;

            if (RefreshSettings())
            {
                ConfirmAndroidBuild(StartBuildAndroid, deploymentType, incrementBundle);
            }
        }

        private static void ConfirmAndroidBuild(Action<string, bool> action, string deploymentType,
            bool incrementBundle)
        {
            if (ShowConfirmationPopup())
            {
                action.Invoke(deploymentType, incrementBundle);
            }
        }

        private static bool ShowConfirmationPopup()
        {
            var message = new StringBuilder();
            message.AppendLine($"Project Name: {CircleProjectName}").AppendLine();
            message.AppendLine($"Git branch: {Branch}").AppendLine();
            if (_debug)
            {
                message.AppendLine("This is a 'DEBUG' build.").AppendLine();
            }
            else
            {
                message.AppendLine("This is a 'Release' build").AppendLine();
            }

            if (CiSettings.UseGitVersionNumber)
            {
                message.AppendLine("Git version in CiSettings is enabled");
                message.AppendLine($"Project version is set as: {GitUtils.BuildVersion}").AppendLine();
            }

            if (_incrementBundleVersion)
            {
                message.AppendLine("This build will increment 'Bundle Version Code'");
                message.AppendLine(
                    "To check the current Bundle Version Code use 'Advanced Settings' in CircleCI Editor tool");
            }

            return EditorUtility.DisplayDialog(
                "Please verify the options and press 'Yes' to make a build ",
                $"{message}"
                , "Yes",
                "Cancel");
        }

        private static void StartBuildAndroid(string deploymentType, bool incrementBundle)
        {
            if (CiSettings.UseGitVersionNumber)
            {
                PlayerSettings.bundleVersion = GitUtils.BuildVersion;
            }

            var shortHash = GitUtils.GetShortHash();

            var cciBuildParameters = new AndroidParameters()
            {
                Branch = Branch,
                ParametersArray = new AndroidParameters.Parameters()
                {
                    DeploymentType = deploymentType,
                    ManualTriggerAndroidDeployment = true,
                    UploadComment = $"[{Branch} - {shortHash} - {deploymentType}] {Comment}",
                    GameCiVersionAndroid = CiSettings.GameCiDockerTagAndroid,
                    ArtifactName = BuildPipelineBase.ProductName,
                    S3BuildNumberFolder = CiSettings.S3BucketFolder,
                    UnityProjectPath = CiSettings.UnityProjectPath,
                    AndroidIncrementBundle = incrementBundle,
                    AndroidKeystore = CiSettings.KeyStoreFileAddress,
                    ProjectVersion = PlayerSettings.bundleVersion,
                    EnableTest = CiSettings.EnableTestRunner,
                    EnableEditModeTest = CiSettings.EnableEditModeTest,
                    EnablePlayModeTest = CiSettings.EnablePlayModeTest,
                    EnableCaching = CiSettings.EnableCaching
                }
            };

            Debug.Log($"{cciBuildParameters.GetParametersAsJson()}");
            SendBuildRequest(cciBuildParameters);
        }
    }
}