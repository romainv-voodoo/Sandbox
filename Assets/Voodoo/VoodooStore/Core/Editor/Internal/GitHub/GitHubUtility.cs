using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Voodoo.Store
{
	public static class GitHubUtility
	{
		private static GitHubClient client;
		private static string currentToken;
		
		private static GitHubClient Client
		{
			get
			{
				if (client == null || currentToken != User.signInToken)
				{
					SetupClient();
				}

				return client;
			}
		}

		public static string ClientName => Client.User.Current().Result.Name;

		private static void SetupClient()
		{
			ProductHeaderValue productInformation = new ProductHeaderValue("VoodooUser");
			if (string.IsNullOrEmpty(User.signInToken))
			{
				return;
			}

			currentToken = User.signInToken;
			Credentials credentials = new Credentials(currentToken);
			client = new GitHubClient(productInformation) { Credentials = credentials };
		}

		/// <summary>Is the user Authorized</summary>
		/// <returns></returns>
		public static async Task<bool> IsUserAuthorized() 
		{
			try 
			{
				_ = await GetRepository(GitHubConstants.Owner, GitHubConstants.StoreRepository);
			}
			catch
			{
				return false;
			}
            
			return true;
		}

		/// <summary>Gets whether or not the given repository is managed by the given team.</summary>
		/// <param name="teamId">The team identifier.</param>
		/// <param name="owner">Org to associate the repo with.</param>
		/// <param name="repositoryName">Name of the repo.</param>
		/// <returns><see langword="true"/> if the repository is managed by the given team; <see langword="false"/> otherwise.</returns>
		public static async Task<bool> IsRepositoryManagedByTeam(int teamId, string owner, string repositoryName)
		{
			return await Client.Organization.Team.IsRepositoryManagedByTeam(teamId, owner, repositoryName);
		}

		/// <summary>Add a repository to the team</summary>
		/// <param name="teamId">The team identifier.</param>
		/// <param name="owner">Org to associate the repo with.</param>
		/// <param name="repositoryName">Name of the repo.</param>
		/// <param name="permissionRequest">The permission to grant the team on this repository.</param>
		/// <returns><see langword="true"/> if the repository was added successfully; <see langword="false"/> otherwise.</returns>
		public static async Task<bool> GiveRepositoryPermission(int teamId, string owner, string repositoryName, RepositoryPermissionRequest permissionRequest)
		{
			return await Client.Organization.Team.AddRepository(teamId, owner, repositoryName, permissionRequest);
		}

		/// <summary>Remove a repository from the team</summary>
		/// <param name="teamId">The team identifier.</param>
		/// <param name="owner">Org to associate the repo with.</param>
		/// <param name="repositoryName">Name of the repo.</param>
		/// <returns><see langword="true"/> if the repository was removed successfully; <see langword="false"/> otherwise.</returns>
		public static async Task<bool> RemoveRepositoryPermission(int teamId, string owner, string repositoryName)
		{
			return await Client.Organization.Team.RemoveRepository(teamId, owner, repositoryName);
		}

		/// <summary>Gets a single commit for a given repository</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="branchName">The branch name or ref</param>
		public static async Task<GitHubCommit> GetLatestCommit(string owner, string repositoryName, string branchName)
		{
			return await Client.Repository.Commit.Get(owner, repositoryName, branchName);
		}

		/// <summary>Creates a reference for a given repository</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="newRef">The reference to create</param>
		/// <remarks>
		/// The reference parameter supports either the fully-qualified ref
		/// (prefixed with  "refs/", e.g. "refs/heads/master" or
		/// "refs/tags/release-1") or the shortened form (omitting "refs/", e.g.
		/// "heads/master" or "tags/release-1")
		/// </remarks>
		public static async Task<Reference> CreateReference(string owner, string repositoryName, NewReference newRef)
		{
			return await Client.Git.Reference.Create(owner, repositoryName, newRef);
		}
		
		/// <summary>
		/// Deletes a reference for a given repository by reference name
		/// </summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="branchRef">The reference name</param>
		/// <remarks>
		/// The reference parameter supports either the fully-qualified ref
		/// (prefixed with  "refs/", e.g. "refs/heads/master" or
		/// "refs/tags/release-1") or the shortened form (omitting "refs/", e.g.
		/// "heads/master" or "tags/release-1")
		/// </remarks>
		public static async Task DeleteReference(string owner, string repositoryName, string branchRef)
		{
			await Client.Git.Reference.Delete(owner, repositoryName, branchRef);
		}

		/// <summary>Creates a new repository in the specified organization.</summary>
		/// <param name="owner">Login of the organization in which to create the repository</param>
		/// <param name="newRepository">A <see cref="T:Octokit.NewRepository" /> instance describing the new repository to create</param>
		/// <returns>A <see cref="T:Octokit.Repository" /> instance for the created repository</returns>
		public static async Task<Repository> CreateRepository(string owner, NewRepository newRepository)
		{
			return await Client.Repository.Create(owner, newRepository);
		}
		
		/// <summary>Gets all repositories owned by the current user.</summary>
		public static async Task<IReadOnlyList<Repository>> GetAllRepositories()
		{
			return await Client.Repository.GetAllForCurrent();
		}
		
		/// <summary>Gets all repositories owned by the current user.</summary>
		/// <param name="request">Search parameters to filter results on</param>
		public static async Task<IReadOnlyList<Repository>> GetAllRepositories(RepositoryRequest request)
		{
			return await Client.Repository.GetAllForCurrent(request);
		}
		
		/// <summary>Gets the specified repository.</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		public static async Task<Repository> GetRepository(string owner, string repositoryName)
		{
			return await Client.Repository.Get(owner, repositoryName);
		}

		/// <summary>
		/// Updates the specified repository with the values given in <paramref name="repositoryUpdate" />
		/// </summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="repositoryUpdate">New values to update the repository with</param>
		public static async Task<Repository> EditRepository(string owner, string repositoryName, RepositoryUpdate repositoryUpdate)
		{
			return await Client.Repository.Edit(owner, repositoryName, repositoryUpdate);
		}
		
		/// <summary>
		/// Gets all <see cref="T:Octokit.Release" />s for the specified repository.
		/// </summary>
		/// <param name="owner">The repository's owner</param>
		/// <param name="repositoryName">The repository's name</param>
		private static async Task<IReadOnlyList<Release>> GetAllReleases(string owner, string repositoryName)
		{
			return await Client.Repository.Release.GetAll(owner, repositoryName);;
		}

		/// <summary>Gets all tags for the specified repository.</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <returns>All of the repositories tags.</returns>
		public static async Task<IReadOnlyList<RepositoryTag>> GetAllTags(string owner, string repositoryName)
		{
			return await Client.Repository.GetAllTags(owner, repositoryName);
		}

		/// <summary>
		/// Get an archive of a given repository's contents, using a specific format and reference
		/// </summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="archiveFormat">The format of the archive. Can be either tarball or zipball</param>
		/// <param name="reference">A valid Git reference.</param>
		public static async Task<byte[]> GetArchive(string owner, string repositoryName, ArchiveFormat archiveFormat, string reference)
		{
			return await Client.Repository.Content.GetArchive(owner, repositoryName, archiveFormat, reference);
		}

		/// <summary>Gets a Tree Response for a given SHA.</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="reference">The SHA that references the tree</param>
		/// <param name="recursive">Should the action be done recursively</param>
		public static async Task<TreeResponse> GetTreeResponse(string owner, string repositoryName, string reference, bool recursive)
		{
			if (recursive)
			{
				return await Client.Git.Tree.GetRecursive(owner, repositoryName, reference);
			}
			
			return await Client.Git.Tree.Get(owner, repositoryName, reference);
		}

		/// <summary>Create a tag for a given repository</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="newTag">The tag to create</param>
		public static async Task<GitTag> CreateTag(string owner, string repositoryName, NewTag newTag)
		{
			return await Client.Git.Tag.Create(owner, repositoryName, newTag);
		}

		/// <summary>Gets a single Blob by SHA.</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="sha">The SHA of the blob</param>
		public static async Task<Blob> GetBlob(string owner, string repositoryName, string sha)
		{
			return await Client.Git.Blob.Get(owner, repositoryName, sha);
		}

		/// <summary>Creates a new Tree in the specified repo</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="newTree">The value of the new tree</param>
		public static async Task<TreeResponse> CreateTree(string owner, string repositoryName, NewTree newTree)
		{
			return await Client.Git.Tree.Create(owner, repositoryName, newTree);
		}

		/// <summary>Create a commit for a given repository</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="newCommit">The commit to create</param>
		public static async Task<Commit> CreateCommit(string owner, string repositoryName, NewCommit newCommit)
		{
			return await Client.Git.Commit.Create(owner, repositoryName, newCommit);
		}

		/// <summary>Compare two references in a repository</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="firstCommitSha">The reference to use as the base commit</param>
		/// <param name="secondCommitSha">The reference to use as the head commit</param>
		public static async Task<CompareResult> CompareCommit(string owner, string repositoryName, string firstCommitSha, string secondCommitSha)
		{
			return await Client.Repository.Commit.Compare(owner, repositoryName, firstCommitSha, secondCommitSha);
		}

		/// <summary>
		/// Updates a reference for a given repository by reference name
		/// </summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="reference">The reference name</param>
		/// <param name="referenceUpdate">The updated reference data</param>
		/// <remarks>
		/// The reference parameter supports either the fully-qualified ref
		/// (prefixed with  "refs/", e.g. "refs/heads/master" or
		/// "refs/tags/release-1") or the shortened form (omitting "refs/", e.g.
		/// "heads/master" or "tags/release-1")
		/// </remarks>
		public static async Task<Reference> UpdateReference(string owner, string repositoryName, string reference, ReferenceUpdate referenceUpdate)
		{
			return await Client.Git.Reference.Update(owner, repositoryName, reference, referenceUpdate);
		}
		
		/// <summary>Creates a new Blob</summary>
		/// <param name="owner">The owner of the repository</param>
		/// <param name="repositoryName">The name of the repository</param>
		/// <param name="newBlob">The new Blob</param>
		public static async Task<BlobReference> CreateBlobReference(string owner, string repositoryName, NewBlob newBlob)
		{
			return await Client.Git.Blob.Create(owner, repositoryName, newBlob);
		}

		/// <summary>
		/// Returns all <see cref="T:Octokit.Team" />s for the current user.
		/// </summary>
		public static async Task<IReadOnlyList<Team>> GetUserTeams()
		{
			return await Client.Organization.Team.GetAllForCurrent();
		}
	}
}