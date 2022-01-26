using System;
using System.Text;

namespace Voodoo.CI
{
    [Serializable]
    public class CiSettingsData
    {
        // Docker tags
        public string GameCiDockerTagAndroid;
        public string GameCiDockerTagIos;

        // Fastlane
        public string AndroidBundleId;
        public string IosBundleId;
        public string AppleTeamId;
        public string AppleTeamContext;

        // S3 bucket folder
        public string S3BucketFolder;

        // Use git version
        public bool UseGitVersionNumber;

        // Use CircleCI Sha1
        public bool UseCircleCiSha1;

        // Use Automated Bundle Version Number
        public bool UseAutomatedBundleVersionNumber;

        // Use Symbol zip file
        public bool CreateSymbolsZipFile;

        // Keystore 
        public bool UseKeyStore;
        public string KeyStoreFileAddress;

        // Extra project path of Unity Project
        public bool UseExtraUnityProjectPath;
        private string _unityProjectPath = "";

        // Test runner
        public bool EnableTestRunner;
        public bool EnableEditModeTest;
        public bool EnablePlayModeTest;

        // CI Caching
        public bool EnableCaching;

        // Auto Build Parameters
        public bool EnableAutoBuild;

        public string UnityProjectPath
        {
            get
            {
                if (UseExtraUnityProjectPath == false)
                {
                    return "./";
                }

                var projectPath = _unityProjectPath;
                if (projectPath.StartsWith("./") == false)
                {
                    projectPath = $"./{_unityProjectPath}";
                }

                if (projectPath.EndsWith("/") == false)
                {
                    projectPath = $"{projectPath}/";
                }

                return projectPath;
            }
            set => _unityProjectPath = value;
        }

        public string GetSettingsError()
        {
            var errorMessages = new StringBuilder();
            if (string.IsNullOrWhiteSpace(GameCiDockerTagAndroid))
            {
                errorMessages.Append(
                    $"Docker Tag for Android Is missing! Add a proper Docker Tag from Vengadores team to this field: .{nameof(GameCiDockerTagAndroid)}. \n");
            }
            else if (string.IsNullOrWhiteSpace(GameCiDockerTagIos))
            {
                errorMessages.Append(
                    $"Docker Tag for IOS Is missing! Add a proper Docker Tag from Vengadores team to this field .{nameof(GameCiDockerTagIos)}.\n");
            }
            else if (string.IsNullOrWhiteSpace(S3BucketFolder))
            {
                errorMessages.Append(
                    $"Bundle Version Code is stored in S3 bucket. Please contact Vengadores team to get the bucket name for the project and add the name here .{nameof(S3BucketFolder)}.\n");
            }
            else if (string.IsNullOrWhiteSpace(KeyStoreFileAddress))
            {
                errorMessages.Append(
                    $"Keystore file's address is missing. Please add the link of the file in here at .{nameof(KeyStoreFileAddress)}.\n");
            }
            else if (UseExtraUnityProjectPath && string.IsNullOrWhiteSpace(UnityProjectPath))
            {
                errorMessages.Append(
                    $"Project is not placed in the root of the git repo. Please mention the name of the folder in which Unity is placed in .{nameof(_unityProjectPath)} field.\n");
            }

            return errorMessages.ToString();
        }
    }
}