using System;
using System.Diagnostics.CodeAnalysis;
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
            WriteTeamCityParameters(version: version, env: env);
        }
    }

    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0045", Justification = "IVersionPublisher Method is not async")]
    private static void WriteTeamCityParameters(NuGetVersion version, string env)
    {
        string[] fileContent = [$"BUILD_VERSION={version}"];

        File.AppendAllLines(path: env, contents: fileContent);
    }
}