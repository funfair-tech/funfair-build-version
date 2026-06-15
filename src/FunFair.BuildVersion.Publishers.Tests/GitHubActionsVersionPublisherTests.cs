using System;
using System.IO;
using FunFair.Test.Common;
using FunFair.Test.Common.Helpers;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Publishers.Tests;

public sealed class GitHubActionsVersionPublisherTests : TestBase
{
    private const string ENV_VAR_NAME = "GITHUB_ENV";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PublishWhenGithubEnvNotPresent_ShouldNotWriteToFile(string? envValue)
    {
        using EnvironmentVariableScope scope = new(variableName: ENV_VAR_NAME, value: envValue);

        GitHubActionsVersionPublisher publisher = new();
        NuGetVersion version = new(major: 1, minor: 2, patch: 3);

        publisher.Publish(version: version);

        // No exception and no file created - nothing to assert beyond no crash
    }

    [Fact]
    public void PublishWhenGithubEnvSet_ShouldAppendVersionToFile()
    {
        string tempFile = Path.GetTempFileName();

        using EnvironmentVariableScope scope = new(variableName: ENV_VAR_NAME, value: tempFile);

        try
        {
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
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
