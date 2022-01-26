using System;
using Octokit;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;
using UnityEngine;

namespace Voodoo.Store
{
	public static class GitHubDownload
	{
        public static event Action<DownloadState> downloadStatusChanged;

        public static async Task<bool> DownloadCodeFromLatestTag(string owner, string repositoryName, List<string> filesToAvoid = null, bool keepArchive = true)
        {
            downloadStatusChanged?.Invoke(DownloadState.START_DOWNLOADING);
            string archivePath = Path.Combine(PathHelper.DirectoryPath, "Packages", repositoryName + PathHelper.zip);

            RepositoryTag latestTag;
            try
            {
                IReadOnlyList<RepositoryTag> tags = await GitHubUtility.GetAllTags(owner, repositoryName);
                latestTag = tags[0];
            }
            catch
            {
                Debug.LogError($"Failed to retrieve tags from repository {repositoryName}");
                downloadStatusChanged?.Invoke(DownloadState.DOWNLOAD_STOPPED);
                return false;
            }

            downloadStatusChanged?.Invoke(DownloadState.CREATE_ARCHIVE);
            
            //Temporary modification to allow testing
            //bool success = await CreateArchive(archivePath, repositoryName, repositoryName == GitHubConstants.StoreRepository2 ? "feature/data_cleaning" : latestTag.Name);
            
            bool success = await CreateArchive(archivePath, repositoryName, latestTag.Name);

            if (success == false)
            {
                downloadStatusChanged?.Invoke(DownloadState.DOWNLOAD_STOPPED);
                return false;
            }
            
            downloadStatusChanged?.Invoke(DownloadState.EXTRACT_ARCHIVE);
            ExtractArchive(archivePath, PathHelper.ProjectVoodooPath, filesToAvoid);
            
            downloadStatusChanged?.Invoke(DownloadState.RENAME_ARCHIVE);
            RenameArchive(repositoryName, PathHelper.ProjectVoodooPath);
            
            if (keepArchive == false)
            {
                File.Delete(archivePath);
            }

            downloadStatusChanged?.Invoke(DownloadState.DOWNLOAD_FINISHED);

            if (repositoryName == GitHubConstants.StoreRepository2)
            {
                UnityEditor.EditorPrefs.SetString(PathHelper.VoodooStoreEditorPrefKey, latestTag.Name);
            }
            return true;
        }

        private static async Task<bool> CreateArchive(string zipPath, string repositoryName, string reference)
        {
            try
            {
                byte[] archive = await GitHubUtility.GetArchive(GitHubConstants.Owner, repositoryName, ArchiveFormat.Zipball, reference);

                FileInfo file = new FileInfo(zipPath);
                file.Directory?.Create();
                File.WriteAllBytes(zipPath, archive);
            }
            catch
            {
                Debug.LogError($"Failed to retrieve archive from repository {repositoryName} with reference {reference}");
                return false;
            }

            return true;
        }
        
        private static void ExtractArchive(string archivePath, string outputFile, List<string> filesToAvoid)
        {
            if (filesToAvoid == null)
            {
                filesToAvoid = new List<string>();
            }
            
            //Remove the prefix "Assets/" from the elements in "filesToAvoid"
            List<string> fixedFilesToAvoid = new List<string>();
            foreach (string fileToAvoid in filesToAvoid)
            {
                fixedFilesToAvoid.Add(fileToAvoid.StartsWith("Assets") ? fileToAvoid.Substring(7) : fileToAvoid);
            }
            
            using (ZipFile zipFile = ZipFile.Read(archivePath))
            {
                // Loop through the archive's files.
                foreach (ZipEntry entry in zipFile)
                {
                    bool avoidFile = ShouldAvoidFile(entry, fixedFilesToAvoid);

                    if (avoidFile)
                    {
                        continue;
                    }
                    
                    entry.Extract(outputFile);
                }
            }
        }

        private static bool ShouldAvoidFile(ZipEntry entry, List<string> filesToAvoid)
        {
            if (filesToAvoid.Count == 0)
            {
                return false;
            }
            
            string entryPath = entry.FileName;
            string[] path = entryPath.Split(Path.AltDirectorySeparatorChar);
            int additionalContentIndex = Array.IndexOf(path, PathHelper.AdditionalContentFolderName);

            if (additionalContentIndex == -1)
            {
                return false;
            }
            
            //Format the file to obtain the relative path without the organization and the "-commitId" (ex: PackageName/Folder1/FileName)
            int baseFolderIndex = additionalContentIndex - 1;
            string[] pathComposition = path[baseFolderIndex].Split('-');
            string newBasePath = string.Empty;
            for (int i = 1; i < pathComposition.Length-1; i++)
            {
                newBasePath = string.Concat(newBasePath, "-", pathComposition[i]);
            }
            path[baseFolderIndex] = newBasePath.Replace("VST_", "");

            string newEntryPath = string.Empty;
            for (int i = baseFolderIndex; i < path.Length; i++)
            {
                newEntryPath = string.Concat(newEntryPath, Path.AltDirectorySeparatorChar, path[i]);
            }

            //Remove the special external folders. They will be added automatically if needed when creating the file
            for (var i = 0; i < PathHelper.SpecialExternalFolders.Count; i++)
            {
                string specialExternalFolder = PathHelper.SpecialExternalFolders[i];
                if (newEntryPath.EndsWith(specialExternalFolder))
                {
                    return true;
                }
            }

            for (var i = 0; i < filesToAvoid.Count; i++)
            {
                string fileToAvoid = filesToAvoid[i];
                if (fileToAvoid.StartsWith("Voodoo")) //If the element start with "Voodoo" -> look for the entry name
                {
                    if (newEntryPath.Contains(fileToAvoid.Substring(7)))
                    {
                        return true;
                    }
                }
                else //If the element doesn't start with "Voodoo" -> look for "PackageName/Additional Content/"
                {
                    if (string.Concat(path[baseFolderIndex], Path.AltDirectorySeparatorChar, path[additionalContentIndex], Path.AltDirectorySeparatorChar, newEntryPath).Contains(fileToAvoid))
                    {
                        return true;
                    }
                }
            }

            //Remove the folder "Additional Content". It will be added automatically if there is a file in it
            return newEntryPath == string.Concat(path[baseFolderIndex], Path.AltDirectorySeparatorChar, path[additionalContentIndex]);
        }

        private static void RenameArchive(string repositoryName, string outputPath)
        {
            string pckgName = repositoryName == GitHubConstants.StoreRepository2 ? GitHubConstants.StoreRepository : repositoryName.Replace("VST_", string.Empty);

            string newDirPath = Path.Combine(outputPath, pckgName);
            if (Directory.Exists(newDirPath))
            {
                Directory.Delete(newDirPath, true);
            }

            string[] directories = Directory.GetDirectories(outputPath);
            for (var i = 0; i < directories.Length; i++)
            {
                string directory = directories[i];
                if (directory.Contains(repositoryName) == false)
                {
                    continue;
                }

                string[] subDirectories = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);
                for (var i1 = 0; i1 < subDirectories.Length; i1++)
                {
                    string subDirectory = subDirectories[i1];

                    for (var i2 = 0; i2 < PathHelper.SpecialExternalFolders.Count; i2++)
                    {
                        if (subDirectory.EndsWith(PathHelper.SpecialExternalFolders[i2]) == false)
                        {
                            continue;
                        }

                        string[] folders = subDirectory.Split(Path.DirectorySeparatorChar);
                        int additionalContentIndex = Array.IndexOf(folders, PathHelper.AdditionalContentFolderName);
                        string newDirectoryPath = string.Empty;
                        for (int i3 = additionalContentIndex + 1; i3 < folders.Length; i3++)
                        {
                            newDirectoryPath = Path.Combine(newDirectoryPath, folders[i3]);
                        }

                        string[] files = Directory.GetFiles(subDirectory, "*", SearchOption.AllDirectories);
                        for (var i3 = 0; i3 < files.Length; i3++)
                        {
                            string file = files[i3];

                            string newFilePath = Path.Combine(UnityEngine.Application.dataPath, file.Substring(file.IndexOf(newDirectoryPath, StringComparison.Ordinal)));

                            FileInfo fileInfo = new FileInfo(newFilePath);
                            fileInfo.Directory?.Create();

                            if (File.Exists(newFilePath))
                            {
                                File.Delete(newFilePath);
                            }

                            File.Move(file, newFilePath);
                        }

                        Directory.Delete(subDirectory, true);
                        break;
                    }
                }

                try
                {
                    string additionalContentFolder = Path.Combine(directory, PathHelper.AdditionalContentFolderName);
                    Directory.Delete(additionalContentFolder);
                }
                catch
                {
                    // ignored
                }

                Directory.Move(directory, newDirPath);
                break;
            }
        }
        
        public static void Dispose()
        {
            downloadStatusChanged = null;
        }
	}
}