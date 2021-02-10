using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FunFair.BuildVersion.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection
{
    /// <summary>
    ///     Version Detection.
    /// </summary>
    public sealed class VersionDetector : IVersionDetector
    {
        private readonly IBranchClassification _branchClassification;
        private readonly IBranchDiscovery _branchDiscovery;
        private readonly ILogger<VersionDetector> _logger;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="branchDiscovery">Branch discovery</param>
        /// <param name="branchClassification">Branch classification</param>
        /// <param name="logger">Logging</param>
        public VersionDetector(IBranchDiscovery branchDiscovery, IBranchClassification branchClassification, ILogger<VersionDetector> logger)
        {
            this._branchDiscovery = branchDiscovery ?? throw new ArgumentNullException(nameof(branchDiscovery));
            this._branchClassification = branchClassification ?? throw new ArgumentNullException(nameof(branchClassification));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public NuGetVersion? FindVersion(int buildNumber)
        {
            string currentBranch = this._branchDiscovery.FindCurrentBranch();
            this._logger.LogInformation($">>>>>> Current branch: {currentBranch}");
            this._logger.LogInformation($">>>>>> Current Build number: {buildNumber}");

            if (this._branchClassification.IsRelease(branchName: currentBranch, out NuGetVersion? branchVersion))
            {
                return branchVersion;
            }

            NuGetVersion latest = this.DetermineLatestReleaseFromPreviousReleaseBranches(buildNumber: buildNumber);

            this._logger.LogInformation($"Latest Release Version: {latest}");

            return this.BuildPreReleaseVersion(latest: latest, currentBranch: currentBranch, buildNumber: buildNumber);
        }

        private NuGetVersion DetermineLatestReleaseFromPreviousReleaseBranches(int buildNumber)
        {
            IReadOnlyList<string> branches = this._branchDiscovery.FindBranches();
            NuGetVersion latest = new(version: @"0.0.0.0");

            foreach (string branch in branches)
            {
                this._logger.LogDebug($" * => {branch}");

                if (this._branchClassification.IsRelease(branchName: branch, out NuGetVersion? version))
                {
                    if (version != null)
                    {
                        if (latest < version)
                        {
                            latest = version;
                        }
                    }
                }
            }

            Version dv = new(revision: buildNumber, build: latest.Version.Build, minor: latest.Version.Minor, major: latest.Version.Major);

            return new NuGetVersion(dv);
        }

        private NuGetVersion BuildPreReleaseVersion(NuGetVersion latest, string currentBranch, int buildNumber)
        {
            string usedSuffix = this.BuildPreReleaseSuffix(currentBranch: currentBranch);

            this._logger.LogInformation($"Build Pre-Release Suffix: {usedSuffix}");

            Version version = new(major: latest.Version.Major, minor: latest.Version.Minor, latest.Version.Build + 1, revision: buildNumber);

            return new NuGetVersion(version: version, releaseLabel: usedSuffix);
        }

        private string BuildPreReleaseSuffix(string currentBranch)
        {
            if (this._branchClassification.IsPullRequest(currentBranch: currentBranch, out long pullRequestId))
            {
                currentBranch = @"pull-request-" + pullRequestId.ToString(CultureInfo.InvariantCulture);
            }

            StringBuilder suffix = new(currentBranch);

            int pos = suffix.ToString()
                            .IndexOf('/');

            if (pos != -1)
            {
                suffix = suffix.Remove(startIndex: 0, pos + 1);
            }

            const char replacementChar = '-';

            foreach (char ch in currentBranch.Where(predicate: c => !char.IsLetterOrDigit(c) && c != replacementChar)
                                             .Distinct())
            {
                suffix.Replace(oldChar: ch, newChar: replacementChar);
            }

            suffix.Replace(oldValue: "--", newValue: "-");

            string usedSuffix = suffix.ToString()
                                      .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(usedSuffix))
            {
                usedSuffix = @"prerelease";
            }

            usedSuffix = usedSuffix.TrimStart('-');

            const int maxSuffixLength = 15;

            if (usedSuffix.Length > maxSuffixLength)
            {
                usedSuffix = usedSuffix.Substring(startIndex: 0, length: maxSuffixLength);
            }

            // Ensure that the name doesn't end with a -
            usedSuffix = usedSuffix.Trim(trimChar: '-');

            return usedSuffix;
        }
    }
}