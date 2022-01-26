using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class PackageManagementPanel : BuildPanelBase
    {
        private static bool _showPanel;
        private static string _nextPackageVersion;

        private const string CiVersionFile = "Assets/Voodoo/VoodooCI/Resources/VoodooCiVersion.asset";
        private static VoodooCiPackageVersion _ciVersion;

        private static readonly string RootPath = Path.Combine(Application.dataPath);

        private static readonly string VoodooCiFolderName = "VoodooCI";
        private static readonly string CircleCiConfigFolderName = "circleci";

        private static readonly string RootCircleCiConfigPath =
            Path.Combine($"{RootPath}", "../", $".{CircleCiConfigFolderName}");

        private static readonly string RootVoodooCiFolder = Path.Combine($"{RootPath}", "../", VoodooCiFolderName);

        private static readonly string AssetsVoodooCiFolderPath =
            Path.Combine(Application.dataPath, "Voodoo", "VoodooCI");

        private static readonly string AssetsRoot = Path.Combine(Application.dataPath, "Voodoo", "VoodooCI", "root");
        private static readonly string AssetsVoodooCiPath = Path.Combine(AssetsRoot, VoodooCiFolderName);
        private static readonly string AssetsCircleCiPath = Path.Combine(AssetsRoot, CircleCiConfigFolderName);


        public static void Initialize()
        {
            _ciVersion = AssetDatabase.LoadAssetAtPath<VoodooCiPackageVersion>(CiVersionFile);
            _nextPackageVersion = _ciVersion.NextPackageVersion;
        }

        public static void ShowPanel()
        {
            _showPanel = EditorGUILayout.BeginFoldoutHeaderGroup(_showPanel, "Package Management");
            if (_showPanel)
            {
                EditorGUILayout.Space(20);
                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Label($"Current Version: {_ciVersion.CurrentVersion}", EditorStyles.largeLabel);

                GUILayout.Space(5);
                ShowTextField("Next version number: ", ref _nextPackageVersion);
                GUILayout.Space(5);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Instruction to setup VoodooCI for a project\n", EditorStyles.boldLabel);

                GUILayout.Label($"After downloading the package from Voodoo Store:\n" +
                                $"> Press 'Setup project from package' button.\n" +
                                $"> Then Press 'Clear Package Folder' button.\n" +
                                $"> Then Press 'Refresh Assets Database' button.\n" +
                                $"> Ensure CiSettings file has all data.", EditorStyles.whiteLabel);
                GUILayout.Space(5);
                ShowSetupProjectFromPackageButton();
                GUILayout.Space(5);
                ShowClearPackageFolderButton();
                GUILayout.Space(5);
                ShowRefreshAssetsDatabaseButton();
                GUILayout.Space(5);
                GUILayout.EndVertical();
                GUILayout.Space(5);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Instruction to export VoodooCI as package\n", EditorStyles.boldLabel);

                GUILayout.Label($"To prepare VoodooCI to export as a package:\n" +
                                $"> Press 'Create VoodooCI Package Folder' button.\n" +
                                $"> Then drag and drop 'Assets/Voodoo/VoodooCI' folder to VoodooStore.\n" +
                                $"> Make sure that version is correct. Current Version is {_ciVersion}.\n" +
                                $"> After package is exported, press 'Clear Package folder button'." +
                                $"> Press 'Assets Database Refresh' button.", EditorStyles.whiteLabel);
                GUILayout.Space(5);
                ShowMakePackageFolderButton();
                GUILayout.Space(5);
                ShowClearPackageFolderButton();
                GUILayout.Space(5);
                ShowRefreshAssetsDatabaseButton();
                GUILayout.Space(5);
                GUILayout.EndVertical();


                GUILayout.Space(20);
                ShowRemoveVoodooCiButton();

                GUILayout.EndVertical();
                EditorGUILayout.Space(20);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void ShowRemoveVoodooCiButton()
        {
            if (GUILayout.Button("Remove VoodooCI From Project", GUILayout.Height(30)))
            {
                RefreshSettings();
                ShowConfirmationPopup("Remove VoodooCI",
                    $"Do you Want to remove VoodooCI version {_ciVersion} from Project?" +
                    $"\n\n" +
                    $"Your VoodooCI settings will be stored at project root as VoodooCISettings.json file.\n" +
                    $"Use this file to import new Version of VoodooCI",
                    () =>
                    {
                        RemovePackageFolderFromAssets();
                        DeleteCiFoldersAtRoot();
                        DeleteVoodooCiFromAssets();

                        Debug.Log(
                            $"All Files related to CI version {_ciVersion}  is deleted. Close and Open Unity. Consider a git commit afterwards before updating new version.");
                    });
            }
        }

        private static void ShowMakePackageFolderButton()
        {
            if (GUILayout.Button("Create Package folder at Assets/Voodoo/VoodooCI", GUILayout.Height(30)))
            {
                ShowConfirmationPopup("Create Package folder", $"Export VoodooCI version {_nextPackageVersion}",
                    () =>
                    {
                        if (Directory.Exists(AssetsRoot))
                        {
                            RemovePackageFolderFromAssets();
                        }

                        Directory.CreateDirectory(AssetsRoot);
                        Directory.CreateDirectory(AssetsCircleCiPath);
                        Directory.CreateDirectory(AssetsVoodooCiPath);

                        CopyAll(RootCircleCiConfigPath, AssetsCircleCiPath);
                        CopyAll(RootVoodooCiFolder, AssetsVoodooCiPath);

                        CleanTools();

                        _ciVersion.UpdateVersion(_nextPackageVersion);
                        ShowConfirmationPopup("Package Exported Successfully",
                            $"Press Refresh Asset Database button.\n" +
                            $"Copy VoodooCI Folder and export to VoodooStore with the Current Version {_ciVersion}.",
                            () =>
                            {
                                Debug.Log(
                                    $"Package Exported Successfully. Copy VoodooCI Folder and export to VoodooStore with the Current Version {_ciVersion}.");
                            });
                    });
            }
        }

        private static void CleanTools()
        {
            var unitTestTool = Path.Combine(AssetsVoodooCiPath, "common", "unit_tests", "TestReportGenerator");
            Directory.Delete(unitTestTool, true);
            var versionUpdateTool = Path.Combine(AssetsVoodooCiPath, "common", "version_update", "VersionUpdateTool");
            Directory.Delete(versionUpdateTool, true);
        }


        private static void ShowRefreshAssetsDatabaseButton()
        {
            if (GUILayout.Button("Refresh Asset database", GUILayout.Height(30)))
            {
                AssetDatabase.Refresh();
            }
        }

        private static void ShowClearPackageFolderButton()
        {
            if (GUILayout.Button("Clear Package Folder", GUILayout.Height(30)))
            {
                ShowConfirmationPopup("Clear package folder",
                    $"Voodoo/VoodooCI/root folder will be removed.",
                    () =>
                    {
                        if (Directory.Exists(AssetsRoot))
                        {
                            RemovePackageFolderFromAssets();
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            Debug.Log($"{AssetsRoot} folder is empty.");
                        }
                    });
            }
        }

        private static void ShowSetupProjectFromPackageButton()
        {
            if (GUILayout.Button("Set up project from package", GUILayout.Height(30)))
            {
                if (Directory.Exists(AssetsRoot) == false)
                {
                    Debug.LogError($"The package is not ready. Create package folder first.");
                }
                else
                {
                    ShowConfirmationPopup("Set up CI from package",
                        $".circleci and VoodooCI folder will be created at root",
                        () =>
                        {
                            DeleteCiFoldersAtRoot();

                            CopyAll(AssetsCircleCiPath, RootCircleCiConfigPath);
                            CopyAll(AssetsVoodooCiPath, RootVoodooCiFolder);
                            Debug.Log(
                                $"Project is updated with VoodooCI version {_ciVersion}. Press 'Clear Package Folder button'.");
                        });
                }
            }
        }

        private static void DeleteVoodooCiFromAssets()
        {
            if (Directory.Exists(AssetsVoodooCiFolderPath))
            {
                Directory.Delete(AssetsVoodooCiFolderPath, true);
            }
        }


        private static void DeleteCiFoldersAtRoot()
        {
            if (Directory.Exists(RootVoodooCiFolder))
            {
                Directory.Delete(RootVoodooCiFolder, true);
            }

            if (Directory.Exists(RootCircleCiConfigPath))
            {
                Directory.Delete(RootCircleCiConfigPath, true);
            }
        }

        private static void RemovePackageFolderFromAssets()
        {
            if (!Directory.Exists(AssetsRoot)) return;

            Directory.Delete(AssetsRoot, true);
            File.Delete($"{AssetsRoot}.meta");
        }

        private static void CopyAll(string sourcePath, string targetPath)
        {
            var source = new DirectoryInfo(sourcePath);
            var target = new DirectoryInfo(targetPath);
            CopyAll(source, target);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var fi in source.GetFiles())
            {
                if (fi.Extension == ".meta") continue;

                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}