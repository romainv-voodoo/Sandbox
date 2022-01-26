using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if VOODOO_CI_ADDRESSABLE
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Voodoo.CI
{
    public class BuildPipelineBase
    {
        private const string CircleSha1Argument = "CIRCLE_SHA1";
        protected const string DeploymentTypeArgument = "DEPLOYMENT_TYPE";
        private const string ProjectVersion = "PROJECT_VERSION";


        private static string _circleSha1 = "";
        protected static ProjectVersionInformation _projectVersionInformation;

        private static readonly string Eol = Environment.NewLine;

        protected static string ArtifactName = "";

        public static string ProductName
        {
            get
            {
                var productName = PlayerSettings.productName;
                productName = productName.Replace(@" ", "");
                productName = Regex.Replace(productName, "[^0-9a-zA-Z]+", "");
                return productName;
            }
        }

        protected static CiSettingsData CiSettings;

        protected static string[] FindEnabledEditorScenes() =>
            EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();


        [MenuItem("Voodoo/VoodooCI/Utilities/Check Version")]
        public static void CheckBuildVersion()
        {
            OutputBuildVersionToConsole();
        }

        [MenuItem("Voodoo/VoodooCI/Utilities/Find and Compress Symbol Files")]
        public static void CompressSymbolFiles()
        {
            Debug.Log(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#   Compressing Symbols   #{Eol}" +
                $"###########################{Eol}"
            );

            try
            {
                var symbolsZipFile = GetAndroidSymbolsFile();
                
                
                if (string.IsNullOrEmpty(symbolsZipFile))
                {
                    symbolsZipFile = GetAndroidSymbolsFile();
                }

                if (string.IsNullOrEmpty(symbolsZipFile))
                {
                    throw new FileNotFoundException("XX XX XX No Symbols.Zip file is found XX XX XX");
                }

                ConsoleLogSingleLine($"Found Symbol files at {symbolsZipFile}");

                var symbolName = $"{ProductName}-symbols.zip";
                AndroidSymbolShrinker.ShrinkSymbols(symbolsZipFile, symbolName);

                ConsoleLogSingleLine($"New symbol file saved at {symbolName}");
            }
            catch (InvalidOperationException e)
            {
                Debug.Log($"No Zip file is found. {e.StackTrace}");
                throw;
            }
        }

        private static string GetAndroidSymbolsFile()
        {
            var symbolsZipFiles = Directory.GetFiles(Environment.CurrentDirectory, $"{ProductName}*.zip");

            if (symbolsZipFiles.Length == 0)
            {
                var symbolFileLocation = Path.Combine(Environment.CurrentDirectory, "Builds", "Android");
                symbolsZipFiles = Directory.GetFiles(symbolFileLocation, $"{ProductName}*.zip");    
            }

            if (symbolsZipFiles.Length == 0)
            {
                throw new InvalidOperationException("There is no Android symbols file in the project");
            }
            
            return symbolsZipFiles[0];
        }


        protected static void PrepareSettings(string[] arguments = null)
        {
            CiSettings = CiSettingsWindow.GetSettings();

            if (CiSettings == null)
            {
                throw new NullReferenceException(
                    $"XX XX XX {nameof(CiSettings)} is missing. Is there any vcisettings.json file places at root ?? XX XX XX");
            }

            if (arguments == null) return;

            CommandUtils.GetArgumentValue(arguments, CircleSha1Argument, out var circleSha1);
            _circleSha1 = circleSha1;

            CommandUtils.GetArgumentValue(arguments, ProjectVersion, out var projectVersion);
            ConsoleLogSingleLine(projectVersion);
            _projectVersionInformation = JsonConvert.DeserializeObject<ProjectVersionInformation>(projectVersion);
            Debug.Log($"{_projectVersionInformation}");
        }


        private static void OutputBuildVersionToConsole()
        {
            var branch = GitUtils.Branch;
            var buildVersion = GitUtils.BuildVersion;

            ConsoleLogSingleLine($"Build version for branch <b>{branch}</b> is <b>{buildVersion}</b>");
        }


        private static void BuildAddressables()
        {
#if VOODOO_CI_ADDRESSABLE
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                ConsoleLogSingleLine("AddressableCan't be built", true);
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder;
            AddressableAssetSettings.CleanPlayerContent(settings);
            AddressableAssetSettings.BuildPlayerContent();
#endif
        }

        protected static void GenericBuild(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup,
            BuildTarget buildTarget, BuildOptions buildOptions = BuildOptions.None)
        {
            if (CiSettings.UseGitVersionNumber)
            {
                GitUtils.buildVersionIsGitLogBased = true;
                var buildVersion = GitUtils.BuildVersion;
                ConsoleLogSingleLine($"buildVersion (Version*): {buildVersion}");

                PlayerSettings.bundleVersion = buildVersion;
            }

            if (CiSettings.UseCircleCiSha1)
            {
                if (string.IsNullOrEmpty(_circleSha1))
                {
                    ConsoleLogSingleLine("Circle SHA1 is not set", true);
                }
                else
                {
                    _circleSha1 = _circleSha1.Substring(0, 7); // Like CicleCI shows in the dashboard
                    PlayerSettings.bundleVersion = $"{PlayerSettings.bundleVersion}-{_circleSha1}";
                }
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

            BuildAddressables();

            var res = BuildPipeline.BuildPlayer(scenes, targetDir, buildTarget, buildOptions);
            ReportSummary(res.summary);

            if (res.summary.result == BuildResult.Failed)
            {
                ConsoleLogSingleLine($"BUILD FAILED!!!!!", true);
                throw new Exception("BuildPlayer failure: " + res);
            }
            else
            {
                ConsoleLogSingleLine($"BUILD Artifact produced at the location: {targetDir}");
            }
        }

        private static void ReportSummary(BuildSummary summary)
        {
            Debug.Log(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Build results      #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Result: {summary.result.ToString()}{Eol}" +
                $"Duration: {summary.totalTime.ToString()}{Eol}" +
                $"Warnings: {summary.totalWarnings.ToString()}{Eol}" +
                $"Errors: {summary.totalErrors.ToString()}{Eol}" +
                $"Size: {summary.totalSize.ToString()} bytes{Eol}" +
                $"Artifact Name: {ArtifactName} {Eol}" +
                $"{Eol}"
            );
        }

        protected static void LogBeginning()
        {
            Debug.Log(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );
        }

        protected static void CreateArtifactName(string extension, bool isLocalBuild = false)
        {
            switch (extension)
            {
                case AndroidDeploymentSettings.AabArtifact:
                    EditorUserBuildSettings.buildAppBundle = true;
                    break;
                case AndroidDeploymentSettings.ApkArtifact:
                    EditorUserBuildSettings.buildAppBundle = false;
                    break;
            }

            ArtifactName = $"{ProductName}.{extension}";
            
            if (isLocalBuild)
            {
                var buildsDirectory = Path.Combine("Builds", "Android");
                
                if (Directory.Exists(buildsDirectory))
                {
                    Directory.Delete(buildsDirectory, true);
                }
                
                ArtifactName = Path.Combine(buildsDirectory, ArtifactName);
            }
            
            ConsoleLogSingleLine(ArtifactName);
        }

        protected static void ConsoleLogSingleLine(string message, bool isError = false)
        {
            if (isError)
            {
                Debug.LogError($"::X::X:: {message}  ::X::X::");
            }
            else
            {
                Debug.Log($":: :: :: {message}  :: :: ::");
            }
        }

        protected static void UpdateProjectVersion()
        {
            var projectVersionJson = JsonConvert.SerializeObject(_projectVersionInformation);

            ConsoleLogSingleLine(projectVersionJson);

            var projectVersionDirectory = "ProjectVersion";
            if (Directory.Exists(projectVersionDirectory) == false)
            {
                Directory.CreateDirectory(projectVersionDirectory);
            }

            using (var outputFile = new StreamWriter(Path.Combine(projectVersionDirectory, "ProjectVersion.json")))
            {
                outputFile.WriteLine(projectVersionJson);
            }

            ConsoleLogSingleLine("Updated Project version information");
            Debug.Log($"{_projectVersionInformation}");
        }
    }
}