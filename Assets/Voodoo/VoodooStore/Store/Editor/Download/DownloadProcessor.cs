using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public static class DownloadProcessor
    {
        static Queue<DownloadRequest> pendingRequests = new Queue<DownloadRequest>();
        static DownloadRequest        request;

        public static bool HasRequestRunning => pendingRequests.Count > 0 || request != null;

        public static event Action downloadEnd;

        ///////////Install functions/////////////
        public static DownloadRequest DownloadPackages(List<Package> packages)
        {
            for (var i = packages.Count - 1; i >= 0; i--)
            {
                Package package = packages[i];
                if (package.VersionStatus == VersionState.UpToDate)
                {
                    packages.RemoveAt(i);
                }
            }
            
            if (packages.Count == 1)
            {
                return DownloadPackage(packages[0]);
            }
            
            // int downloadOption = GetDownloadOption(packages);
            int downloadOption = MultiDownloadWindow.ShowWindow(packages);

            if (downloadOption == 1)
            {
                return null;
            }

            return CreateAndAddRequest(packages, downloadOption == 0);
        }
        
        public static DownloadRequest DownloadPackage(Package package)
        {
            return CreateAndAddRequest(new List<Package> {package}, false);
        }

        private static DownloadRequest CreateAndAddRequest(List<Package> packages, bool autoDownload)
        {
            DownloadRequest downloadRequest = new DownloadRequest(packages, autoDownload);
            AddRequest(downloadRequest);

            return downloadRequest;
        }

        private static void AddRequest(DownloadRequest newRequest)
        {
            pendingRequests.Enqueue(newRequest);
            if (request != null)
            {
                return;
            }

            NextRequest();
        }

        private static void NextRequest()
        {
            if (pendingRequests.Count <= 0)
            {
                request = null;
                downloadEnd?.Invoke();
                return;
            }

            request = pendingRequests.Dequeue();
            request.Start(NextPackage, NextRequest);
        }

        private static async void NextPackage()
        {
            bool downloadPackage = true;
            List<OptionalElement> optionalElements = new List<OptionalElement>();
            
            // if (request.downloadDemo)
            // {
            //     optionalElements.Add(new OptionalElement(Path.Combine("Assets","Voodoo",request.Current.Name,"Additional Content", "Demo")));
            // }
            
            if (request.autoDownload == false)
            {
                downloadPackage = PackageDownloadWindow.ShowWindow(request);
                optionalElements = PackageDownloadWindow.window.optionalElements;
            }
            
            if (downloadPackage)
            {
                List<string> filesToAvoid = new List<string>();
                foreach (OptionalElement optionalElement in optionalElements)
                {
                    if (optionalElement.isSelected == false)
                    {
                        filesToAvoid.Add(optionalElement.path);
                    }
                }
        
                bool downloadSucceeded = await GitHubBridge.DownloadCodeFromLatestTag(request.Current.RepositoryName, filesToAvoid);

                if (downloadSucceeded)
                {
                    request.Current.localVersion = request.Current.version;
                }
                else
                {
                    request.Index--;
                }
            }
            else
            {
                request.Index--;
            }

            request.Index++;
        }

        public static void UninstallProcess(List<Package> packages, bool askConfirmation = false)
        {
            for (int i = packages.Count - 1; i >= 0; --i)
            {
                string packageDataPath = Path.Combine(PathHelper.ProjectVoodooPath, packages[i].Name);

                if (packages[i].VersionStatus == VersionState.NotPresent)
                {
                    packages.RemoveAt(i);
                    continue;
                }

                if (Directory.Exists(packageDataPath) == false)
                {
                    packages[i].localVersion = string.Empty;
                    packages.RemoveAt(i);
                }
            }

            string warningMessage = "The following packages will be uninstall:" + Environment.NewLine + Environment.NewLine;

            for (int i = 0; i < packages.Count; i++)
            {
                warningMessage += packages[i].displayName + Environment.NewLine;
            }

            warningMessage += Environment.NewLine;
            warningMessage += Environment.NewLine + "Are you sure you want to do that ?";

            if (EditorUtility.DisplayDialog("Warning", warningMessage, "Yes", "Cancel"))
            {
                for (int i = packages.Count - 1; i >= 0; i--)
                {
                    Uninstall(packages[i]);
                }

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        public static void Uninstall(Package package)
        {
            string packageDataPath = Path.Combine(PathHelper.ProjectVoodooPath, package.Name);

            if (Directory.Exists(packageDataPath))
            {
                Directory.Delete(packageDataPath, true);
                File.Delete(packageDataPath + ".meta");
            }

            package.localVersion = string.Empty;
        }

        public static void DisposeAllRequest()
        {
            for (int i = 0; i < pendingRequests.Count; i++)
            {
                request = pendingRequests.Dequeue();
                request.Dispose();
            }
            
            EditorApplication.UnlockReloadAssemblies();
            #if UNITY_2019_3_OR_NEWER
            AssetDatabase.AllowAutoRefresh();
            #endif
        }
    }

    public sealed class DownloadRequest
    {
        bool disposed;

        public List<Package> packages;
        private int          index;
        public bool          autoDownload;
        // public bool          downloadDemo;

        public int Index
        {
            get => index;
            set
            {
                if (index == value)
                    return;
                
                int currentIndex = index;
                index = value;
                
                if (index < currentIndex)
                {
                    // Debug.LogError(packages[currentIndex].Name + " download has failed.");
                    if (packages.Count > currentIndex)
                    {
                        packages.RemoveAt(currentIndex);
                    }
                }
                else if (index > currentIndex)
                {
                    Next();
                }
            }
        }
        
        public event Action readyForNextPackage;
        public event Action requestComplete;

        public DownloadRequest(List<Package> pkgs, bool auto) 
        {
            packages            = pkgs;
            index               = -1;
            autoDownload        = auto;
            disposed            = false;
            // downloadDemo        = true;
        }
        
        public Package Current => packages[index];

        public void Start(Action onNext, Action onComplete) 
        {
            readyForNextPackage += onNext;
            requestComplete += onComplete;

            EditorApplication.LockReloadAssemblies();
            #if UNITY_2019_3_OR_NEWER
            AssetDatabase.DisallowAutoRefresh();
            #endif

            Index++;
        }

        private void Next() 
        {
            while(index < packages.Count && Current.VersionStatus == VersionState.UpToDate)
            {
                index++;
            }

            if (index >= packages.Count)
            {
                End();
                requestComplete?.Invoke();
                Dispose();
            }
            else
            {
                readyForNextPackage?.Invoke();
            }
        }

        private void End() 
        {
            EditorApplication.UnlockReloadAssemblies();
#if UNITY_2019_3_OR_NEWER
            AssetDatabase.AllowAutoRefresh();
#endif

            //TODO : verify if this is still needed
            
            // ImportAssetOptions importOption = ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive;
            // foreach (var package in packages)
            // {
            //     AssetDatabase.ImportAsset("Assets/Voodoo/" + package.Name, importOption);
            // }

            if (packages.Count > 0)
            {
                AssetDatabase.Refresh();
            }
        }

        public void Dispose() 
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            packages.Clear();
            requestComplete = null;
            readyForNextPackage = null;
            GC.SuppressFinalize(this);
        }
    }
}