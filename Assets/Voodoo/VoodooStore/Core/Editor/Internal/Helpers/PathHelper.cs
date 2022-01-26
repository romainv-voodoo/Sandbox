using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Voodoo.Store
{
    public static class PathHelper
    {
        public static readonly string VST_NAME = "Store";

        public const string zip                           = ".zip";
        public const string readme                        = "readme.json";

        public const string vstPortal                     = "https://voodoo.atlassian.net/wiki/spaces/VST/overview";
        public const string serviceDeskSupport            = "https://voodoo.atlassian.net/servicedesk/customer/portal/11";
        public const string slackSupport                  = "https://app.slack.com/client/T07ELDMJ9/CU9007RG8";
        
        public static readonly string UserFolderLocation  = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static readonly string DirectoryPath       = Path.Combine(UserFolderLocation, "Voodoo");
        public static readonly string ProjectVoodooPath   = Path.Combine(Application.dataPath, "Voodoo");
        public static readonly string StoreEditorPath     = Path.Combine(ProjectVoodooPath, "VoodooStore", VST_NAME);
        public static readonly string RelativeStorePath   = Path.Combine("Assets", "Voodoo", "VoodooStore", VST_NAME);

        public static readonly string VoodooStoreEditorPrefKey = "VoodooStore-Downloaded-Version";
     
        public static readonly List<string> IgnoredFiles  = new List<string>()
        {
            ".DS_Store",
            ".git"
            // "README.md",
            // ".gitignore"
        };

        public static readonly string AdditionalContentFolderName = "Additional Content";

        public static readonly List<string> SpecialExternalFolders = new List<string>
        {
            Path.Combine(AdditionalContentFolderName, "Resources"),
            Path.Combine(AdditionalContentFolderName, "Editor Default Resources"),
            Path.Combine(AdditionalContentFolderName, "Gizmos"),
            Path.Combine(AdditionalContentFolderName, "StreamingAssets"),
            Path.Combine(AdditionalContentFolderName, "Standard Assets"),
        };

        public static string GetMacPath(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}