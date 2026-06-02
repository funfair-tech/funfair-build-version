using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.Tests;

public sealed class OptionsTests : TestBase
{
    [Fact]
    public void DefaultWarningsAsErrorsIsFalse()
    {
        Options options = new();

        Assert.False(condition: options.WarningsAsErrors, userMessage: "WarningsAsErrors should default to false");
    }

    [Fact]
    public void DefaultBuildNumberIsMinusOne()
    {
        Options options = new();

        Assert.Equal(expected: -1, actual: options.BuildNumber);
    }

    [Fact]
    public void DefaultReleaseSuffixIsEmptyString()
    {
        Options options = new();

        Assert.Equal(expected: string.Empty, actual: options.ReleaseSuffix);
    }

    [Fact]
    public void DefaultPackageIsEmptyString()
    {
        Options options = new();

        Assert.Equal(expected: string.Empty, actual: options.Package);
    }

    [Fact]
    public void DefaultGithubTokenIsEmptyString()
    {
        Options options = new();

        Assert.Equal(expected: string.Empty, actual: options.GithubToken);
    }

    [Fact]
    public void DefaultGitTagPrefixIsEmptyString()
    {
        Options options = new();

        Assert.Equal(expected: string.Empty, actual: options.GitTagPrefix);
    }

    [Fact]
    public void TwoDefaultOptionsAreEqual()
    {
        Options first = new();
        Options second = new();

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public void OptionsWithSameValuesAreEqual()
    {
        Options first = new()
        {
            WarningsAsErrors = true,
            BuildNumber = 42,
            ReleaseSuffix = "rc",
            Package = "mypkg",
            GithubToken = "ghtoken",
            GitTagPrefix = "v",
        };

        Options second = new()
        {
            WarningsAsErrors = true,
            BuildNumber = 42,
            ReleaseSuffix = "rc",
            Package = "mypkg",
            GithubToken = "ghtoken",
            GitTagPrefix = "v",
        };

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public void OptionsWithDifferentBuildNumberAreNotEqual()
    {
        Options first = new() { BuildNumber = 1 };
        Options second = new() { BuildNumber = 2 };

        Assert.NotEqual(expected: first, actual: second);
    }

    [Fact]
    public void OptionsWithDifferentWarningsAsErrorsAreNotEqual()
    {
        Options first = new() { WarningsAsErrors = false };
        Options second = new() { WarningsAsErrors = true };

        Assert.NotEqual(expected: first, actual: second);
    }

    [Fact]
    public void WithExpressionProducesUpdatedRecord()
    {
        Options original = new() { BuildNumber = 10 };
        Options modified = original with { BuildNumber = 99 };

        Assert.Equal(expected: 10, actual: original.BuildNumber);
        Assert.Equal(expected: 99, actual: modified.BuildNumber);
    }
}
