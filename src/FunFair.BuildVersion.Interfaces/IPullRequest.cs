namespace FunFair.BuildVersion.Interfaces
{
    /// <summary>
    ///     Pull Request detection.
    /// </summary>
    public interface IPullRequest
    {
        /// <summary>
        ///     Attempts to get the Pull request ID from the current branch name.
        /// </summary>
        /// <param name="currentBranch">The current branch name.</param>
        /// <param name="pullRequestId">The pull request id if found</param>
        /// <returns>true, if the branch was a pull request; otherwise, false.</returns>
        bool ExtractPullRequestId(string currentBranch, out long pullRequestId);
    }
}