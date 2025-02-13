using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Extensions.Linq;
using FunFair.BuildVersion.Detection.LoggingExtensions;
using FunFair.BuildVersion.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using Version = System.Version;

namespace FunFair.BuildVersion.Detection;

public sealed class VersionDetector : IVersionDetector
{
    private static readonly NuGetVersion InitialVersion = new(
        version: new Version(major: 0, minor: 0, build: 0, revision: 0)
    );
    private readonly IBranchClassification _branchClassification;
    private readonly IBranchDiscovery _branchDiscovery;
    private readonly ILogger<VersionDetector> _logger;

    public VersionDetector(
        IBranchDiscovery branchDiscovery,
        IBranchClassification branchClassification,
        ILogger<VersionDetector> logger
    )
    {
        this._branchDiscovery =
            branchDiscovery ?? throw new ArgumentNullException(nameof(branchDiscovery));
        this._branchClassification =
            branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public NuGetVersion FindVersion(Repository repository, int buildNumber)
    {
        string currentBranch = this._branchDiscovery.FindCurrentBranch(repository);
        this._logger.LogCurrentBranch(currentBranch);
        this._logger.LogCurrentBuildNumber(buildNumber);

        if (
            this._branchClassification.IsRelease(
                branchName: currentBranch,
                out NuGetVersion? branchVersion
            )
        )
        {
            return AddBuildNumberToVersion(version: branchVersion, buildNumber: buildNumber);
        }

        NuGetVersion latest = this.DetermineLatestReleaseFromPreviousReleaseBranches(
            repository: repository,
            buildNumber: buildNumber
        );

        this._logger.LogLatestReleaseVersion(latest);

        return this.BuildPreReleaseVersion(
            latest: latest,
            currentBranch: currentBranch,
            buildNumber: buildNumber
        );
    }

    private NuGetVersion DetermineLatestReleaseFromPreviousReleaseBranches(
        Repository repository,
        int buildNumber
    )
    {
        IReadOnlyList<string> branches = this._branchDiscovery.FindBranches(repository);

        NuGetVersion? latestVersion = this.FindLatestReleaseVersion(branches);

        return AddBuildNumberToVersion(latestVersion ?? InitialVersion, buildNumber: buildNumber);
    }

    private NuGetVersion? FindLatestReleaseVersion(IReadOnlyList<string> branches)
    {
        return branches.Select(GetReleaseVersion).RemoveNulls().Max();

        NuGetVersion? GetReleaseVersion(string branch)
        {
            this._logger.LogFoundBranch(branch);

            return this._branchClassification.IsRelease(
                branchName: branch,
                out NuGetVersion? version
            )
                ? version
                : null;
        }
    }

    private static NuGetVersion AddBuildNumberToVersion(NuGetVersion version, int buildNumber)
    {
        Version dv = new(
            major: version.Version.Major,
            minor: version.Version.Minor,
            build: version.Version.Build,
            revision: buildNumber
        );

        return new(dv);
    }

    private NuGetVersion BuildPreReleaseVersion(
        NuGetVersion latest,
        string currentBranch,
        int buildNumber
    )
    {
        string usedSuffix = this.BuildPreReleaseSuffix(currentBranch: currentBranch);

        this._logger.LogPreReleaseSuffix(usedSuffix);

        Version version = new(
            major: latest.Version.Major,
            minor: latest.Version.Minor,
            latest.Version.Build + 1,
            revision: buildNumber
        );

        return new(version: version, releaseLabel: usedSuffix);
    }

    private string BuildPreReleaseSuffix(string currentBranch)
    {
        return currentBranch
            .NormalizeSourceBranchName(this._branchClassification)
            .RemoveFirstFolderInBranchName()
            .ReplaceInvalidCharacters()
            .RemoveDoubleHyphens()
            .RemoveLeadingDigits()
            .EnsureNotBlank()
            .EnsureNotTooLong();
    }
}
