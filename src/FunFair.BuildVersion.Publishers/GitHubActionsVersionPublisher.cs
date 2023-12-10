using System;
using System.IO;
using FunFair.BuildVersion.Interfaces;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Publishers;

public sealed class GitHubActionsVersionPublisher : IVersionPublisher
{
    public void Publish(NuGetVersion version)
    {
        string? env = Environment.GetEnvironmentVariable("GITHUB_ENV");

        if (!string.IsNullOrEmpty(env))
        {
            File.AppendAllLines(path: env,
            [
                $"BUILD_VERSION={version}"
            ]);
        }
    }
}