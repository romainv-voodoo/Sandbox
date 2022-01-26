using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Voodoo.Store
{
    public static class VoodooStoreBridge
    {
        static string _latestSha = string.Empty;

        public static async Task<bool> FetchMaster()
        {
            // IReadOnlyList<Octokit.Repository> repositories2 = await Client.Organization.Team.GetAllRepositories(GitHubConstants.GamingEngineeringId);
            //
            // Debug.Log(repositories2.Count);
            // foreach (var repository in repositories2)
            // {
            //     if (repository.Name.StartsWith("VST_"))
            //     {
            //         Debug.Log("Get : " + repository.FullName);
            //     }
            // }
            //
            //
            // RepositoryRequest rq = new RepositoryRequest
            // {
            //     Affiliation = RepositoryAffiliation.OrganizationMember,
            //     Visibility = RepositoryVisibility.Private,
            // };
            //
            // var repositories = await Client.Repository.GetAllForCurrent(rq);
            //
            // Debug.Log(repositories.Count);
            // foreach (var repository in repositories)
            // {
            //     if (repository.Name.StartsWith("VST_"))
            //     {
            //         Debug.Log("Get : " + repository.FullName);
            //     }
            // }

            bool success = await RetrievePackages();

            VoodooStore.RemoveDeletedPackages();
            VoodooStore.UpdateLocalPackageStatus();

            return success;
        }

        private static async Task<bool> RetrievePackages()
        {
            string reference = GitHubConstants.ReadmeBranch;
            try
            {
                var treeResponse = await GitHubUtility.GetTreeResponse(GitHubConstants.Owner, GitHubConstants.StoreRepository, reference, true);
                
                bool success = false;
                for (int i = 0; i < treeResponse.Tree.Count; i++)
                {
                    var file = treeResponse.Tree[i];
                    if (file.Path != PathHelper.readme)
                    {
                        continue;
                    }

                    success = true;
                    await FetchPackagesInfo(file.Sha);
                    break;
                }

                if (success == false)
                {
                    Debug.LogError($"No packages were found because we didn't find the {PathHelper.readme} file.");
                    return false;
                }
            }
            catch
            {
                Debug.LogError($"Couldn't access the reference {reference} from the VoodooStore repository.");
                return false;
            }

            return true;
        }
        
        private static async Task FetchPackagesInfo(string sha)
        {
            //    if (_latestSha == sha)
            //    {
            //        return;
            //    }

            //    _latestSha = sha;

            var blob = await GitHubUtility.GetBlob(GitHubConstants.Owner, GitHubConstants.StoreRepository, sha);
            byte[] content = Convert.FromBase64String(blob.Content);
            string json = System.Text.Encoding.UTF8.GetString(content);
            
            PackageList packageList = JsonUtility.FromJson<PackageList>(json);
            foreach (Package package in packageList.packages)
            {
                Package pkg = VoodooStore.GetOrCreatePackage(package.name);
                
                pkg.existRemotely       = true;
                pkg.name                = package.name;
                pkg.author              = package.author;
                pkg.displayName         = package.displayName;
                pkg.description         = package.description;
                pkg.version             = package.version;
                pkg.unityVersion        = package.unityVersion;
                pkg.size                = package.size;
                pkg.documentationLink   = package.documentationLink;
                pkg.updatedAt           = package.updatedAt;
                pkg.dependencies        = package.dependencies;
                pkg.labels              = package.labels;
                pkg.tags                = package.tags;
                pkg.additionalContents  = package.additionalContents;
            }
        }
    }
}