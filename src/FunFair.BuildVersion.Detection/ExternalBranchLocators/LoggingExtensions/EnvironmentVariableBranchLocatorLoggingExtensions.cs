using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Detection.ExternalBranchLocators.LoggingExtensions;

internal static partial class EnvironmentVariableBranchLocatorLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Branch from continuous integration server: {branch}")]
    public static partial void LogBranchFromContinuousIntegration(this ILogger logger, string branch);
}