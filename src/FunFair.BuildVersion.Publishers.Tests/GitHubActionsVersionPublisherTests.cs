using System;
using System.IO;
using FunFair.Test.Common;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Publishers.Tests;

public sealed class GitHubActionsVersionPublisherTests : TestBase
{
    private const string EnvVarName = "GITHUB_ENV";

    [Fact]
    public void PublishWhenGithubEnvNotSet_ShouldNotWriteToFile()
    {
        string? originalValue = Environment.GetEnvironmentVariable(EnvVarName);

        try
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: null);

            GitHubActionsVersionPublisher publisher = new();
            NuGetVersion version = new(major: 1, minor: 2, patch: 3);

            publisher.Publish(version: version);

            // No exception and no file created - nothing to assert beyond no crash
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: originalValue);
        }
    }

    [Fact]
    public void PublishWhenGithubEnvSetToEmptyString_ShouldNotWriteToFile()
    {
        string? originalValue = Environment.GetEnvironmentVariable(EnvVarName);

        try
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: string.Empty);

            GitHubActionsVersionPublisher publisher = new();
            NuGetVersion version = new(major: 1, minor: 2, patch: 3);

            publisher.Publish(version: version);

            // No exception and no file created - nothing to assert beyond no crash
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: originalValue);
        }
    }

    [Fact]
    public void PublishWhenGithubEnvSet_ShouldAppendVersionToFile()
    {
        string? originalValue = Environment.GetEnvironmentVariable(EnvVarName);
        string tempFile = Path.GetTempFileName();

        try
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: tempFile);

            GitHubActionsVersionPublisher publisher = new();
            NuGetVersion version = new(major: 1, minor: 2, patch: 3);

            publisher.Publish(version: version);

            string fileContent = File.ReadAllText(tempFile);
            Assert.Contains(
                expectedSubstring: $"BUILD_VERSION={version}",
                actualString: fileContent,
                comparisonType: StringComparison.Ordinal
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: EnvVarName, value: originalValue);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
