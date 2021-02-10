using System.Diagnostics.CodeAnalysis;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces
{
    /// <summary>
    ///     Branch Classification
    /// </summary>
    public interface IBranchClassification
    {
        /// <summary>
        ///     Checks to see if the branch is a release branch.
        /// </summary>
        /// <param name="branchName">The branch to check</param>
        /// <param name="version">The version that was found, if the branch is a release branch.</param>
        /// <returns>true, if the branch is a release branch; otherwise, false.</returns>
        bool IsRelease(string branchName, [NotNullWhen(true)] out NuGetVersion? version);

        /// <summary>
        ///     Attempts to get the Pull request ID from the current branch name.
        /// </summary>
        /// <param name="currentBranch">The current branch name.</param>
        /// <param name="pullRequestId">The pull request id if found</param>
        /// <returns>true, if the branch was a pull request; otherwise, false.</returns>
        bool IsPullRequest(string currentBranch, out long pullRequestId);
    }
}