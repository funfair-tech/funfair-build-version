using FunFair.BuildVersion.Detection.ExternalBranchLocators;
using FunFair.Test.Common;
using FunFair.Test.Common.Helpers;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests;

public sealed class GitBranchEnvironmentVariableBranchLocatorTests : LoggingTestBase
{
    private const string ENV_VAR = "GIT_BRANCH";

    public GitBranchEnvironmentVariableBranchLocatorTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void WhenEnvironmentVariableNotSet_CurrentBranchIsNull()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: null);

        GitBranchEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitBranchEnvironmentVariableBranchLocator>()
        );

        Assert.Null(locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsEmpty_CurrentBranchIsNull()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: string.Empty);

        GitBranchEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitBranchEnvironmentVariableBranchLocator>()
        );

        Assert.Null(locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsWhitespace_CurrentBranchIsNull()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: "   ");

        GitBranchEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitBranchEnvironmentVariableBranchLocator>()
        );

        Assert.Null(locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableIsPlainBranchName_CurrentBranchIsReturnedAsIs()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: "feature/my-feature");

        GitBranchEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitBranchEnvironmentVariableBranchLocator>()
        );

        Assert.Equal(expected: "feature/my-feature", actual: locator.CurrentBranch);
    }

    [Fact]
    public void WhenEnvironmentVariableHasRefsHeadsPrefix_PrefixIsStripped()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR, value: "refs/heads/feature/my-feature");

        GitBranchEnvironmentVariableBranchLocator locator = new(
            this.GetTypedLogger<GitBranchEnvironmentVariableBranchLocator>()
        );

        Assert.Equal(expected: "feature/my-feature", actual: locator.CurrentBranch);
    }
}
