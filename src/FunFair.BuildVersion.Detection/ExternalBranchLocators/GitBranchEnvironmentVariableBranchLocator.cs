using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Detection.ExternalBranchLocators;

/// <summary>
///     Branch detector using the 'GIT_BRANCH' environment variable.
/// </summary>
public sealed class GitBranchEnvironmentVariableBranchLocator : EnvironmentVariableBranchLocator
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="logger">Logging.</param>
    public GitBranchEnvironmentVariableBranchLocator(ILogger<GitBranchEnvironmentVariableBranchLocator> logger)
        : base(environmentVariable: @"GIT_BRANCH", logger: logger)
    {
    }
}