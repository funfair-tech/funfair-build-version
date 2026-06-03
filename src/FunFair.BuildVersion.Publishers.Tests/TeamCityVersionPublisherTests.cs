using System;
using FunFair.BuildVersion.Publishers.Tests.Helpers;
using FunFair.Test.Common;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Publishers.Tests;

public sealed class TeamCityVersionPublisherTests : TestBase
{
    private const string EnvVarName = "TEAMCITY_VERSION";

    [Fact]
    public void PublishWhenTeamCityVersionNotSet_ShouldNotWriteToConsole()
    {
        string? originalValue = Environment.GetEnvironmentVariable(EnvVarName);

        try
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: null);

            using ConsoleCapture capture = new();

            TeamCityVersionPublisher publisher = new();
            NuGetVersion version = new(major: 1, minor: 2, patch: 3);

            publisher.Publish(version: version);

            Assert.Equal(expected: string.Empty, actual: capture.StdOut);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: originalValue);
        }
    }

    [Fact]
    public void PublishWhenTeamCityVersionSetToWhiteSpace_ShouldNotWriteToConsole()
    {
        string? originalValue = Environment.GetEnvironmentVariable(EnvVarName);

        try
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: "   ");

            using ConsoleCapture capture = new();

            TeamCityVersionPublisher publisher = new();
            NuGetVersion version = new(major: 1, minor: 2, patch: 3);

            publisher.Publish(version: version);

            Assert.Equal(expected: string.Empty, actual: capture.StdOut);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: originalValue);
        }
    }

    [Fact]
    public void PublishWhenTeamCityVersionSet_ShouldWriteBuildNumberAndParameterLinesToConsole()
    {
        string? originalValue = Environment.GetEnvironmentVariable(EnvVarName);

        try
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: "2025.1");

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
        finally
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: originalValue);
        }
    }
}
