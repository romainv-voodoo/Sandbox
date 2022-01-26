using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class IosData
    {
        public string ProjectVersion { get; set; }
        public string IosVersionCode { get; set; }
    }

    public class ProjectVersionInformation
    {
        private string ProjectName { get; set; }
        public string AndroidBundleVersionCode { get; set; }
        public List<IosData> IosData { get; set; }

        public override string ToString()
        {
            var output = new StringBuilder("Project Version Information\n");
            output.Append($"Project Name: {ProjectName}\n");
            output.Append($"Android Bundle Code: {AndroidBundleVersionCode}\n");
            output.Append($"IOS Data\n");
            foreach (var iosData in IosData)
            {
                output.Append($"{iosData.ProjectVersion} -> {iosData.IosVersionCode}\n");
            }

            return output.ToString();
        }
    }

    public class BuildVersionsPanel : BuildPanelBase
    {
        private static bool _showPanel = false;
        private const string ProjectVersionFileName = "ProjectVersion.json";
        private static string _buildVersionPathKey = "BuildVersionPathKey";
        private static string _versionSettingFilePath;
        private static ProjectVersionInformation _projectVersionInformation;

        private static string _androidBundleVersion;
        private static List<IosData> _iosData;

        public static void Initialize()
        {
            var versionFilePath = EditorPrefs.GetString(_buildVersionPathKey);
            if (string.IsNullOrEmpty(versionFilePath) == false)
            {
                _versionSettingFilePath = versionFilePath;
            }
        }

        public static void ShowPanel()
        {
            _showPanel = EditorGUILayout.BeginFoldoutHeaderGroup(_showPanel, "Version Management");
            if (_showPanel)
            {
                EditorGUILayout.Space(20);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                GUILayout.Label($"Version Management", EditorStyles.largeLabel);
                EditorGUILayout.Space(5);
                GUILayout.Label($"Instruction\n\n" +
                                $"> Press Download Button to Download Current {ProjectVersionFileName} file for the project." +
                                $"\n" +
                                $"> Once the file is downloaded, import the file to modify." +
                                $"\n" +
                                $"> You can upload the json file after modification.", EditorStyles.whiteLargeLabel);
                EditorGUILayout.Space(10);
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (GUILayout.Button($"Download ${ProjectVersionFileName} file (From CircleCI Artifact)",
                    GUILayout.Height(30)))
                {
                    DownloadSettings();
                }

                if (GUILayout.Button($"Import {ProjectVersionFileName} file", GUILayout.Height(30)))
                {
                    _versionSettingFilePath =
                        EditorUtility.OpenFilePanel("Select VersionInformation.json file", "", "json");
                    EditorPrefs.SetString(_buildVersionPathKey, _versionSettingFilePath);
                    _projectVersionInformation = null;
                }

                GUILayout.EndHorizontal();
                EditorGUILayout.Space(10);

                if (_versionSettingFilePath != null && _projectVersionInformation == null)
                {
                    try
                    {
                        _projectVersionInformation =
                            JsonConvert.DeserializeObject<ProjectVersionInformation>(
                                File.ReadAllText(_versionSettingFilePath));
                        _androidBundleVersion = _projectVersionInformation.AndroidBundleVersionCode;
                        _iosData = new List<IosData>();
                        _projectVersionInformation.IosData.ForEach(v => _iosData.Add(v));
                    }
                    catch (Exception e)
                    {
                        GUILayout.Label(
                            "VersionInformation.json File appears to have incorrect format. Please download the file again.");
                    }
                }

                if (_projectVersionInformation != null)
                {
                    ShowTextField("Android Bundle VersionCode", ref _androidBundleVersion);
                    ShowIosVersionTextField();
                    UpdateProjectVersionInformation();

                    EditorGUILayout.Space(10);
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    if (GUILayout.Button("Add New Project Version + ", GUILayout.Height(30)))
                    {
                        _iosData.Add(new IosData() {ProjectVersion = "", IosVersionCode = ""});
                    }

                    if (GUILayout.Button("Remove last Project Version -", GUILayout.Height(30)))
                    {
                        if (_iosData.Count > 0)
                        {
                            _iosData.RemoveAt(_iosData.Count - 1);
                        }
                    }

                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);

                    if (GUILayout.Button("Save To file", GUILayout.Height(30)))
                    {
                        var path = EditorUtility.SaveFilePanel("Save Json File", "", ProjectVersionFileName, "json");
                        if (path.Length != 0)
                        {
                            File.WriteAllText(path, GetVersionString());
                            Debug.Log($"File saved at {path}");
                            _versionSettingFilePath = path;
                            EditorPrefs.SetString(_buildVersionPathKey, _versionSettingFilePath);
                        }
                    }

                    if (GUILayout.Button($"Upload {ProjectVersionFileName}", GUILayout.Height(30)))
                    {
                        UploadSettings();
                    }
                }

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DownloadSettings()
        {
            var versionSyncParameter = new BuildVersionsSyncParameter()
            {
                Branch = Branch,
                ParametersArray = new BuildVersionsSyncParameter.Parameters()
                {
                    SyncVersion = true,
                    S3Folder = CiSettings.S3BucketFolder,
                    UploadVersionFile = false,
                    DownloadVersionFile = true,
                    VersionFile = string.Empty
                }
            };

            SendBuildRequest(versionSyncParameter);
        }

        private static void UploadSettings()
        {
            var versionSyncParameter = new BuildVersionsSyncParameter()
            {
                Branch = Branch,
                ParametersArray = new BuildVersionsSyncParameter.Parameters()
                {
                    SyncVersion = true,
                    S3Folder = CiSettings.S3BucketFolder,
                    UploadVersionFile = true,
                    DownloadVersionFile = false,
                    VersionFile = GetVersionString()
                }
            };

            SendBuildRequest(versionSyncParameter);
        }

        private static void ShowIosVersionTextField()
        {
            for (var i = 0; i < _iosData.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Project Version:", EditorStyles.largeLabel);
                _iosData[i].ProjectVersion = GUILayout.TextField(_iosData[i].ProjectVersion);
                GUILayout.Label("Build Number:", EditorStyles.largeLabel);
                _iosData[i].IosVersionCode = GUILayout.TextField(_iosData[i].IosVersionCode);
                GUILayout.EndHorizontal();
            }
        }

        private static string GetVersionString()
        {
            UpdateProjectVersionInformation();
            var versionFile = JsonConvert.SerializeObject(_projectVersionInformation);
            Debug.Log($"{versionFile}");
            versionFile = versionFile.Replace("\"", "\\\"");
            Debug.Log($"{versionFile}");
            return versionFile;
        }

        private static void UpdateProjectVersionInformation()
        {
            _projectVersionInformation.AndroidBundleVersionCode = _androidBundleVersion;
            _projectVersionInformation.IosData.Clear();
            foreach (var iosData in _iosData)
            {
                _projectVersionInformation.IosData.Add(iosData);
            }
        }
    }
}