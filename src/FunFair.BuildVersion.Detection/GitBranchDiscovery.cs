using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Extensions.Linq;
using FunFair.BuildVersion.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection;

public sealed class GitBranchDiscovery : IBranchDiscovery
{
    private readonly IBranchClassification _branchClassification;
    private readonly IEnumerable<IExternalBranchLocator> _externalBranchLocators;
    private readonly ILogger<GitBranchDiscovery> _logger;
    private readonly Repository _repository;

    public GitBranchDiscovery(Repository repository, IBranchClassification branchClassification, IEnumerable<IExternalBranchLocator> externalBranchLocators, ILogger<GitBranchDiscovery> logger)
    {
        this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this._branchClassification = branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
        this._externalBranchLocators = externalBranchLocators ?? throw new ArgumentNullException(nameof(externalBranchLocators));
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
            if (!StringComparer.Ordinal.Equals(candidateBranch.FriendlyName, branch) && StringComparer.Ordinal.Equals(candidateBranch.Tip.Sha, sha))
            {
                this._logger.LogInformation($"Found Branch for PR {pullRequestId} : {candidateBranch.FriendlyName}");

                return this._branchClassification.IsRelease(branchName: candidateBranch.FriendlyName, out NuGetVersion? _)
                    ? "pre-" + candidateBranch.FriendlyName
                    : candidateBranch.FriendlyName;
            }
        }

        return branch;
    }

    public IReadOnlyList<string> FindBranches()
    {
        return this._repository.Branches.Select(selector: b => this.ExtractBranch(b.FriendlyName))
                   .ToArray();
    }

    private string ExtractBranch(string branch)
    {
        IReadOnlyList<string> remotes = this._repository.Network.Remotes.Select(r => r.Name + "/")
                                            .ToArray();

        string? remote = remotes.FirstOrDefault(remote => branch.StartsWith(value: remote, comparisonType: StringComparison.OrdinalIgnoreCase));

        if (remote is not null)
        {
            return branch.Substring(remote.Length);
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