using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Detection.LoggingExtensions;

internal static partial class VersionDetectorLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = ">>>>>> Current branch: {branch}")]
    public static partial void LogCurrentBranch(this ILogger<VersionDetector> logger, string branch);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = ">>>>>> Current Build number: {buildNumber}")]
    public static partial void LogCurrentBuildNumber(this ILogger<VersionDetector> logger, int buildNumber);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Latest Release Version: {latest}")]
    private static partial void LogLatestReleaseVersion(this ILogger<VersionDetector> logger, string latest);

    public static void LogLatestReleaseVersion(this ILogger<VersionDetector> logger, NuGetVersion latest)
    {
        logger.LogLatestReleaseVersion(latest: latest.ToString());
    }

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "* {branch}")]
    [Conditional("DEBUG")]
    public static partial void LogFoundBranch(this ILogger<VersionDetector> logger, string branch);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Build Pre-Release Suffix: {suffix}")]
    public static partial void LogPreReleaseSuffix(this ILogger<VersionDetector> logger, string suffix);
}