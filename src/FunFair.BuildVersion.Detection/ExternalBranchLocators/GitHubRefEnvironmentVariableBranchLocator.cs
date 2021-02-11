using Microsoft.Extensions.Logging;

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
        /// <param name="logger">Logging.</param>
        public GitHubRefEnvironmentVariableBranchLocator(ILogger<GitHubRefEnvironmentVariableBranchLocator> logger)
            : base(environmentVariable: @"GITHUB_REF", logger: logger)
        {
        }
    }
}