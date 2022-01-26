using System.IO;
using UnityEngine;

namespace Voodoo.Distribution
{
    public static class PathHelper
    {
        static readonly string Assets = "Assets";
        static readonly string CompanyName = "Voodoo";
        
        public static string ToolLocalPath(string toolName)
        {
            return Path.Combine(CompanyName, toolName);
        }

        public static string ToolAbsolutePath(string toolName)
        {
            return Path.Combine(Application.dataPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), CompanyName, toolName);
        }

        public static string ToolAssetPath(string toolName)
        {
            return Path.Combine(Assets, CompanyName, toolName);
        }

        public static string ToLocalPath(this string absolutePath)
        {
            int index = absolutePath.IndexOf(Assets);
            if (index < 0)
            {
                return absolutePath;
            }

            return absolutePath.Substring(index);
        }
    }
}