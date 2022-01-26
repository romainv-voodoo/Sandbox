using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Voodoo.Store
{
    public static class VoodooStoreSerializer 
    {
        private const string infoFileName    = "VoodooStore-Info.json";
        private const string projectFileName = "packages.json";
        
        public static void Read()
        {
            var packagesData = ReadLocalData<VoodooStoreData>(Path.Combine(PathHelper.DirectoryPath, infoFileName));
            if (packagesData != null)
            {
                FromLocalData(packagesData);
            }

            var versionsData = ReadLocalData<ProjectData>(Path.Combine("VoodooStore", projectFileName));
            if (versionsData != null)
            {
                FromLocalData(versionsData);
            }
        }

        public static T ReadLocalData<T>(string path)
        {
            if (File.Exists(path) == false)
            {
                return default;
            }

            string text;
            using (StreamReader reader = File.OpenText(path))
            {
                text = reader.ReadToEnd();
                reader.Close();
            }

            T data = JsonUtility.FromJson<T>(text);
            
            return data;
        }

        private static void FromLocalData(VoodooStoreData data)
        {
            VoodooStore.packages = data.packageList;
            VoodooStore.presets  = new List<PackagePreset>();
            foreach (PackagePreset packagePreset in data.presets)
            {
                GetPackageReferences(packagePreset, out PackagePreset newPreset);
                VoodooStore.presets.Add(newPreset);
            }
            
            GetPackageReferences(data.cart, out VoodooStore.cart);
            GetPackageReferences(data.favorites, out VoodooStore.favorites);
        }

        private static void GetPackageReferences<T>(T from, out T to) where T : PackagePreset
        {
            to = from;
            
            List<Package> tempPackages = new List<Package>(from);
            to.Clear();
            foreach (Package package in tempPackages)
            {
                Package vstPackage = VoodooStore.GetOrCreatePackage(package.name);
                if (vstPackage != null)
                {
                    to.Add(vstPackage);
                }
            }
        }

        private static void FromLocalData(ProjectData data)
        {
            for (int i = 0; i < data.packagesNames.Count; i++)
            {
                Package pkg =  VoodooStore.GetOrCreatePackage(data.packagesNames[i]);
                if (pkg == null)
                {
                    continue;
                }

                pkg.localVersion = data.packagesVersion[i];
            }
        }

        public static void Write()
        {
            ToLocalData(out VoodooStoreData packagesData);
            WriteLocalData(Path.Combine(PathHelper.DirectoryPath, infoFileName), packagesData);

            ToLocalData(out ProjectData versionsData);
            WriteLocalData(Path.Combine("VoodooStore", projectFileName), versionsData);
        }

        public static void WriteLocalData<T>(string path, T data)
        {
            string text = JsonUtility.ToJson(data, true);
            
            FileInfo fileInfo = new FileInfo(path);
            fileInfo.Directory?.Create();
            File.WriteAllText(path, text);
        }

        private static void ToLocalData(out VoodooStoreData data) 
        {
            data = new VoodooStoreData
            {
                packageList = VoodooStore.packages,
                cart        = VoodooStore.cart,
                favorites   = VoodooStore.favorites,
                presets     = new List<PackagePreset>(VoodooStore.presets)
            };
        }

        private static void ToLocalData(out ProjectData data)
        {
            data = new ProjectData();
            List<Package> packages = VoodooStore.packages.OrderBy(x => x.name).ToList();
            foreach (Package pkg in packages)
            {
                if (pkg == null || string.IsNullOrEmpty(pkg.localVersion))
                {
                    continue;
                }
                
                data.packagesNames.Add(pkg.name);
                data.packagesVersion.Add(pkg.localVersion);
            }
        }
    }
}