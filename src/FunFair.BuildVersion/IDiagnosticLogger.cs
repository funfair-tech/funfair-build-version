using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion;

public interface IDiagnosticLogger : ILogger
{
    long Errors { get; }

    bool IsErrored { get; }
}
