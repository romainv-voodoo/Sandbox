using System;
using UnityEditor;
using UnityEditor.CrashReporting;

namespace Voodoo.CI
{
    public class BuildAndroid : BuildPipelineBase
    {
        private const string KeyStorePassArgument = "KEYSTORE_PASS";
        private const string KeyAliasNameArgument = "KEY_ALIAS_NAME";
        private const string KeyAliasPassArgument = "KEY_ALIAS_PASS";

        private static AndroidDeploymentSettings _androidDeploymentSettings;

        private static void Build()
        {
            var arguments = CommandUtils.GetArguments();
            PrepareSettings(arguments);
            CrashReportingSettings.enabled = false;
            LogBeginning();
            PrepareAndroidSettings(arguments);

            ConsoleLogSingleLine(
                $"Building Android. Deployment Type: {_androidDeploymentSettings.DeploymentType}. Artifact Type: {_androidDeploymentSettings.ArtifactType} ");

            CreateArtifactName(_androidDeploymentSettings.ArtifactType);

            switch (_androidDeploymentSettings.DeploymentType)
            {
                case BuildPipelineDeploymentSettings.DebugDeployment:
                    BuildAndroidDebug();
                    break;
                case BuildPipelineDeploymentSettings.ReleaseDeployment:
                    BuildAndroidRelease();
                    break;
            }
        }

        [MenuItem("Voodoo/VoodooCI/Local Builds on this Machine/Android Debug-APK")]
        public static void BuildAndroidApk_Debug()
        {
            CreateArtifactName(AndroidDeploymentSettings.ApkArtifact, true);
            BuildAndroidDebug();
        }

        [MenuItem("Voodoo/VoodooCI/Local Builds on this Machine/Android Release-APK")]
        public static void BuildAndroidApkRelease()
        {
            CreateArtifactName(AndroidDeploymentSettings.ApkArtifact, true);
            BuildAndroidRelease();
        }

        [MenuItem("Voodoo/VoodooCI/Local Builds on this Machine/Android Debug-AAB")]
        public static void BuildAndroidAab_Debug()
        {
            CreateArtifactName(AndroidDeploymentSettings.AabArtifact, true);
            BuildAndroidDebug();
        }

        [MenuItem("Voodoo/VoodooCI/Local Builds on this Machine/Android Release-AAB")]
        public static void BuildAndroidAabRelease()
        {
            CreateArtifactName(AndroidDeploymentSettings.AabArtifact, true);
            BuildAndroidRelease();
        }

        private static void BuildAndroidDebug()
        {
            PrepareAndroidSettings();
            GenericBuild(FindEnabledEditorScenes(),
                ArtifactName,
                BuildTargetGroup.Android,
                BuildTarget.Android,
                BuildOptions.Development);
        }

        private static void BuildAndroidRelease()
        {
            PrepareAndroidSettings();
            GenericBuild(FindEnabledEditorScenes(),
                ArtifactName,
                BuildTargetGroup.Android,
                BuildTarget.Android);
        }

        private static void PrepareAndroidSettings()
        {
            PrepareSettings();
            if (CiSettings.CreateSymbolsZipFile)
            {
                EditorUserBuildSettings.androidCreateSymbolsZip = true;
            }
        }

        private static void PrepareAndroidSettings(string[] arguments)
        {
            if (TryGetAndroidSigning(out var signingInfo))
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = CiSettings.KeyStoreFileAddress;
                PlayerSettings.Android.keystorePass = signingInfo.KeyStorePass;
                PlayerSettings.Android.keyaliasName = signingInfo.KeyAliasName;
                PlayerSettings.Android.keyaliasPass = signingInfo.KeyAliasPass;
            }
            else
            {
                PlayerSettings.Android.useCustomKeystore = false;
            }

            _androidDeploymentSettings = new AndroidDeploymentSettings();

            CommandUtils.GetArgumentValue(arguments, DeploymentTypeArgument, out var deploymentType);

            _androidDeploymentSettings.DeploymentType = deploymentType;

            _androidDeploymentSettings.ArtifactType = AndroidDeploymentSettings.AabArtifact;

            AssignProjectVersion();
        }

        private static void AssignProjectVersion()
        {
            var bundleVersionCode = Convert.ToInt32(_projectVersionInformation.AndroidBundleVersionCode) + 1;
            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            _projectVersionInformation.AndroidBundleVersionCode = $"{bundleVersionCode}";
            UpdateProjectVersion();
        }

        private static bool TryGetAndroidSigning(out AndroidSigninInfo signingInfo)
        {
            signingInfo = new AndroidSigninInfo();
            var arguments = CommandUtils.GetArguments();

            if (CommandUtils.GetArgumentValue(arguments, KeyStorePassArgument, out var keyStorePassValue) == false ||
                CommandUtils.GetArgumentValue(arguments, KeyAliasNameArgument, out var keyAliasNameValue) == false ||
                CommandUtils.GetArgumentValue(arguments, KeyAliasPassArgument, out var keyAliasPassValue) == false)
            {
                ConsoleLogSingleLine(
                    $"Keystore was not used to make build due to invalid parameters. Check Keystore file, alias, pass in environment variables",
                    true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CiSettings.KeyStoreFileAddress))
            {
                ConsoleLogSingleLine(
                    $"Keystore was not used to make build due to invalid parameters. Key store file path must be set to File>VoodooCI>Open Settings",
                    true);
                return false;
            }

            signingInfo.KeyStorePass = keyStorePassValue;
            signingInfo.KeyAliasName = keyAliasNameValue;
            signingInfo.KeyAliasPass = keyAliasPassValue;

            return true;
        }

        private struct AndroidSigninInfo
        {
            public string KeyStorePass;
            public string KeyAliasName;
            public string KeyAliasPass;
        }
    }
}