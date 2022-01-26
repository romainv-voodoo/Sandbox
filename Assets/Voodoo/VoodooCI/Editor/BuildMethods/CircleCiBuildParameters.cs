using Newtonsoft.Json;

namespace Voodoo.CI
{
    public class CircleCiBuildParameters
    {
        [JsonProperty("branch")] public string Branch { get; set; }

        public string GetParametersAsJson()
        {
            return $"{JsonConvert.SerializeObject(this)}";
        }
    }

    public class BuildVersionsSyncParameter : CircleCiBuildParameters
    {
        [JsonProperty("parameters")] public Parameters ParametersArray { get; set; }

        public class Parameters
        {
            [JsonProperty("sync-version")] public bool SyncVersion { get; set; }
            [JsonProperty("s3-folder")] public string S3Folder { get; set; }

            [JsonProperty("download-version-file")]
            public bool DownloadVersionFile { get; set; }

            [JsonProperty("upload-version-file")] public bool UploadVersionFile { get; set; }
            [JsonProperty("version-file")] public string VersionFile { get; set; }
        }
    }

    public class CiPlatformParametersBase
    {
        [JsonProperty("upload-comment")] public string UploadComment { get; set; }

        [JsonProperty("unity-project-path")] public string UnityProjectPath { get; set; }

        [JsonProperty("s3-build-number-folder")]
        public string S3BuildNumberFolder { get; set; }

        [JsonProperty("deployment-type")] public string DeploymentType { get; set; }

        [JsonProperty("enable-test")] public bool EnableTest { get; set; }

        [JsonProperty("enable-editmode-test")] public bool EnableEditModeTest { get; set; }

        [JsonProperty("enable-playmode-test")] public bool EnablePlayModeTest { get; set; }

        [JsonProperty("enable-caching")] public bool EnableCaching { get; set; }

        [JsonProperty("project-version")] public string ProjectVersion { get; set; }

        [JsonProperty("artifact-name")] public string ArtifactName { get; set; }
    }


    public class IosParameters : CircleCiBuildParameters
    {
        [JsonProperty("parameters")] public Parameters ParametersArray { get; set; }

        public class Parameters : CiPlatformParametersBase
        {
            [JsonProperty("manual-trigger-ios")] public bool ManualTriggerIos { get; set; }

            [JsonProperty("manual-trigger-ios-testflight")]
            public bool ManualTriggerIosTestFlight { get; set; }

            [JsonProperty("game-ci-version-ios")] public string GameCiVersionIos { get; set; }

            [JsonProperty("apple-team-context")] public string AppleTeamContext { get; set; }
            [JsonProperty("apple-team-id")] public string AppleTeamId { get; set; }
        }
    }

    public class AndroidParameters : CircleCiBuildParameters
    {
        [JsonProperty("parameters")] public Parameters ParametersArray { get; set; }

        public class Parameters : CiPlatformParametersBase
        {
            [JsonProperty("manual-trigger-android-deployment")]
            public bool ManualTriggerAndroidDeployment { get; set; }

            [JsonProperty("game-ci-version-android")]
            public string GameCiVersionAndroid { get; set; }

            [JsonProperty("android-increment-bundle")]
            public bool AndroidIncrementBundle { get; set; }

            [JsonProperty("android-keystore")] public string AndroidKeystore { get; set; }
        }
    }
}