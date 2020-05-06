using System;
using LibGit2Sharp;

namespace FunFair.BuildVersion
{
    internal static class BranchDiscovery
    {
        public static string FindCurrentBranch(Repository repo)
        {
            string branch = FindConfiguredBranch(repo);
            var sha = repo.Head.Tip.Sha;
            Console.WriteLine($"Head SHA: {sha}");

            if (!PullRequest.ExtractPullRequestId(currentBranch: branch, out long pullRequestId))
            {
                return branch;
            }

            Console.WriteLine($"Pull Request: {pullRequestId}");

            foreach (var candidateBranch in repo.Branches)
            {
                if (candidateBranch.FriendlyName != branch && candidateBranch.Tip.Sha == sha)
                {
                    Console.WriteLine($"Found Branch for PR {pullRequestId} : candidateBranch.FriendlyName");

                    return candidateBranch.FriendlyName;
                }
            }

            return branch;
        }

        private static string FindConfiguredBranch(Repository repo)
        {
            string? branch = Environment.GetEnvironmentVariable(variable: @"GIT_BRANCH");

            if (!string.IsNullOrWhiteSpace(branch))
            {
                return ExtractBranchFromBranchSpec(branch);
            }

            branch = Environment.GetEnvironmentVariable(variable: @"GITHUB_REF");

            if (!string.IsNullOrWhiteSpace(branch))
            {
                return ExtractBranchFromBranchSpec(branch);
            }

            return ExtractBranchFromGitHead(repo);
        }

        private static string ExtractBranchFromBranchSpec(string branch)
        {
            Console.WriteLine($"Branch from CI: {branch}");
            string branchRef = branch.Trim();

            const string branchRefPrefix = "refs/heads/";

            if (branchRef.StartsWith(value: branchRefPrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                branchRef = branchRef.Substring(branchRefPrefix.Length);
            }

            return branchRef;
        }

        private static string ExtractBranchFromGitHead(Repository repository)
        {
            return repository.Head.FriendlyName;
        }
    }
}