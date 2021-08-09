using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.BuildVersion.Detection.Extensions;
using FunFair.BuildVersion.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection
{
    /// <summary>
    ///     Branch discovery using Git as the repo.
    /// </summary>
    public sealed class GitBranchDiscovery : IBranchDiscovery
    {
        private readonly IBranchClassification _branchClassification;
        private readonly IEnumerable<IExternalBranchLocator> _externalBranchLocators;
        private readonly ILogger<GitBranchDiscovery> _logger;
        private readonly Repository _repository;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="repository">The Git Repository to search.</param>
        /// <param name="branchClassification">Branch classification.</param>
        /// <param name="externalBranchLocators">Branch location.</param>
        /// <param name="logger">Logging.</param>
        public GitBranchDiscovery(Repository repository,
                                  IBranchClassification branchClassification,
                                  IEnumerable<IExternalBranchLocator> externalBranchLocators,
                                  ILogger<GitBranchDiscovery> logger)
        {
            this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this._branchClassification = branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
            this._externalBranchLocators = externalBranchLocators ?? throw new ArgumentNullException(nameof(externalBranchLocators));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IReadOnlyList<string> FindBranches()
        {
            return this._repository.Branches.Select(selector: b => this.ExtractBranch(b.FriendlyName))
                       .ToArray();
        }

        private string ExtractBranch(string branch)
        {
            IReadOnlyList<string> remotes = this._repository.Network.Remotes.Select(r => r.Name + "/")
                                                .ToArray();

            foreach (string remote in remotes)
            {
                if (branch.StartsWith(value: remote, comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    return branch.Substring(remote.Length);
                }
            }

            return branch;
        }

        private string FindConfiguredBranch()
        {
            return this.FindConfiguredBranchUsingExternalLocators() ?? this.ExtractBranchFromGitHead();
        }

        private string? FindConfiguredBranchUsingExternalLocators()
        {
            return this._externalBranchLocators.Select(locator => locator.CurrentBranch)
                       .RemoveNulls()
                       .FirstOrDefault();
        }

        private string ExtractBranchFromGitHead()
        {
            return this._repository.Head.FriendlyName;
        }
    }
}