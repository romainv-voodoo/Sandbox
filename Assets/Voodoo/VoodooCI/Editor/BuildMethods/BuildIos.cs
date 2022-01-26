using System;
using System.IO;
using UnityEditor;

namespace Voodoo.CI
{
    public class BuildIos : BuildPipelineBase
    {
        private const string ProvisioningProfileArgument = "PROVISIONING_PROFILE";
        private const string AppleTeamArgument = "APPLE_TEAM";

        private static void PrepareIosSettings(string[] args = null)
        {
            PrepareSettings(args);

            if (args == null) return;

            CommandUtils.GetArgumentValue(args, ProvisioningProfileArgument, out var iOsManualProvisioningProfileId);
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileID = iOsManualProvisioningProfileId;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;

            CommandUtils.GetArgumentValue(args, AppleTeamArgument, out var appleTeam);
            PlayerSettings.iOS.appleDeveloperTeamID = appleTeam;

            AssignProjectVersion();
        }

        private static void AssignProjectVersion()
        {
            var projectVersionExists = false;

            foreach (var iosData in _projectVersionInformation.IosData)
            {
                if (iosData.ProjectVersion == PlayerSettings.bundleVersion)
                {
                    int iosVersionCode = Convert.ToInt32(iosData.IosVersionCode);
                    iosData.IosVersionCode = $"{iosVersionCode + 1}";

                    PlayerSettings.iOS.buildNumber = iosData.IosVersionCode;
                    projectVersionExists = true;
                    break;
                }
            }

            if (projectVersionExists == false)
            {
                _projectVersionInformation.IosData.Add(new IosData()
                {
                    ProjectVersion = PlayerSettings.bundleVersion,
                    IosVersionCode = "1"
                });
                PlayerSettings.iOS.buildNumber = "1";
            }

            ConsoleLogSingleLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            ConsoleLogSingleLine($"Project Version: {PlayerSettings.bundleVersion}");
            ConsoleLogSingleLine($"Build Number: {PlayerSettings.iOS.buildNumber}");
            ConsoleLogSingleLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            UpdateProjectVersion();
        }

        [MenuItem("Voodoo/VoodooCI/Local Builds on this Machine/iOS Debug")]
        public static void BuildIosDebug()
        {
            var targetDir = Path.Combine("Builds", "ios");
            Directory.CreateDirectory(targetDir);

            PrepareIosSettings();
            GenericBuild(FindEnabledEditorScenes(), targetDir, BuildTargetGroup.iOS, BuildTarget.iOS,
                BuildOptions.Development);
        }

        [MenuItem("Voodoo/VoodooCI/Local Builds on this Machine/iOS Release")]
        public static void BuildIosRelease()
        {
            var targetDir = Path.Combine("Builds", "ios");
            Directory.CreateDirectory(targetDir);

            PrepareIosSettings();
            GenericBuild(FindEnabledEditorScenes(), targetDir, BuildTargetGroup.iOS, BuildTarget.iOS);
        }

        public static void Build()
        {
            var arguments = CommandUtils.GetArguments();
            PrepareIosSettings(arguments);

            CommandUtils.GetArgumentValue(arguments, DeploymentTypeArgument, out var deploymentType);
            switch (deploymentType)
            {
                case BuildPipelineDeploymentSettings.DebugDeployment:
                    BuildIosDebug();
                    break;
                case BuildPipelineDeploymentSettings.ReleaseDeployment:
                    BuildIosRelease();
                    break;
            }

            PlayerSettings.iOS.iOSManualProvisioningProfileID = deploymentType;
        }
    }
}