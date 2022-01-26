using System;
using UnityEngine;

namespace Voodoo.CI
{
    [CreateAssetMenu(fileName = "VoodooCiVersion", menuName = "VoodooCI/Create CI Version", order = 1)]
    public class VoodooCiPackageVersion : ScriptableObject
    {
        [field: SerializeField] public int Major { get; set; }
        [field: SerializeField] public int Minor { get; set; }
        [field: SerializeField] public int Patch { get; set; }

        public override string ToString() => CurrentVersion;

        public string CurrentVersion => $"{Major}.{Minor}.{Patch}";
        public string NextPackageVersion => $"{Major}.{Minor}.{Patch + 1}";

        public void UpdateVersion(string nextPackageVersion)
        {
            var nextPackage = nextPackageVersion.Split('.');
            Major = Convert.ToInt32(nextPackage[0]);
            Minor = Convert.ToInt32(nextPackage[1]);
            Patch = Convert.ToInt32(nextPackage[2]);
        }
    }
}