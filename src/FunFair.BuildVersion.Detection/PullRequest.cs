using System;
using FunFair.BuildVersion.Interfaces;

namespace FunFair.BuildVersion.Detection
{
    /// <summary>
    ///     Pull Request detection.
    /// </summary>
    public sealed class PullRequest : IPullRequest
    {
        private const string PULL_REQUEST_PREFIX = @"refs/pull/";
        private const string PULL_REQUEST_SUFFIX = @"/head";

        /// <inheritdoc />
        public bool ExtractPullRequestId(string currentBranch, out long pullRequestId)
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
    }
}