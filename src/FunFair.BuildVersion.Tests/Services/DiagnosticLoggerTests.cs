using System;
using FunFair.BuildVersion.Services;
using FunFair.Test.Common;
using FunFair.Test.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FunFair.BuildVersion.Tests.Services;

public sealed class DiagnosticLoggerTests : TestBase
{
    private static void Log(DiagnosticLogger logger, LogLevel logLevel, string message)
    {
        logger.Log(
            logLevel: logLevel,
            eventId: default,
            state: message,
            exception: null,
            formatter: static (s, _) => s
        );
    }

    [Fact]
    public void InitialErrorCountIsZero()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        Assert.Equal(expected: 0, actual: logger.Errors);
    }

    [Fact]
    public void InitialIsErroredIsFalse()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        Assert.False(condition: logger.IsErrored, userMessage: "Should not be errored initially");
    }

    [Fact]
    public void InformationWritesToStdout()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Information, message: "hello info");

        Assert.Contains(
            expectedSubstring: "hello info",
            actualString: capture.StdOut,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Equal(expected: 0, actual: logger.Errors);
        Assert.False(condition: logger.IsErrored, userMessage: "Should not be errored after information log");
    }

    [Fact]
    public void InformationDoesNotWriteToStderr()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Information, message: "hello info");

        Assert.Empty(capture.StdErr);
    }

    [Fact]
    public void InformationHasNoPrefix()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Information, message: "hello info");

        Assert.Equal(expected: "hello info", actual: capture.StdOut.Trim());
    }

    [Fact]
    public void ErrorWritesToStderr()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Error, message: "something broke");

        Assert.Contains(
            expectedSubstring: "something broke",
            actualString: capture.StdErr,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void ErrorHasErrorPrefix()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Error, message: "something broke");

        Assert.Equal(expected: "ERROR: something broke", actual: capture.StdErr.Trim());
    }

    [Fact]
    public void ErrorIncrementsErrorCount()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Error, message: "err");

        Assert.Equal(expected: 1, actual: logger.Errors);
        Assert.True(condition: logger.IsErrored, userMessage: "Should be errored after error log");
    }

    [Fact]
    public void MultipleErrorsIncrementErrorCountCumulatively()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Error, message: "err1");
        Log(logger: logger, logLevel: LogLevel.Error, message: "err2");
        Log(logger: logger, logLevel: LogLevel.Error, message: "err3");

        Assert.Equal(expected: 3, actual: logger.Errors);
    }

    [Fact]
    public void CriticalWritesToStderr()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Critical, message: "critical failure");

        Assert.Contains(
            expectedSubstring: "critical failure",
            actualString: capture.StdErr,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void CriticalHasCriticalPrefix()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Critical, message: "critical failure");

        Assert.Equal(expected: "CRITICAL: critical failure", actual: capture.StdErr.Trim());
    }

    [Fact]
    public void CriticalIncrementsErrorCount()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Critical, message: "crit");

        Assert.Equal(expected: 1, actual: logger.Errors);
        Assert.True(condition: logger.IsErrored, userMessage: "Should be errored after critical log");
    }

    [Fact]
    public void WarningWithWarningsAsErrorsFalseWritesToStdout()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Warning, message: "a warning");

        Assert.Contains(
            expectedSubstring: "a warning",
            actualString: capture.StdOut,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void WarningWithWarningsAsErrorsFalseHasWarningPrefix()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Warning, message: "a warning");

        Assert.Equal(expected: "WARNING: a warning", actual: capture.StdOut.Trim());
    }

    [Fact]
    public void WarningWithWarningsAsErrorsFalseDoesNotIncrementErrorCount()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Warning, message: "a warning");

        Assert.Equal(expected: 0, actual: logger.Errors);
        Assert.False(
            condition: logger.IsErrored,
            userMessage: "Should not be errored after warning when warnings-as-errors is false"
        );
    }

    [Fact]
    public void WarningWithWarningsAsErrorsTrueWritesToStderr()
    {
        DiagnosticLogger logger = new(warningsAsErrors: true);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Warning, message: "treated as error");

        Assert.Contains(
            expectedSubstring: "treated as error",
            actualString: capture.StdErr,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void WarningWithWarningsAsErrorsTrueIncrementsErrorCount()
    {
        DiagnosticLogger logger = new(warningsAsErrors: true);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Warning, message: "treated as error");

        Assert.Equal(expected: 1, actual: logger.Errors);
        Assert.True(condition: logger.IsErrored, userMessage: "Should be errored when warning treated as error");
    }

    [Fact]
    public void DebugIsNotWrittenToStdout()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Debug, message: "debug message");

        Assert.Empty(capture.StdOut);
    }

    [Fact]
    public void DebugIsNotWrittenToStderr()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Debug, message: "debug message");

        Assert.Empty(capture.StdErr);
    }

    [Fact]
    public void DebugDoesNotIncrementErrorCount()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using ConsoleCapture capture = new();
        Log(logger: logger, logLevel: LogLevel.Debug, message: "debug message");

        Assert.Equal(expected: 0, actual: logger.Errors);
    }

    [Theory]
    [InlineData(LogLevel.Trace, true)]
    [InlineData(LogLevel.Debug, false)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Warning, true)]
    [InlineData(LogLevel.Error, true)]
    [InlineData(LogLevel.Critical, true)]
    public void IsEnabledReturnsExpectedValue(LogLevel logLevel, bool expected)
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        Assert.Equal(expected: expected, actual: logger.IsEnabled(logLevel));
    }

    [Fact]
    public void BeginScopeReturnsNonNull()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        using IDisposable scope = logger.BeginScope("some scope");

        Assert.NotNull(scope);
    }

    [Fact]
    public void BeginScopeReturnedDisposableCanBeDisposedMultipleTimes()
    {
        DiagnosticLogger logger = new(warningsAsErrors: false);

        IDisposable scope = logger.BeginScope("scope");
        scope.Dispose();
        scope.Dispose();
    }
}
