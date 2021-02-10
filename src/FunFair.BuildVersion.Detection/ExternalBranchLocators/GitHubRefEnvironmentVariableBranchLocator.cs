namespace FunFair.BuildVersion.Detection.ExternalBranchLocators
{
    /// <summary>
    ///     Branch detector using the 'GITHUB_REF' environment variable.
    /// </summary>
    public sealed class GitHubRefEnvironmentVariableBranchLocator : EnvironmentVariableBranchLocator
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public GitHubRefEnvironmentVariableBranchLocator()
            : base(@"GITHUB_REF")
        {
        }
    }
}