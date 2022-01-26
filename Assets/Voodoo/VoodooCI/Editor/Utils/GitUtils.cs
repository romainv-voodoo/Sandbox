using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Voodoo.CI
{
    public class GitException : InvalidOperationException
    {
        public GitException(int exitCode, string errors) : base(errors) => _exitCode = exitCode;
        private readonly int _exitCode;
    }

    public static class GitUtils
    {
        /* Properties ============================================================================================================= */

        /// <summary>
        /// Retrieves the build version from git based on the most recent matching tag and
        /// commit history. This returns the version as: {major.minor.build} where 'build'
        /// represents the nth commit after the tagged commit.    
        /// </summary>
        public static string BuildVersion
        {
            get
            {
                // Get latest tag string descriptor
                var gitDescribeOutput = Run(@"describe --tags --long");
                if (buildVersionIsGitLogBased)
                {
                    // Extract tag
                    var tag = gitDescribeOutput.Substring(0, gitDescribeOutput.IndexOf('-'));

                    // Feed tag into log command
                    var gitLogOutput = Run($"log --ancestry-path {tag}..HEAD --pretty=format:%h");

                    // Count the number of lines in git log output. 
                    var commitCount = 0;

                    if (!string.IsNullOrEmpty(gitLogOutput))
                    {
                        var newLine = Environment.NewLine;
                        var newLineLength = newLine.Length;

                        commitCount =
                            (gitLogOutput.Length - gitLogOutput.Replace(Environment.NewLine, string.Empty).Length) /
                            newLineLength + 1;
                    }

                    var version = $"{tag}.{commitCount}";

                    return version;
                }
                // Only viable when building from the tagged branch or if the branch that the tagged branch
                // has been merged to has no commits prior to the tag's application.
                else
                {
                    var version = gitDescribeOutput.Replace('-', '.');
                    version = version.Substring(0, version.LastIndexOf('.') - 1);
                    return version;
                }
            }
        }

        /// <summary>
        /// The currently active branch.
        /// </summary>
        public static string Branch => Run(@"rev-parse --abbrev-ref HEAD");

        /// <summary>
        /// Returns a listing of all uncommitted or untracked (added) files.
        /// </summary>
        public static string Status => Run(@"status --porcelain");

        /// <summary>
        /// Flag that determines how BuildVersion is retrieved:
        ///   false - only 'git describe' is used.
        ///           This approach has issues when called from a branch that has the tagged branch merged into to.
        ///
        ///   true - 'git describe' is used to fetch the current tag and then 'git log' is used to retrieve relevant commits.
        /// </summary>
        public static bool buildVersionIsGitLogBased = true;


        /* Methods ================================================================================================================ */

        /// <summary>
        /// Runs git.exe with the specified arguments and returns the output.
        /// </summary>
        private static string Run(string arguments)
        {
            Debug.Log($"Running git command with arg: {arguments}");
            using (var process = new System.Diagnostics.Process())
            {
                var exitCode = process.Run(@"git", arguments, Application.dataPath,
                    out var output, out var errors);
                if (exitCode == 0)
                {
                    return output;
                }
                else
                {
                    Debug.LogError("There is no tags assigned to the git repo. Please tag a commit and push the tag!");
                    throw new GitException(exitCode, errors);
                }
            }
        }

        public static string GetRepoName()
        {
            var gitUrl = Run("config --get remote.origin.url");
            Regex regex = new Regex(".*VoodooTeam/(.*).git");
            var repoName = regex.Match(gitUrl);
            Debug.Log($"Repo Name: {repoName.Groups[1]}");
            return repoName.Groups[1].ToString();
        }

        public static string GetBranchName()
        {
            var branchName = Run("rev-parse --abbrev-ref HEAD");
            return branchName;
        }

        public static string GetShortHash()
        {
            var shortHash = Run("rev-parse --short HEAD");
            return shortHash;
        }

        public static string GetGitStatus()
        {
            var gitStatus = Run("status");
            return gitStatus;
        }
    }
}