using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.BuildVersion.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection
{
    public sealed class GitBranchDiscovery : IBranchDiscovery
    {
        private readonly IBranchClassification _branchClassification;
        private readonly ILogger<GitBranchDiscovery> _logger;
        private readonly Repository _repository;

        public GitBranchDiscovery(Repository repository, IBranchClassification branchClassification, ILogger<GitBranchDiscovery> logger)
        {
            this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this._branchClassification = branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string FindCurrentBranch()
        {
            string branch = this.FindConfiguredBranch();
            string? sha = this._repository.Head.Tip.Sha;
            this._logger.LogInformation($"Head SHA: {sha}");

            if (!this._branchClassification.IsPullRequest(currentBranch: branch, out long pullRequestId))
            {
                return branch;
            }

            this._logger.LogInformation($"Pull Request: {pullRequestId}");

            foreach (Branch candidateBranch in this._repository.Branches)
            {
                if (candidateBranch.FriendlyName != branch && candidateBranch.Tip.Sha == sha)
                {
                    this._logger.LogInformation($"Found Branch for PR {pullRequestId} : {candidateBranch.FriendlyName}");

                    if (this._branchClassification.IsRelease(branchName: candidateBranch.FriendlyName, out NuGetVersion? _))
                    {
                        return "pre-" + candidateBranch.FriendlyName;
                    }

                    return candidateBranch.FriendlyName;
                }
            }

            return branch;
        }

        public IReadOnlyList<string> FindBranches()
        {
            return this._repository.Branches.Select(selector: b => ExtractBranch(b.FriendlyName))
                       .ToArray();
        }

        private static string ExtractBranch(string branch)
        {
            const string originPrefix = "origin/";

            if (branch.StartsWith(value: originPrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return branch.Substring(originPrefix.Length);
            }

            return branch;
        }

        private string FindConfiguredBranch()
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

            return this.ExtractBranchFromGitHead();
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

        private string ExtractBranchFromGitHead()
        {
            return this._repository.Head.FriendlyName;
        }
    }
}