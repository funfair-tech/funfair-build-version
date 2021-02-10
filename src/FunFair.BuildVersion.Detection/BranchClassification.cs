using System;
using System.Diagnostics.CodeAnalysis;
using FunFair.BuildVersion.Interfaces;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection
{
    /// <summary>
    ///     Branch Classification
    /// </summary>
    public sealed class BranchClassification : IBranchClassification
    {
        private const string RELEASE_PREFIX = @"release/";
        private const string HOTFIX_PREFIX = @"hotfix/";

        private const string PULL_REQUEST_PREFIX = @"refs/pull/";
        private const string PULL_REQUEST_SUFFIX = @"/head";

        /// <inheritdoc />
        public bool IsReleaseBranch(string branchName, [NotNullWhen(true)] out NuGetVersion? version)
        {
            version = Extract(prefix: RELEASE_PREFIX, branch: branchName) ?? Extract(prefix: HOTFIX_PREFIX, branch: branchName);

            return version != null;
        }

        /// <inheritdoc />
        public bool IsPullRequest(string currentBranch, out long pullRequestId)
        {
            if (currentBranch.StartsWith(value: PULL_REQUEST_PREFIX, comparisonType: StringComparison.Ordinal))
            {
                currentBranch = currentBranch.Substring(PULL_REQUEST_PREFIX.Length);

                if (currentBranch.EndsWith(value: PULL_REQUEST_SUFFIX, comparisonType: StringComparison.Ordinal))
                {
                    currentBranch = currentBranch.Substring(startIndex: 0, currentBranch.Length - PULL_REQUEST_SUFFIX.Length);
                }

                return long.TryParse(s: currentBranch, result: out pullRequestId);
            }

            pullRequestId = default;

            return false;
        }

        private static NuGetVersion? Extract(string prefix, string branch)
        {
            if (!branch.StartsWith(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string version = branch.Substring(prefix.Length);

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
            Version dv = new(revision: 0, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);

            return new NuGetVersion(dv);
        }

        private static bool IsVersionSameAsBranchName(NuGetVersion baseLine, string version)
        {
            Version revision = new(revision: baseLine.Version.Revision, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);
            bool sameAsBranchName = revision.ToString() == version;

            return sameAsBranchName;
        }
    }
}