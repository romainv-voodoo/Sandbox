using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class IosBuildTriggerPanel : BuildPanelBase
    {
        private static bool _showPanel = false;

        private static bool _debug = false;
        private static bool _enableTestFlight = false;

        private static void IosBuildButtonPressed(string deploymentType, bool enableDeployment)
        {
            if (IsBuildApproved()) return;

            if (RefreshSettings())
            {
                ConfirmIosBuild(StartIosBuild, deploymentType, enableDeployment);
            }
        }

        private static void ConfirmIosBuild(Action<string, bool> action, string deploymentType,
            bool enableDeployment)
        {
            if (ShowConfirmationPopup())
            {
                action.Invoke(deploymentType, enableDeployment);
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

            if (_enableTestFlight)
            {
                message.AppendLine("Testflight is enabled. The build will be pushed to Testflight").AppendLine();
            }
            else
            {
                message.AppendLine("Test flight is disabled. The build will NOT be pushed to Testflight").AppendLine();
            }

            message.AppendLine(
                "This build will increment 'Build Version' number by 1 if the current version is already published.");
            message.AppendLine(
                "To check the current Bundle Version Code use 'Advanced Settings' in CircleCI Editor tool");

            return EditorUtility.DisplayDialog(
                "Please verify the options and press 'Yes' to make a build ",
                $"{message}"
                , "Yes",
                "Cancel");
        }

        private static void StartIosBuild(string deploymentType, bool enableDeployment)
        {
            if (CiSettings.UseGitVersionNumber)
            {
                PlayerSettings.bundleVersion = GitUtils.BuildVersion;
            }

            var shortHash = GitUtils.GetShortHash();

            var cciBuildParameters = new IosParameters()
            {
                Branch = Branch,
                ParametersArray = new IosParameters.Parameters()
                {
                    ManualTriggerIos = true,
                    ManualTriggerIosTestFlight = enableDeployment,
                    UploadComment = $"[{Branch} - {shortHash} - {deploymentType}] {Comment}",
                    GameCiVersionIos = CiSettings.GameCiDockerTagIos,
                    ArtifactName = BuildPipelineBase.ProductName,
                    DeploymentType = deploymentType,
                    UnityProjectPath = CiSettings.UnityProjectPath,
                    ProjectVersion = PlayerSettings.bundleVersion,
                    S3BuildNumberFolder = CiSettings.S3BucketFolder,
                    AppleTeamContext = CiSettings.AppleTeamContext,
                    AppleTeamId = CiSettings.AppleTeamId,
                    EnableTest = CiSettings.EnableTestRunner,
                    EnableEditModeTest = CiSettings.EnableEditModeTest,
                    EnablePlayModeTest = CiSettings.EnablePlayModeTest,
                    EnableCaching = CiSettings.EnableCaching
                }
            };

            Debug.Log($"{cciBuildParameters.GetParametersAsJson()}");
            SendBuildRequest(cciBuildParameters);
        }

        public static void ShowPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(10);
            _showPanel = EditorGUILayout.BeginFoldoutHeaderGroup(_showPanel, "iOS Builds");

            if (_showPanel)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.BeginVertical();
                _debug = EditorGUILayout.Toggle("Debug Build", _debug);
                _enableTestFlight = EditorGUILayout.Toggle("Push Builds to Test flight", _enableTestFlight);
                GUILayout.EndVertical();

                if (GUILayout.Button("Make iOS Build", GUILayout.Height(40)))
                {
                    if (_debug)
                    {
                        IosBuildButtonPressed(BuildPipelineDeploymentSettings.DebugDeployment, _enableTestFlight);
                    }
                    else
                    {
                        IosBuildButtonPressed(BuildPipelineDeploymentSettings.ReleaseDeployment, _enableTestFlight);
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.EndVertical();
        }
    }
}