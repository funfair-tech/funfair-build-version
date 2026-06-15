using FunFair.BuildVersion.Detection.ExternalBranchLocators;
using FunFair.Test.Common;
using FunFair.Test.Common.Helpers;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests;

public sealed class GitHubRefEnvironmentVariableBranchLocatorTests : LoggingTestBase
{
    private const string ENV_VAR = "GITHUB_REF";

    public GitHubRefEnvironmentVariableBranchLocatorTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void WhenEnvironmentVariableNotSet_CurrentBranchIsNull()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: null);

        GitHubRefEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitHubRefEnvironmentVariableBranchLocator>()
        );

        Assert.Null(locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsEmpty_CurrentBranchIsNull()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: string.Empty);

        GitHubRefEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitHubRefEnvironmentVariableBranchLocator>()
        );

        Assert.Null(locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsWhitespace_CurrentBranchIsNull()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: "   ");

        GitHubRefEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitHubRefEnvironmentVariableBranchLocator>()
        );

        Assert.Null(locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsPlainBranchName_CurrentBranchIsReturnedAsIs()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: "refs/heads/main");

        GitHubRefEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitHubRefEnvironmentVariableBranchLocator>()
        );

        Assert.Equal(expected: "main", actual: locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsRefsHeadsBranch_PrefixIsStripped()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: "refs/heads/release/1.0.0");

        GitHubRefEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitHubRefEnvironmentVariableBranchLocator>()
        );

        Assert.Equal(expected: "release/1.0.0", actual: locator.CurrentBranch);
    }
}
