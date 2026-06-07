using FunFair.BuildVersion.Detection.ExternalBranchLocators;
using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests;

public sealed class ServiceRegistrationTests : DependencyInjectionTestsBase
{
    public ServiceRegistrationTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure) { }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services.AddBuildVersionDetection(GetSubstitute<IBranchSettings>());
    }

    [Fact]
    public void BranchDiscoveryCanBeResolved()
    {
        this.RequireService<IBranchDiscovery>();
    }

    [Fact]
    public void BranchClassificationCanBeResolved()
    {
        this.RequireService<IBranchClassification>();
    }

    [Fact]
    public void VersionDetectorCanBeResolved()
    {
        this.RequireService<IVersionDetector>();
    }

    [Fact]
    public void GitHubRefExternalBranchLocatorIsRegistered()
    {
        this.RequireServiceInCollectionFor<IExternalBranchLocator, GitHubRefEnvironmentVariableBranchLocator>();
    }

    [Fact]
    public void GitBranchExternalBranchLocatorIsRegistered()
    {
        this.RequireServiceInCollectionFor<IExternalBranchLocator, GitBranchEnvironmentVariableBranchLocator>();
    }
}
