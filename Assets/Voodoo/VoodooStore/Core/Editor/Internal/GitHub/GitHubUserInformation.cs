using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using UnityEngine;

namespace Voodoo.Store
{
    public static class GitHubUserInformation
    {
        public static async Task<bool> IsUserInTeam(int teamId)
        {
            try
            {
                IReadOnlyList<Team> teams = await GitHubUtility.GetUserTeams();
                return teams.FirstOrDefault(x => teamId == (x.Id))!=null;
            }
            catch (Exception e)
            {
                Debug.Log($"Can't find user. {e})");
                throw;
            }
        }
        
        public static async Task<bool> IsUserInTeams(List<int> teamsId)
        {
            try
            {
                IReadOnlyList<Team> teams = await GitHubUtility.GetUserTeams();
                return teams.FirstOrDefault(x => teamsId.Contains(x.Id))!=null;
            }
            catch (Exception e)
            {
                Debug.Log($"Can't find user. {e}");
                throw;
            }
        }
    }
}