using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Octokit;
using System.Threading.Tasks;
using UnityEngine;

namespace Voodoo.Store
{
	public class GitHubExport : IDisposable
	{
		private List<TreeItem> currentTreeItems;
		private NewTree        newTree;
		private GitHubCommit   gitHubCommit;
		
		public static event Action<PushState> pushStatusChanged;

        public async Task<bool> PushNewTag(string owner, string repositoryName, string refBranch, string tagName, string tagMessage = "")
        {
	        GitHubCommit commit = await GitHubUtility.GetLatestCommit(owner, repositoryName, refBranch);

	        NewTag newTag = new NewTag
	        {
		        Tag = tagName,
		        Message = string.IsNullOrEmpty(tagMessage) ? $"Tag {tagName} created programatically" : tagMessage,
		        Object = commit.Sha,
		        Type = TaggedType.Commit
	        };

	        try
	        {
		        GitTag gitTag = await GitHubUtility.CreateTag(owner, repositoryName, newTag);

		        try
		        {
			        string reference = string.Concat("refs/tags/", gitTag.Tag);
			        NewReference newReference = new NewReference(reference, gitTag.Sha);
			        Reference gitRef = await GitHubUtility.CreateReference(owner, repositoryName, newReference);
            
			        return gitRef != null;
		        }
		        catch
		        {
			        Debug.LogError($"Failed to push the tag {tagName} to the reference branch {refBranch}");
			        return false;
		        }
	        }
	        catch
	        {
		        Debug.LogError($"Failed to create tag {tagName} on the repository {repositoryName} with the owner {owner}");
		        return false;
	        }
        }

        public async Task AddToGit(string repositoryName, string reference, string filePath, string content, bool isBase64Encoded) 
        {
	        // if there is no temporary tree, create a new one.
	        // it will contains all file added to the commit 
	        // and will have the previous commit as parent
	        if (newTree == null)
	        {
		        try
		        {
			        currentTreeItems = await GetTreeItems(repositoryName,reference);
			        gitHubCommit     = await GitHubUtility.GetLatestCommit(GitHubConstants.Owner, repositoryName, reference);
			        newTree          = new NewTree();
		        }
		        catch
		        {
			        Debug.LogError($"Couldn't commit to the repository {repositoryName} with the reference {reference}");
			        return;
		        }
	        }

	        // Add content to commitTree. It will automatically create the associated blob
	        NewTreeItem treeItem = new NewTreeItem
	        {
		        Path = filePath,
		        Mode = "100644",
		        Type = TreeType.Blob
	        };

	        if (isBase64Encoded)
	        {
		        NewBlob tempBlob = new NewBlob
		        {
			        Encoding = EncodingType.Base64,
			        Content = content
		        };
	        
		        pushStatusChanged?.Invoke(PushState.ADDING_FILES);
		        BlobReference blobRef = await GitHubUtility.CreateBlobReference(GitHubConstants.Owner, repositoryName, tempBlob);
		        treeItem.Sha = blobRef.Sha;
	        }
	        else
	        {
		        treeItem.Content = content;
	        }
	        
	        newTree.Tree.Add(treeItem);
        }

        public async Task AddToGit(string repositoryName, string reference, string filePath, string fileSha) 
        {
	        // if there is no temporary tree, create a new one.
	        // it will contains all file added to the commit 
	        // and will have the previous commit as parent
	        if (newTree == null)
	        {
		        try
		        {
			        currentTreeItems = await GetTreeItems(repositoryName,reference);
			        gitHubCommit     = await GitHubUtility.GetLatestCommit(GitHubConstants.Owner, repositoryName, reference);
			        newTree          = new NewTree();
		        }
		        catch
		        {
			        Debug.LogError($"Couldn't commit to the repository {repositoryName} with the reference {reference}");
			        return;
		        }
	        }

	        // Add content to commitTree. It will automatically create the associated blob
	        NewTreeItem treeItem = new NewTreeItem
	        {
		        Path = filePath,
		        Mode = "100644",
		        Type = TreeType.Blob,
		        Sha  = fileSha
	        };

	        newTree.Tree.Add(treeItem);
        }

        public async Task<bool> CommitAndPush(string repositoryName, string reference, string commitMessage, bool removeDeletedFiles = true, List<string> filesToKeep = null)
        {
            pushStatusChanged?.Invoke(PushState.START_PUSHING);
            if (newTree == null)
            {
                Debug.LogError("You are trying to push a commit with no file");
                return false;
            }

            if (removeDeletedFiles)
            { 
	            List<TreeItem> deletedFiles = GetDeletedFiles();
	            foreach (TreeItem deletedTreeItem in deletedFiles)
	            {
		            bool shouldKeepFile = ShouldKeepFile(deletedTreeItem.Path, filesToKeep);

		            if (shouldKeepFile == false)
		            {
			            continue;
		            }
		            
		            await AddToGit(repositoryName, reference, deletedTreeItem.Path, deletedTreeItem.Sha);
	            }
            }

            pushStatusChanged?.Invoke(PushState.CREATING_TREE);
            TreeResponse tree = await GitHubUtility.CreateTree(GitHubConstants.Owner, repositoryName, newTree);

            // Create the commit with the tree SHA and the branch sha
            NewCommit newCommit = new NewCommit(commitMessage, tree.Sha, gitHubCommit.Sha);
            
            pushStatusChanged?.Invoke(PushState.CREATING_COMMIT);
            
            var commit = await GitHubUtility.CreateCommit(GitHubConstants.Owner, repositoryName, newCommit);

            pushStatusChanged?.Invoke(PushState.COMPARING_RESULT);
            CompareResult compareResult = await GitHubUtility.CompareCommit(GitHubConstants.Owner, repositoryName, gitHubCommit.Sha, commit.Sha);
            Reference newReference = null;
            if (compareResult.Files.Count > 0)
            {
                pushStatusChanged?.Invoke(PushState.PUSHING);
                newReference = await GitHubUtility.UpdateReference(GitHubConstants.Owner, repositoryName, reference, new ReferenceUpdate(commit.Sha));
            }

            // Update the reference of master branch with the SHA of the commit
            // huge warning you have to use "refs" and at least 2 slash for your ref to be considered, I found it in github doc

            newTree          = null;
            gitHubCommit     = null;
            currentTreeItems = null;
            
            pushStatusChanged?.Invoke(newReference == null ? PushState.PUSH_STOPPED : PushState.PUSH_SUCCESSFULL);
            return newReference != null;
        }

        private List<TreeItem> GetDeletedFiles()
        {
	        List<TreeItem> res = new List<TreeItem>();
            
	        List<string> currentTreePaths = currentTreeItems.Select(x => x.Path).ToList();
	        List<string> newTreePaths = new List<string>();
	        foreach (NewTreeItem newTreeItem in newTree.Tree)
	        {
		        newTreePaths.Add(newTreeItem.Path);
	        }

	        List<string> deletedTreePaths = currentTreePaths.Except(newTreePaths).ToList();
	        foreach (string deletedTreePath in deletedTreePaths)
	        {
		        res.Add(currentTreeItems.Find(x => x.Path == deletedTreePath));
	        }

	        return res;
        }
        
        private bool ShouldKeepFile(string filePath, List<string> filesToKeep)
        {
	        //TODO : Remove this file if the Additional Content folder is empty
	        //Keep meta file for Additional Content folder.
	        if (filePath == "Additional Content.meta")
	        {
		        return true;
	        }
	        
	        if (filesToKeep == null || filesToKeep.Count == 0)
	        {
		        return false;
	        }
	        
	        //Remove the prefix "Assets/" from the elements in "filesToAvoid"
	        List<string> fixedFilesToKeep = new List<string>();
	        foreach (string fileToKeep in filesToKeep)
	        {
		        fixedFilesToKeep.Add(fileToKeep.StartsWith("Assets") ? fileToKeep.Substring(7) : fileToKeep);
	        }

	        if (filePath.StartsWith(PathHelper.AdditionalContentFolderName) == false)
	        {
		        return false;
	        }
	        
	        foreach (string fixedFileToKeep in fixedFilesToKeep)
	        {
				if (fixedFileToKeep.StartsWith("Voodoo")) //If the element start with "Voodoo" -> Remove "Voodoo/PackageName/"
				{
					string[] path = fixedFileToKeep.Split('/');
					int additionalContentIndex = Array.IndexOf(path, PathHelper.AdditionalContentFolderName);
					string newFileToKeep = string.Empty;
					for (int i = additionalContentIndex; i < path.Length; i++)
					{
						if (string.IsNullOrEmpty(newFileToKeep))
						{
							newFileToKeep = path[i];
							continue;
						}

						newFileToKeep = string.Concat(newFileToKeep, Path.AltDirectorySeparatorChar, path[i]);
					}
					
					if (filePath.Contains(newFileToKeep))
					{
						return true;
					}
				}
				else //If the element doesn't start with "Voodoo" -> add "Additional Content/" before
				{
					if (filePath.Contains(string.Concat(PathHelper.AdditionalContentFolderName, Path.AltDirectorySeparatorChar, fixedFileToKeep)))
					{
						return true;
					}
				}
	        }
	        
	        return false;
        }
        
        private async Task<List<TreeItem>> GetTreeItems(string repositoryName, string reference)
        {
	        TreeResponse recursiveRequest = await GitHubUtility.GetTreeResponse(GitHubConstants.Owner, repositoryName, reference, true);
            
	        if (recursiveRequest.Truncated)
	        {
		        TreeResponse response = await GitHubUtility.GetTreeResponse(GitHubConstants.Owner, repositoryName, reference, false);
		        return await FetchTree(response, true);
	        }

	        return await FetchTree(recursiveRequest);
        }
        
        private async Task<List<TreeItem>> FetchTree(TreeResponse response, bool isRecursive = false) 
        {
	        List<Task> tasks = new List<Task>();
	        List<TreeItem> res = new List<TreeItem>();
            
	        for (int i = 0; i < response.Tree.Count; i++)
	        {
		        TreeItem file = response.Tree[i];
                
		        if (file.Type == TreeType.Tree)
		        {
			        if (isRecursive)
			        {
				        TreeResponse subTree = await GitHubUtility.GetTreeResponse(GitHubConstants.Owner, GitHubConstants.StoreRepository, file.Sha, false);
				        tasks.Add(FetchTree(subTree, true));
			        }
                
			        continue;
		        }
		        res.Add(file);
	        }
            
	        Task.WaitAll(tasks.ToArray());

	        return res;
        }

        public void Dispose()
        {
	        currentTreeItems = null;
	        newTree = null;
	        gitHubCommit = null;
	        pushStatusChanged = null;
        }
	}
}