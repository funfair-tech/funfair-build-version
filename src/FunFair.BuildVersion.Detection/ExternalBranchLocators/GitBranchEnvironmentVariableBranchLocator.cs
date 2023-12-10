using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Detection.ExternalBranchLocators;

public sealed class GitBranchEnvironmentVariableBranchLocator : EnvironmentVariableBranchLocator
{
    public GitBranchEnvironmentVariableBranchLocator(ILogger<GitBranchEnvironmentVariableBranchLocator> logger)
        : base(environmentVariable: "GIT_BRANCH", logger: logger)
    {
    }
}