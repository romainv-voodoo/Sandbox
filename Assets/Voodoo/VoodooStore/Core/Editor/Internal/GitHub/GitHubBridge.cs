using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

namespace Voodoo.Store
{
    public static class GitHubBridge
    {
        private static GitHubExport               gitHubExport;
        public  static event Action<bool>         fetchCompleted;
        
        public static void OnEnable() => gitHubExport = new GitHubExport();
        
        public static async Task<bool> FetchMaster()
        {
            bool success = false;

#if VOODOO_STORE_EDITOR
            success = await VoodooStoreBridge.FetchMaster();
#else
            success = await GitHubDownload.DownloadCodeFromLatestTag(GitHubConstants.Owner, GitHubConstants.StoreRepository2);

            if (success)
            {
                SymbolHelper.AddSymbolForAllGroups(SymbolHelper.StoreEditorSymbol);
            }
#endif
            FinishFetch(success);

            return success;
        }

        public static async Task<bool> DownloadCodeFromLatestTag(string repositoryName, List<string> filesToAvoid, bool keepArchive = true)
        {
            return await GitHubDownload.DownloadCodeFromLatestTag(GitHubConstants.Owner, repositoryName, filesToAvoid, keepArchive);
        }

        public static async Task<Repository> GetOrCreateRepository(string repositoryName)
        {
            return await GitHubRepositoryManagement.GetOrCreateRepository(repositoryName);
        }
        
        public static async Task<bool> PushNewTag(string repositoryName, string refBranch, string tagName, string tagMessage)
        {
            return await gitHubExport.PushNewTag(GitHubConstants.Owner, repositoryName, refBranch, tagName, tagMessage);
        }

        public static async Task AddToGit(string repositoryName, string reference, string filePath, string content, bool isBase64Encoded = false)
        {
            await gitHubExport.AddToGit(repositoryName, reference, filePath, content, isBase64Encoded);
        }

        public static async Task AddToGit(string reference, string filePath, string content, bool isBase64Encoded = false)
        {
            await gitHubExport.AddToGit(GitHubConstants.StoreRepository, reference, filePath, content, isBase64Encoded);
        }

        public static async Task<bool> CommitAndPush(string repositoryName, string reference, string commitMessage, bool removeDeletedFiles = true, List<string> filesToKeep = null)
        {
            return await gitHubExport.CommitAndPush(repositoryName, reference, commitMessage, removeDeletedFiles, filesToKeep);
        }

        public static async Task<bool> CommitAndPush(string reference, string commitMessage, bool removeDeletedFiles = true, List<string> filesToKeep = null)
        {
            return await gitHubExport.CommitAndPush(GitHubConstants.StoreRepository, reference, commitMessage, removeDeletedFiles, filesToKeep);
        }
        
        public static async Task<bool> IsUserInTeam(int teamID)
        {
            return await GitHubUserInformation.IsUserInTeam(teamID);
        }
        
        public static async Task<bool> IsUserInTeams(List<int> teamsID)
        {
            return await GitHubUserInformation.IsUserInTeams(teamsID);
        }

        private static void FinishFetch(bool success) 
        {
            AssetDatabase.Refresh();
            fetchCompleted?.Invoke(success);
        }

        public static void Dispose()
        {
            fetchCompleted    = null;
            
            GitHubDownload.Dispose();            
            GitHubRepositoryManagement.Dispose();
            
            gitHubExport.Dispose();
            gitHubExport = null;
        }
    }

    public class SymbolHelper : UnityEditor.AssetModificationProcessor
    {
        public const string StoreEditorSymbol = "VOODOO_STORE_EDITOR";

        private static List<BuildTargetGroup> groups = new List<BuildTargetGroup>
        {
            BuildTargetGroup.Android, 
            BuildTargetGroup.iOS,
            BuildTargetGroup.Standalone
        }; 
        
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options) 
        {
            string storepath = PathHelper.StoreEditorPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (storepath.IndexOf(path) >= 0)
            {
                RemoveSymbolFromAllGroups(StoreEditorSymbol);
            }

            return AssetDeleteResult.DidNotDelete;
        }

        public static void RemoveSymbol(string symbol, BuildTargetGroup group) 
        {
            List<string> symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            
            int index = symbols.IndexOf(symbol);
            if (index < 0)
            {
                return;
            }
            
            symbols.RemoveAt(index);
            var result = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, result);
        }

        public static void RemoveSymbolFromAllGroups(string symbol)
        {
            if (groups.Contains(EditorUserBuildSettings.selectedBuildTargetGroup) == false)
            {
                groups.Add(EditorUserBuildSettings.selectedBuildTargetGroup);
            }

            foreach (BuildTargetGroup grp in groups)
            {
                RemoveSymbol(symbol, grp);
            }
        }

        public static void AddSymbol(string symbol, BuildTargetGroup group)
        {
            List<string> symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            
            int index = symbols.IndexOf(symbol);
            if (index >= 0)
            {
                return;
            }
            
            symbols.Add(symbol);
            var result = string.Join(";", symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, result);
        }

        public static void AddSymbolForAllGroups(string symbol)
        {
            if (groups.Contains(EditorUserBuildSettings.selectedBuildTargetGroup) == false)
            {
                groups.Add(EditorUserBuildSettings.selectedBuildTargetGroup);
            }

            foreach (BuildTargetGroup grp in groups)
            {
                AddSymbol(symbol, grp);
            }
        }
    }
}