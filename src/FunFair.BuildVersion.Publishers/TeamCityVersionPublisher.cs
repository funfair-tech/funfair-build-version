using System;
using FunFair.BuildVersion.Interfaces;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Publishers;

public sealed class TeamCityVersionPublisher : IVersionPublisher
{
    public void Publish(NuGetVersion version)
    {
        string? env = Environment.GetEnvironmentVariable("TEAMCITY_VERSION");

        if (!string.IsNullOrWhiteSpace(env))
        {
            WriteTeamCityParameters(version);
        }
    }

    private static void WriteTeamCityParameters(NuGetVersion version)
    {
        Console.WriteLine($"##teamcity[buildNumber '{version}']");
        Console.WriteLine($"##teamcity[setParameter name='system.build.version' value='{version}']");
    }
}