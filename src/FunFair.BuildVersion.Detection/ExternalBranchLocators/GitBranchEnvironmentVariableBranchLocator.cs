namespace FunFair.BuildVersion.Detection.ExternalBranchLocators
{
    /// <summary>
    ///     Branch detector using the 'GIT_BRANCH' environment variable.
    /// </summary>
    public sealed class GitBranchEnvironmentVariableBranchLocator : EnvironmentVariableBranchLocator
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public GitBranchEnvironmentVariableBranchLocator()
            : base(@"GIT_BRANCH")
        {
        }
    }
}