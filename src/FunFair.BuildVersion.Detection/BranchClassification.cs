using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FunFair.BuildVersion.Interfaces;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection;

public sealed class BranchClassification : IBranchClassification
{
    private const string PULL_REQUEST_PREFIX = "refs/pull/";
    private const string PULL_REQUEST_SUFFIX = "/head";
    private readonly string _hotfixBranch;

    private readonly string _releaseBranch;

    public BranchClassification(IBranchSettings branchSettings)
    {
        this._releaseBranch = BuildBranch(branchSettings: branchSettings, branch: "release");
        this._hotfixBranch = BuildBranch(branchSettings: branchSettings, branch: "hotfix");
    }

    public bool IsRelease(string branchName, [NotNullWhen(true)] out NuGetVersion? version)
    {
        version =
            Extract(prefix: this._releaseBranch, branch: branchName)
            ?? Extract(prefix: this._hotfixBranch, branch: branchName);

        return version is not null;
    }

    public bool IsPullRequest(string currentBranch, out long pullRequestId)
    {
        if (currentBranch.StartsWith(value: PULL_REQUEST_PREFIX, comparisonType: StringComparison.Ordinal))
        {
            currentBranch = currentBranch[PULL_REQUEST_PREFIX.Length..];

            if (currentBranch.EndsWith(value: PULL_REQUEST_SUFFIX, comparisonType: StringComparison.Ordinal))
            {
                currentBranch = currentBranch[..^PULL_REQUEST_SUFFIX.Length];
            }

            return long.TryParse(
                s: currentBranch,
                style: NumberStyles.Integer,
                provider: CultureInfo.InvariantCulture,
                result: out pullRequestId
            );
        }

        pullRequestId = default;

        return false;
    }

    private static string BuildBranch(IBranchSettings branchSettings, string branch)
    {
        return string.Join(separator: '/', BuildFragments(branchSettings: branchSettings, branch: branch))
            .ToLowerInvariant();
    }

    private static IEnumerable<string> BuildFragments(IBranchSettings branchSettings, string branch)
    {
        yield return BuildReleaseBranchWithSuffix(branchSettings: branchSettings, branch: branch);

        if (!string.IsNullOrWhiteSpace(branchSettings.Package))
        {
            yield return branchSettings.Package;
        }

        // deliberately end with a trailing /
        yield return string.Empty;
    }

    private static string BuildReleaseBranchWithSuffix(IBranchSettings branchSettings, string branch)
    {
        if (string.IsNullOrWhiteSpace(branchSettings.ReleaseSuffix))
        {
            return branch;
        }

        return string.Concat(str0: branch, str1: "-", str2: branchSettings.ReleaseSuffix);
    }

    private static NuGetVersion? Extract(string prefix, string branch)
    {
        if (!branch.StartsWith(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string version = branch[prefix.Length..];

        if (!NuGetVersion.TryParse(value: version, out NuGetVersion? baseLine))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(baseLine.Release))
        {
            return null;
        }

        if (IsVersionSameAsBranchName(baseLine: baseLine, version: version))
        {
            // can't have 4 parts of a version ion the branch
            return null;
        }

        return ConvertVersion(baseLine);
    }

    private static NuGetVersion ConvertVersion(NuGetVersion baseLine)
    {
        Version dv = new(
            major: baseLine.Version.Major,
            minor: baseLine.Version.Minor,
            build: baseLine.Version.Build,
            revision: 0
        );

        return new(dv);
    }

    private static bool IsVersionSameAsBranchName(NuGetVersion baseLine, string version)
    {
        Version revision = new(
            major: baseLine.Version.Major,
            minor: baseLine.Version.Minor,
            build: baseLine.Version.Build,
            revision: baseLine.Version.Revision
        );

        return StringComparer.Ordinal.Equals(revision.ToString(), y: version);
    }
}
