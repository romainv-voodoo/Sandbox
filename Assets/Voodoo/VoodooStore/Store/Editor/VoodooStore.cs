using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Voodoo.Store
{ 
    public static class VoodooStore
    {
        public static List<Package>                       packages  = new List<Package>();
        public static List<string>                        labels    = new List<string>();
        public static PackagePreset                       cart      = PackagePreset.cart;
        public static PackagePreset                       favorites = PackagePreset.favorites;
        public static List<PackagePreset>                 presets   = new List<PackagePreset>();
        public static ObservableCollection<IDownloadable> selection = new ObservableCollection<IDownloadable>();
        public static int                                 filters;

        public static Package _info;
        public static Package Info
        {
            get
            {
                if (_info == null)
                {
                    _info = GetPackageByName(PathHelper.VST_NAME);
                }

                return _info;
            }
        }

        public static Package GetPackageByPath(string path)
        {
            string[] splitPath = path.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitPath.Length < 2)
            {
                return null;
            }

            Package package = GetOrCreatePackage(splitPath[splitPath.Length - 2]);
            
            return package;
        }

        public static Package GetOrCreatePackage(string name)
        {
            Package package = GetPackageByName(name);
            
            if (package == null)
            {
                package = new Package { name = name };
                packages.Add(package);
            }

            return package;
        }

        public static Package GetPackageByName(string name)
        {
            for (int i = 0; i < packages.Count; i++)
            {
                if (packages[i].name == name)
                {
                    return packages[i];
                }
            }

            return null;
        }

        public static void RemoveDeletedPackages()
        {
            for (var i = 0; i < packages.Count; i++)
            {
                Package package = packages[i];
                if (!package.existRemotely)
                {
                    packages.Remove(package);
                }
            }
        }
        
        public static void UpdateLocalPackageStatus()
        {
            string vstDownloadedVersion = UnityEditor.EditorPrefs.GetString(PathHelper.VoodooStoreEditorPrefKey, string.Empty);
            
            for (int i = 0; i < packages.Count; i++)
            {
                if (packages[i].RepositoryName == GitHubConstants.StoreRepository2 && string.IsNullOrEmpty(vstDownloadedVersion) == false)
                {
                    packages[i].localVersion = vstDownloadedVersion;
                    UnityEditor.EditorPrefs.DeleteKey(PathHelper.VoodooStoreEditorPrefKey);
                    continue;
                }
                
                string packagePath = Path.Combine(PathHelper.ProjectVoodooPath, packages[i].Name);
                if (packages[i].VersionStatus == VersionState.NotPresent && Directory.Exists(packagePath))
                {
                    packages[i].localVersion = "manually";
                    continue;
                }

                if (packages[i].IsInstalled && Directory.Exists(packagePath) == false)
                {
                    packages[i].localVersion = string.Empty;
                }
            }
        }

        public static void UpdateLocalPackageStatus(Package package)
        {
            string packagePath = Path.Combine(PathHelper.ProjectVoodooPath, package.Name);
            if (Directory.Exists(packagePath) == false)
            {
                package.localVersion = string.Empty;
                return;
            }
            
            if (package.VersionStatus == VersionState.NotPresent)
            {
                package.localVersion = "manually";
            }
        }

        public static void UpdateLabels()
        {
            labels = new List<string>();

            labels.AddRange(SpecialLabels.GetAllSpecialLabels());

            for (int i = 0; i < packages.Count; i++)
            {
                if (packages[i] == null || packages[i].labels == null || packages[i].labels == null)
                {
                    continue;
                }

                List<string> packageLabels = new List<string> (packages[i].labels);
                for (int j = 0; j < packageLabels.Count; j++)
                {
                    if (labels.Any(x => String.Equals(x, packageLabels[j])))
                    {
                        continue;
                    }
                    labels.Add(packageLabels[j]);
                }
            }
        }
        
        public static bool IsFilterActive(Filters filter) 
        {
            return (filters & (int)filter) == (int)filter;
        }
        
        public static bool ContainsPreset(IPackageComposition preset)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].Name == preset.Name && presets[i].Count == preset.Count && presets[i].All(preset.Contains))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanSaveAsPreset(IPackageComposition preset) 
        {
            if (preset == null || preset.Count <= 0)
            {
                return false;
            }

            return ContainsPreset(preset) == false;
        }

        public static void SaveAsPreset(PackagePreset preset)
        {
            if (CanSaveAsPreset(preset) == false)
            {
                return;
            }

            preset.colorText = ColorUtility.ToHtmlStringRGB(preset.Color);
            presets.Add(preset);
        }

        public static void Dispose()
        {
            cart             = PackagePreset.cart;
            favorites        = PackagePreset.favorites;
            selection        = new ObservableCollection<IDownloadable>();
            presets          = new List<PackagePreset>();
            filters          = 0;
            packages         = new List<Package>();
            _info            = null;
        }
    }
}