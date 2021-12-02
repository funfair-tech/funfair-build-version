using System;
using FunFair.BuildVersion.Interfaces;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Publishers;

/// <summary>
///     Publishes the version so it is suitable for integrations with TeamCity.
/// </summary>
public sealed class TeamCityVersionPublisher : IVersionPublisher
{
    /// <inheritdoc />
    public void Publish(NuGetVersion version)
    {
        string? env = Environment.GetEnvironmentVariable("TEAMCITY_VERSION");

        if (!string.IsNullOrWhiteSpace(env))
        {
            Console.WriteLine($"##teamcity[buildNumber '{version}']");
            Console.WriteLine($"##teamcity[setParameter name='system.build.version' value='{version}']");
        }
    }
}