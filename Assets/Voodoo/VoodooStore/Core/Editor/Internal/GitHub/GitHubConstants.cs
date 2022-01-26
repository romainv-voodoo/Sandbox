using System.Collections.Generic;

namespace Voodoo.Store
{
	public static class GitHubConstants
	{
		//Organization
		public const string Owner                   = "VoodooTeam";
		
		// Teams
		public const int GamingEngineeringTeamId    = 2970071;
		public const int VoodooStoreTeamId          = 4675998;
		public const int GameTeamId                 = 2918769;
		public const int LabTeamId                  = 2820020;
		public const int MarketingGameDevTeamId     = 3302707;
		public const int PublishingOperationsTeamId = 4198475;
		public const int CasualEngineeringTeamId    = 4706511;

		public static readonly List<int> GameOpsTeamID = new List<int>
		{
			3227200, //GameOps
			3240475, //GameOps_Artists
			3285632  //GameOps_Devs"
		};
		
		// Repositories
		public const string StoreRepository         = "VoodooStore";
		public const string StoreRepository2        = "VST_Store";
		
		// Branch
		public const string MasterBranch            = "master";
		public const string MainBranch              = "main";
		
		//Readme branch
		public const string ReadmeBranch            = "feature/Readme_Without_Prefix";
		
		public static string GetRefBranch(string branchName) => $"refs/heads/{branchName}";
	}
}