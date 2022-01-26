namespace Voodoo.CI
{
    public class BuildPipelineDeploymentSettings
    {
        public const string ReleaseDeployment = "release";
        public const string DebugDeployment = "debug";
        public string DeploymentType { get; set; }
    }

    public class AndroidDeploymentSettings : BuildPipelineDeploymentSettings
    {
        public const string ApkArtifact = "apk";
        public const string AabArtifact = "aab";

        public string ArtifactType { get; set; }
    }
}