using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Extensions.Linq;
using FunFair.BuildVersion.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection;

public sealed class VersionDetector : IVersionDetector
{
    private static readonly NuGetVersion InitialVersion = new(version: new Version(major: 0, minor: 0, build: 0, revision: 0));
    private readonly IBranchClassification _branchClassification;
    private readonly IBranchDiscovery _branchDiscovery;
    private readonly ILogger<VersionDetector> _logger;

    public VersionDetector(IBranchDiscovery branchDiscovery, IBranchClassification branchClassification, ILogger<VersionDetector> logger)
    {
        this._branchDiscovery = branchDiscovery ?? throw new ArgumentNullException(nameof(branchDiscovery));
        this._branchClassification = branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public NuGetVersion FindVersion(int buildNumber)
    {
        string currentBranch = this._branchDiscovery.FindCurrentBranch();
        this._logger.LogInformation($">>>>>> Current branch: {currentBranch}");
        this._logger.LogInformation($">>>>>> Current Build number: {buildNumber}");

        if (this._branchClassification.IsRelease(branchName: currentBranch, out NuGetVersion? branchVersion))
        {
            return AddBuildNumberToVersion(version: branchVersion, buildNumber: buildNumber);
        }

        NuGetVersion latest = this.DetermineLatestReleaseFromPreviousReleaseBranches(buildNumber: buildNumber);

        this._logger.LogInformation($"Latest Release Version: {latest}");

        return this.BuildPreReleaseVersion(latest: latest, currentBranch: currentBranch, buildNumber: buildNumber);
    }

    private NuGetVersion DetermineLatestReleaseFromPreviousReleaseBranches(int buildNumber)
    {
        NuGetVersion? GetReleaseVersion(string branch)
        {
            this._logger.LogDebug($" * => {branch}");

            return this._branchClassification.IsRelease(branchName: branch, out NuGetVersion? version)
                ? version
                : null;
        }

        IReadOnlyList<string> branches = this._branchDiscovery.FindBranches();

        NuGetVersion? latestVersion = branches.Select(GetReleaseVersion)
                                              .RemoveNulls()
                                              .Max();

        return AddBuildNumberToVersion(latestVersion ?? InitialVersion, buildNumber: buildNumber);
    }

    private static NuGetVersion AddBuildNumberToVersion(NuGetVersion version, int buildNumber)
    {
        Version dv = new(revision: buildNumber, build: version.Version.Build, minor: version.Version.Minor, major: version.Version.Major);

        return new(dv);
    }

    private NuGetVersion BuildPreReleaseVersion(NuGetVersion latest, string currentBranch, int buildNumber)
    {
        string usedSuffix = this.BuildPreReleaseSuffix(currentBranch: currentBranch);

        this._logger.LogInformation($"Build Pre-Release Suffix: {usedSuffix}");

        Version version = new(major: latest.Version.Major, minor: latest.Version.Minor, latest.Version.Build + 1, revision: buildNumber);

        return new(version: version, releaseLabel: usedSuffix);
    }

    private string BuildPreReleaseSuffix(string currentBranch)
    {
        return currentBranch.NormalizeSourceBranchName(this._branchClassification)
                            .RemoveFirstFolderInBranchName()
                            .ReplaceInvalidCharacters()
                            .RemoveDoubleHyphens()
                            .RemoveLeadingDigits()
                            .EnsureNotBlank()
                            .EnsureNotTooLong();
    }
}