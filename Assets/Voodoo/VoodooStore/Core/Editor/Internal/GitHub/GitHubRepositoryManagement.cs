using System;
using Octokit;
using System.Threading.Tasks;
using UnityEngine;

namespace Voodoo.Store
{
	public static class GitHubRepositoryManagement
	{
		public static event Action<CreateRepoState> createRepositoryStatusChanged;
		
		public static async Task<Repository> GetOrCreateRepository(string repositoryName)
		{
			try
			{
				return await GitHubUtility.GetRepository(GitHubConstants.Owner, repositoryName);
			}
			catch
			{
				return await CreateRepository(repositoryName);
			}
		}
		
		private static async Task<Repository> CreateRepository(string repositoryName)
		{
			createRepositoryStatusChanged?.Invoke(CreateRepoState.CREATE_REPOSITORY);
			NewRepository newRepository = new NewRepository(repositoryName)
			{
				Private = true,
				GitignoreTemplate = "Unity",
			};

			try
			{
				Repository createdRepo = await GitHubUtility.CreateRepository(GitHubConstants.Owner, newRepository);
                
				createRepositoryStatusChanged?.Invoke(CreateRepoState.GRANT_ACCESS);
				await GrantAccess(repositoryName, GitHubConstants.GamingEngineeringTeamId, Permission.Admin);
				await GrantAccess(repositoryName, GitHubConstants.VoodooStoreTeamId, Permission.Push);
				await GrantAccess(repositoryName, GitHubConstants.GameTeamId, Permission.Push);
				await GrantAccess(repositoryName, GitHubConstants.LabTeamId, Permission.Push);
				await GrantAccess(repositoryName, GitHubConstants.MarketingGameDevTeamId, Permission.Push);
				await GrantAccess(repositoryName, GitHubConstants.PublishingOperationsTeamId, Permission.Push);
				await GrantAccess(repositoryName, GitHubConstants.CasualEngineeringTeamId, Permission.Push);
				
				createRepositoryStatusChanged?.Invoke(CreateRepoState.CHANGE_DEFAULT_BRANCH);

				await RenameMasterToMain(repositoryName);
                
				createRepositoryStatusChanged?.Invoke(CreateRepoState.CREATE_SUCCESSFULL);

				return createdRepo;
			}
			catch (Exception e)
			{
				createRepositoryStatusChanged?.Invoke(CreateRepoState.CREATE_STOPPED);
				Debug.LogError(e.Message);
				return null;
			}
		}

		private static async Task GrantAccess(string repositoryName, int teamId, Permission permission)
		{
			RepositoryPermissionRequest permissionRequest = new RepositoryPermissionRequest(permission);
			try
			{
				bool success = await GitHubUtility.GiveRepositoryPermission(teamId, GitHubConstants.Owner, repositoryName, permissionRequest);
				if (success == false)
				{
					// await GrantAccess(repositoryName, teamId, permission);
					Debug.LogError($"Can't grant {permission} permission to the team with id {teamId}");
					bool success2 = await GitHubUtility.GiveRepositoryPermission(teamId, GitHubConstants.Owner, repositoryName, permissionRequest);
					if (success2 == false)
					{
						// await GrantAccess(repositoryName, teamId, permission);
						Debug.LogError($"Can't grant {permission} permission to the team with id {teamId}");
					}
					else
					{
						Debug.Log("Retry was successful");
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
		
		private static async Task RenameMasterToMain(string repositoryName, bool setNewBranchToDefaultBranch = true)
        {
            await RenameBranch(repositoryName, GitHubConstants.MasterBranch, GitHubConstants.MainBranch, setNewBranchToDefaultBranch);
        }
        
		private static async Task RenameBranch(string repositoryName, string oldBranchName, string newBranchName, bool setNewBranchToDefaultBranch)
        {
            string oldBranchRef = GitHubConstants.GetRefBranch(oldBranchName);
            string newBranchRef = GitHubConstants.GetRefBranch(newBranchName);
            
            //Get the latest commit and create a new reference with the new branch name
            var latestCommit = await GitHubUtility.GetLatestCommit(GitHubConstants.Owner, repositoryName, oldBranchName);
            NewReference newRef = new NewReference(newBranchRef, latestCommit.Sha);

            try
            {
                _ = await GitHubUtility.CreateReference(GitHubConstants.Owner, repositoryName, newRef);
            }
            catch
            {
                Debug.Log($"the branch {newBranchName} already exist for the repository {repositoryName}.");
                // If the reference already exist, continue.
            }

            if (setNewBranchToDefaultBranch)
            {
                try
                {
                    _ = await ChangeDefaultBranch(repositoryName, newBranchName);
                }
                catch
                {
                    Debug.LogError($"Couldn't change the default branch from {oldBranchName} to {newBranchName}." +
                                   $"Please verify that you have write access to the repository {repositoryName}.");
                    return;
                }
            }

            try
            {
                await GitHubUtility.DeleteReference(GitHubConstants.Owner, repositoryName, oldBranchRef);
            }
            catch
            {
                Debug.LogError($"Could not delete the old branch {oldBranchName} from the repository {repositoryName}." +
                               $"Please, go to the github website and do it manually.");
            }
        }

		private static async Task<Repository> ChangeDefaultBranch(string repositoryName, string branchName)
        {
	        RepositoryUpdate repositoryUpdate = new RepositoryUpdate(repositoryName)
	        {
		        DefaultBranch = branchName
	        };

	        try
	        {
		        return await GitHubUtility.EditRepository(GitHubConstants.Owner, repositoryName, repositoryUpdate);
	        }
	        catch (Exception e)
	        {
		        Debug.LogError(e.Message);
		        return null;
	        }
        }

        public static void Dispose()
        {
	        createRepositoryStatusChanged = null;
        }
	}
}