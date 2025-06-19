using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Extensions.Linq;
using FunFair.BuildVersion.Detection.LoggingExtensions;
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

    public GitBranchDiscovery(IBranchClassification branchClassification, IEnumerable<IExternalBranchLocator> externalBranchLocators, ILogger<GitBranchDiscovery> logger)
    {
        this._branchClassification = branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
        this._externalBranchLocators = externalBranchLocators ?? throw new ArgumentNullException(nameof(externalBranchLocators));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string FindCurrentBranch(Repository repository)
    {
        string branch = this.FindConfiguredBranch(repository);
        string sha = GetHeadSha(repository);
        this._logger.LogHeadSha(sha);

        if (!this._branchClassification.IsPullRequest(currentBranch: branch, out long pullRequestId))
        {
            return branch;
        }

        this._logger.LogPullRequest(pullRequestId);

        Branch? candidateBranch = repository.Branches.FirstOrDefault(IsMatchingPullRequestBranch);

        if (candidateBranch is null)
        {
            return branch;
        }

        this._logger.LogFoundBranchForPullRequest(pullRequestId: pullRequestId, branch: candidateBranch.FriendlyName);

        return this._branchClassification.IsRelease(branchName: candidateBranch.FriendlyName, out NuGetVersion? _)
            ? "pre-" + candidateBranch.FriendlyName
            : candidateBranch.FriendlyName;

        bool IsMatchingPullRequestBranch(Branch candidateBranch1)
        {
            return !StringComparer.Ordinal.Equals(x: candidateBranch1.FriendlyName, y: branch) && StringComparer.Ordinal.Equals(x: candidateBranch1.Tip.Sha, y: sha);
        }
    }

    public IReadOnlyList<string> FindBranches(Repository repository)
    {
        return
        [
            .. repository.Branches.Select(selector: b => ExtractBranch(repository: repository, branch: b.FriendlyName)),
        ];
    }

    private static string GetHeadSha(Repository repository)
    {
        return repository.Head.Tip.Sha;
    }

    private static string ExtractBranch(Repository repository, string branch)
    {
        IReadOnlyList<string> remotes = GetRemotes(repository);

        string? remote = remotes.FirstOrDefault(remote => branch.StartsWith(value: remote, comparisonType: StringComparison.OrdinalIgnoreCase));

        return remote is not null
            ? branch[remote.Length..]
            : branch;
    }

    private static IReadOnlyList<string> GetRemotes(Repository repository)
    {
        return [.. repository.Network.Remotes.Select(static r => r.Name + "/")];
    }

    private string FindConfiguredBranch(Repository repository)
    {
        return this.FindConfiguredBranchUsingExternalLocators() ?? ExtractBranchFromGitHead(repository);
    }

    private string? FindConfiguredBranchUsingExternalLocators()
    {
        return this._externalBranchLocators.Select(static locator => locator.CurrentBranch)
                   .RemoveNulls()
                   .FirstOrDefault();
    }

    private static string ExtractBranchFromGitHead(Repository repository)
    {
        return repository.Head.FriendlyName;
    }
}