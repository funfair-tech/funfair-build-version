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

        /// <inheritdoc />
        public bool IsReleaseBranch(string branchName, [NotNullWhen(true)] out NuGetVersion? version)
        {
            version = Extract(prefix: RELEASE_PREFIX, branch: branchName) ?? Extract(prefix: HOTFIX_PREFIX, branch: branchName);

            return version != null;
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

            Version dv = new(revision: 0, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);

            return new NuGetVersion(dv);
        }
    }
}