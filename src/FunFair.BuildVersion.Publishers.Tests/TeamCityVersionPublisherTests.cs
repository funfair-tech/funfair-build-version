using System;
using FunFair.BuildVersion.Publishers.Tests.Helpers;
using FunFair.Test.Common;
using FunFair.Test.Common.Helpers;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Publishers.Tests;

public sealed class TeamCityVersionPublisherTests : TestBase
{
    private const string ENV_VAR_NAME = "TEAMCITY_VERSION";

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void PublishWhenTeamCityVersionNotPresent_ShouldNotWriteToConsole(string? envValue)
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR_NAME, value: envValue);
        using ConsoleCapture capture = new();

        TeamCityVersionPublisher publisher = new();
        NuGetVersion version = new(major: 1, minor: 2, patch: 3);

        publisher.Publish(version: version);

        Assert.Equal(expected: string.Empty, actual: capture.StdOut);
    }

    [Fact]
    public void PublishWhenTeamCityVersionSet_ShouldWriteBuildNumberAndParameterLinesToConsole()
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR_NAME, value: "2025.1");
        using ConsoleCapture capture = new();

        TeamCityVersionPublisher publisher = new();
        NuGetVersion version = new(major: 1, minor: 2, patch: 3);

        publisher.Publish(version: version);

        string stdOut = capture.StdOut;
        Assert.Contains(
            expectedSubstring: $"##teamcity[buildNumber '{version}']",
            actualString: stdOut,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: $"##teamcity[setParameter name='system.build.version' value='{version}']",
            actualString: stdOut,
            comparisonType: StringComparison.Ordinal
        );
    }
}
