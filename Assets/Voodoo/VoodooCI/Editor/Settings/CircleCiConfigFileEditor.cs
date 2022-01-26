using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class CircleCiConfigFileEditor
    {
        private static string _configFilePath = Path.Combine(Application.dataPath, "../", ".circleci", "config.yml");

        public static bool TryUpdateConfigFile(CiSettingsData ciSettingsData)
        {
            var configFileLines = File.ReadAllLines(_configFilePath).ToList();

            if (File.Exists(_configFilePath))
            {
                ReplaceDefaultValues(configFileLines, "upload-comment", "[Auto Build on main branch]");
                ReplaceDefaultValues(configFileLines, "unity-project-path", ciSettingsData.UnityProjectPath);
                ReplaceDefaultValues(configFileLines, "s3-build-number-folder", ciSettingsData.S3BucketFolder);
                ReplaceDefaultValues(configFileLines, "enable-test", ciSettingsData.EnableTestRunner);
                ReplaceDefaultValues(configFileLines, "enable-editmode-test", ciSettingsData.EnableEditModeTest);
                ReplaceDefaultValues(configFileLines, "enable-playmode-test", ciSettingsData.EnablePlayModeTest);
                ReplaceDefaultValues(configFileLines, "project-version", PlayerSettings.bundleVersion);
                ReplaceDefaultValues(configFileLines, "artifact-name", BuildPipelineBase.ProductName);
                ReplaceDefaultValues(configFileLines, "enable-caching", ciSettingsData.EnableCaching);


                ReplaceDefaultValues(configFileLines, "game-ci-version-android", ciSettingsData.GameCiDockerTagAndroid);
                ReplaceDefaultValues(configFileLines, "android-keystore", ciSettingsData.KeyStoreFileAddress);

                ReplaceDefaultValues(configFileLines, "game-ci-version-ios", ciSettingsData.GameCiDockerTagIos);
                ReplaceDefaultValues(configFileLines, "apple-team-context", ciSettingsData.AppleTeamContext);
                ReplaceDefaultValues(configFileLines, "apple-team-id", ciSettingsData.AppleTeamId);

                File.WriteAllLines(_configFilePath, configFileLines);
                Debug.Log(
                    $"Circle CI config file updated successfully. commit and push config.yml file to see it in action!");
            }
            else
            {
                Debug.Log($"File not found");
                return false;
            }

            return true;
        }

        private static void ReplaceDefaultValues(List<string> configFileLines, string parameterName,
            bool parameterValue)
        {
            ReplaceDefaultValues(configFileLines, parameterName, parameterValue.ToString().ToLower());
        }

        private static void ReplaceDefaultValues(List<string> configFileLines, string parameterName,
            string parameterValue)
        {
            for (var i = 0; i < configFileLines.Count; i++)
            {
                if (configFileLines[i].Contains($"{parameterName}:"))
                {
                    var type = configFileLines[i + 1];
                    var defaultValue = configFileLines[i + 2];

                    if (type.Contains("string"))
                    {
                        var regExpr = new Regex("\".*\"", RegexOptions.IgnoreCase);
                        var result = regExpr.Replace(defaultValue, $"\"{parameterValue}\"");
                        configFileLines[i + 2] = result;
                    }

                    else if (type.Contains("boolean"))
                    {
                        var pattern = @"\b(true|false)\b";
                        var regExpr = new Regex(pattern, RegexOptions.IgnoreCase);
                        var result = regExpr.Replace(defaultValue, parameterValue.ToLower());
                        configFileLines[i + 2] = result;
                    }
                }
            }
        }
    }
}