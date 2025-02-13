using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Detection.LoggingExtensions;

internal static partial class GitBranchDiscoveryLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Head SHA: {sha}")]
    public static partial void LogHeadSha(this ILogger<GitBranchDiscovery> logger, string sha);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Pull Request: {pullRequestId}"
    )]
    public static partial void LogPullRequest(
        this ILogger<GitBranchDiscovery> logger,
        long pullRequestId
    );

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Found Branch for PR {pullRequestId} : {branch}"
    )]
    public static partial void LogFoundBranchForPullRequest(
        this ILogger<GitBranchDiscovery> logger,
        long pullRequestId,
        string branch
    );
}
