using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Detection.ExternalBranchLocators;

public sealed class GitHubRefEnvironmentVariableBranchLocator : EnvironmentVariableBranchLocator
{
    public GitHubRefEnvironmentVariableBranchLocator(
        ILogger<GitHubRefEnvironmentVariableBranchLocator> logger
    )
        : base(environmentVariable: "GITHUB_REF", logger: logger) { }
}
