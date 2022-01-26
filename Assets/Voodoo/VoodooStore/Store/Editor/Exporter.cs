using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Voodoo.Store
{ 
	public static class Exporter
	{
		public static ExporterData data;

		public static async Task ExportPackage()
		{
			await GitHubBridge.FetchMaster();
			
	        string repositoryName = data.package.RepositoryName;
	        // string branchRef = repositoryName == GitHubConstants.StoreRepository2 ? "refs/heads/feature/GE-299_Separate_VSTInstaller_from_VST" : "refs/heads/main";
	        string branchRef = "refs/heads/main";

	        if (data.onlyUpdateInfo == false)
	        {
		        await AddFilesToGit(repositoryName, branchRef);

		        List<string> filesToKeep = GetFilesToKeep();
		        
		        bool success = await GitHubBridge.CommitAndPush(repositoryName, branchRef, repositoryName + " v" + data.package.version + " - " + data.commitMessage, true, filesToKeep);
		        
		        if (success)
		        {
			        await AddTagAndUpdateCurrentVersion(repositoryName, branchRef);
		        }
		        else
		        {
			        //Reset version to avoid increasing the version number without having any tag added
			        data.package.version = data.onlinePackage.version;
		        }
	        }
	        
	        UpdatePackageData();
	        await UpdateReadme();
	        
	        await ExporterSlackNotification.SendUpdateNotification();
	        
	        data = null;
	        VoodooStoreState.Current = State.FETCHING;
        }

        private static async Task AddFilesToGit(string repositoryName, string branchRef)
        {
	        //Make sure the the repository exist before pushing files
	        await GitHubBridge.GetOrCreateRepository(repositoryName);
	        
	        //Reset size
	        data.package.size = 0;

	        bool isFirstElement = true;

	        List<Task> tasks = new List<Task>();
	        foreach (string elementToExport in data.elementsToExport)
	        {
		        DirectoryInfo directoryInfo = new DirectoryInfo(elementToExport);
		        FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

		        for (int i = 0; i < files.Length; i++)
		        {
			        if (files[i].FullName.Contains(PathHelper.AdditionalContentFolderName))
			        {
				        continue;
			        }
			        
			        (string filePath, string fileContent, bool isBase64Encoded) preparedFile = PrepareFileForGitExport(files[i], elementToExport);

			        if (preparedFile.filePath == string.Empty)
			        {
				        continue;
			        }

			        if (isFirstElement)
			        {
				        isFirstElement = false;
				        await GitHubBridge.AddToGit(repositoryName, branchRef, preparedFile.filePath, preparedFile.fileContent, preparedFile.isBase64Encoded);
			        }
			        else
			        {
				        Task task = GitHubBridge.AddToGit(repositoryName, branchRef, preparedFile.filePath, preparedFile.fileContent, preparedFile.isBase64Encoded);
				        tasks.Add(task);
			        }
	        
			        // SetUp Size
			        data.package.size += (int) files[i].Length;
		        }
	        }
	        
	        foreach (ExporterAdditionalContent exporterAdditionalContent in data.additionalContents)
	        {
		        if (exporterAdditionalContent.status != AdditionalContentState.UPDATE)
		        {
			        continue;
		        }

		        DirectoryInfo directoryInfo = new DirectoryInfo(exporterAdditionalContent.additionalContent.folderPath);
		        if (directoryInfo.Exists == false)
		        {
			        continue;
		        }

		        FileInfo metaFolderFile = new FileInfo(exporterAdditionalContent.additionalContent.folderPath + ".meta");
		        List<FileInfo> files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
		        files.Insert(0, metaFolderFile);

		        foreach (FileInfo fileInfo in files)
		        {
			        (string filePath, string fileContent, bool isBase64Encoded) preparedFile = PrepareFileForGitExport(fileInfo, exporterAdditionalContent.additionalContent.folderPath);

			        if (preparedFile.filePath == string.Empty)
			        {
				        continue;
			        }

			        if (preparedFile.filePath.StartsWith(PathHelper.AdditionalContentFolderName) == false)
			        {
				        preparedFile.filePath = PathHelper.AdditionalContentFolderName + Path.AltDirectorySeparatorChar + preparedFile.filePath;
			        }

			        Task task = GitHubBridge.AddToGit(repositoryName, branchRef, preparedFile.filePath, preparedFile.fileContent, preparedFile.isBase64Encoded);
			        tasks.Add(task);
		        }
	        }

	        await Task.WhenAll(tasks);
        }

        private static (string, string, bool) PrepareFileForGitExport(FileInfo fileInfo, string elementToExport)
        {
	        string filePath = PathHelper.GetMacPath(fileInfo.FullName);
			        
	        //Do not upload IgnoredFiles
	        if (PathHelper.IgnoredFiles.IndexOf(fileInfo.Name) != -1)
	        {
		        return (string.Empty, string.Empty,false);
	        }

	        //Get the relative file path and remove "Assets" from the beginning
	        filePath = filePath.Substring(filePath.IndexOf(elementToExport, StringComparison.Ordinal) + 7);

	        //Remove "Voodoo" and the package name from the beginning of the file path
	        if (filePath.StartsWith("Voodoo"))
	        {
		        filePath = filePath.Substring(7);
		        filePath = filePath.Substring(data.package.Name.Length + 1);
	        }
			        
	        byte[] bytes = File.ReadAllBytes(fileInfo.FullName);

	        bool isBase64Encoded = false;
	        string fileContent;
	        // test UTF-8 on strict encoding rules. Note that on pure ASCII this will
	        // succeed as well, since valid ASCII is automatically valid UTF-8.
	        try
	        {
		        //Done to verify that the element really is UTF8 encoded
		        fileContent = new System.Text.UTF8Encoding(false, true).GetString(bytes);
	        }
	        catch
	        {
		        // Confirmed as not UTF-8!
		        fileContent = Convert.ToBase64String(bytes);
		        isBase64Encoded = true;
	        }
	        
	        return (filePath, fileContent, isBase64Encoded);
        }

        private static List<string> GetFilesToKeep()
        {
	        List<string> filesToKeep = new List<string>();
		        
	        foreach (ExporterAdditionalContent exporterAdditionalContent in data.additionalContents)
	        {
		        if (exporterAdditionalContent.status != AdditionalContentState.KEEP)
		        {
			        continue;
		        }

		        filesToKeep.Add(exporterAdditionalContent.additionalContent.folderPath);
	        }

	        return filesToKeep;
        }
        
        private static async Task AddTagAndUpdateCurrentVersion(string repositoryName, string branchRef)
        {
	        bool success = await GitHubBridge.PushNewTag(repositoryName, branchRef, data.package.version, data.commitMessage);
	        if (success)
	        {
		        data.package.updatedAt = DateTime.UtcNow.ToString("u");
		        data.package.localVersion = data.package.version;
		        
		        if (data.package.tags == null)
		        {
			        data.package.tags = new List<string>();
		        }
		        
		        if (data.package.tags.IndexOf(data.package.version) == -1)
		        {
			        data.package.tags.Insert(0, data.package.version);
		        }
	        }
        }
        
        private static void UpdatePackageData()
        {
	        //SetUp dependencies
	        data.package.dependencies = new List<string>();
	        foreach (DependencyPackage dependencyPackage in data.dependencyPackages)
	        {
		        if (dependencyPackage.isSelected)
		        {
			        data.package.dependencies.Add(dependencyPackage.name);
		        }
	        }
	        
	        //Setup Additional content
	        data.package.additionalContents = new List<AdditionalContent>();
	        foreach (ExporterAdditionalContent exporterAdditionalContent in data.additionalContents)
	        {
		        if (exporterAdditionalContent.status != AdditionalContentState.REMOVE)
		        {
			        data.package.additionalContents.Add(exporterAdditionalContent.additionalContent);
		        }
	        }
	        
	        //SetUp labels
	        data.package.labels = new List<string>();
	        foreach (ExporterLabel label in data.labels)
	        {
		        if (label.isSelected)
		        {
			        data.package.labels.Add(label.labelName);
		        }
	        }

	        Package currentPackage = VoodooStore.GetOrCreatePackage(data.package.name);
	        
	        if(string.IsNullOrEmpty(data.package.localVersion))
	        {
		        data.package.localVersion = currentPackage.localVersion;
		        data.package.updatedAt = currentPackage.updatedAt;
	        }
	        
	        VoodooStore.packages[VoodooStore.packages.IndexOf(currentPackage)] = data.package;
	        
	        VoodooStoreSerializer.Write();
        }
        
        private static async Task UpdateReadme()
        {
	        string branchRef = GitHubConstants.GetRefBranch(GitHubConstants.ReadmeBranch);
	        
	        PackageList packageList = new PackageList
	        {
		        packages = VoodooStore.packages.OrderBy(x => x.name).ToList()
	        };
	        
	        string fileContent = JsonUtility.ToJson(packageList, true);
	        
	        await GitHubBridge.AddToGit(branchRef, PathHelper.readme, fileContent);
	        await GitHubBridge.CommitAndPush(branchRef, data.package.Name + " v" + data.package.version, false);
        }
    }
}