using System;
using System.Collections.Generic;
using System.Linq;

namespace Voodoo.Store
{
    [Serializable]
    public class Package : IDownloadable
    {
        public string   name;
        public string   author;
        public string   displayName;
        public string   description;
        public string   version;
        public string   unityVersion;
        public string   updatedAt;
        
        public int      size;
        public string   documentationLink;

        [NonSerialized]
        public string   localVersion;

        [NonSerialized]
        public bool existRemotely = false;

        public List<AdditionalContent> additionalContents;
        
        public List<string> dependencies;
        public List<string> labels;
        public List<string> tags;

        public string Name => name == PathHelper.VST_NAME ? "VoodooStore" : name;
        public string RepositoryName => $"VST_{name}";

        public int VersionStatus => string.IsNullOrEmpty(localVersion) ? VersionState.NotPresent : 
            localVersion == version ? VersionState.UpToDate : 
            localVersion == "manually" ? VersionState.Manually : VersionState.OutDated;
        public bool IsInstalled  => VersionStatus == VersionState.UpToDate || VersionStatus == VersionState.OutDated;

        public List<Package> Content => new List<Package> { this };

        public List<Package> GetDependencies(List<Package> packageList)
        {
            List<Package> res = new List<Package>();
            for (int i = 0; i < dependencies.Count; i++)
            {
                Package package = packageList.Find(x => x.name == dependencies[i]);

                if (package == null)
                {
                    continue;
                }

                res.AddRange(package.GetDependencies(packageList));
            }

            res.Add(this);
            res.RemoveAll(x => x == null);
            res = res.Distinct().ToList();

            return res;
        }

        public List<Package> GetRequirements(List<Package> packageList)
        {
            List<Package> res = new List<Package> {this};
            
            for (int i = 0; i < packageList.Count; i++)
            {
                if (packageList[i].dependencies.Contains(name) == false)
                {
                    continue;
                }

                res.AddRange(packageList[i].GetRequirements(packageList));
            }

            res.RemoveAll(x => x == null);
            res = res.Distinct().ToList();

            return res;
        }

        public override string ToString()
        {
            return string.Format("name : {0}, author : {1}, displayName : {2}, description : {3}," +
                " version : {4}, unityVersion : {5}, size : {6}, localVersion : {7}," +
                " tags : {8}, dependencies : {9}, labels : {10}",
                name, author, displayName, description, version, unityVersion, size, localVersion, tags, dependencies, labels);
        }
    }
}