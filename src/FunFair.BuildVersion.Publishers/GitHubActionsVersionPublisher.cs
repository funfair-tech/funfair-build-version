using System;
using System.IO;
using FunFair.BuildVersion.Interfaces;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Publishers;

/// <summary>
///     Publishes the version so it is suitable for integrations with Github Actions.
/// </summary>
public sealed class GitHubActionsVersionPublisher : IVersionPublisher
{
    /// <inheritdoc />
    public void Publish(NuGetVersion version)
    {
        string? env = Environment.GetEnvironmentVariable("GITHUB_ENV");

        if (!string.IsNullOrEmpty(env))
        {
            File.AppendAllLines(path: env, new[] { $"BUILD_VERSION={version}" });
        }
    }
}