using FunFair.BuildVersion.Detection.ExternalBranchLocators;
using FunFair.BuildVersion.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace FunFair.BuildVersion.Detection;

public static class VersionDetectionSetup
{
    public static IServiceCollection AddBuildVersionDetection(this IServiceCollection services, Repository repo, IBranchSettings branchSettings)
    {
        return services.AddSingleton(branchSettings)
                       .AddSingleton(repo)
                       .AddSingleton<IBranchDiscovery, GitBranchDiscovery>()
                       .AddSingleton<IBranchClassification, BranchClassification>()
                       .AddSingleton<IVersionDetector, VersionDetector>()
                       .AddSingleton<IExternalBranchLocator, GitHubRefEnvironmentVariableBranchLocator>()
                       .AddSingleton<IExternalBranchLocator, GitBranchEnvironmentVariableBranchLocator>();
    }
}