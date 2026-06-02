using System;
using System.Diagnostics.CodeAnalysis;
using FunFair.BuildVersion.Services;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FunFair.BuildVersion.Tests.Services;

public sealed class LoggerProxyTests : TestBase
{
    [Fact]
    [SuppressMessage(
        category: "Nullable.Extended.Analyzer",
        checkId: "NX0003:NullForgivingOperator",
        Justification = "Intentionally passing null to verify ArgumentNullException is thrown"
    )]
    public void ConstructorWithNullLoggerThrowsArgumentNullException()
    {
        ILogger? nullLogger = null;

        Assert.Throws<ArgumentNullException>(() => new LoggerProxy<LoggerProxyTests>(nullLogger!));
    }

    [Fact]
    [SuppressMessage(
        category: "Microsoft.Performance",
        checkId: "CA1873:AvoidUnnecessaryStringEvaluation",
        Justification = "Test code intentionally calls Log with a literal string to verify delegation"
    )]
    public void LogDelegatesToUnderlyingLogger()
    {
        ILogger underlying = TestBase.GetSubstitute<ILogger>();
        LoggerProxy<LoggerProxyTests> proxy = new(underlying);

        proxy.Log(
            logLevel: LogLevel.Information,
            eventId: default,
            state: "test message",
            exception: null,
            formatter: static (s, _) => s
        );

        underlying
            .Received(1)
            .Log(
                logLevel: LogLevel.Information,
                eventId: default,
                state: "test message",
                exception: null,
                formatter: Arg.Any<Func<string, Exception?, string>>()
            );
    }

    [Theory]
    [InlineData(LogLevel.Trace, true)]
    [InlineData(LogLevel.Debug, false)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Warning, true)]
    [InlineData(LogLevel.Error, true)]
    [InlineData(LogLevel.Critical, true)]
    public void IsEnabledDelegatesToUnderlyingLogger(LogLevel logLevel, bool underlyingResult)
    {
        ILogger underlying = TestBase.GetSubstitute<ILogger>();
        underlying.IsEnabled(logLevel).Returns(underlyingResult);
        LoggerProxy<LoggerProxyTests> proxy = new(underlying);

        bool result = proxy.IsEnabled(logLevel);

        Assert.Equal(expected: underlyingResult, actual: result);
        underlying.Received(1).IsEnabled(logLevel);
    }

    [Fact]
    public void BeginScopeDelegatesToUnderlyingLogger()
    {
        ILogger underlying = TestBase.GetSubstitute<ILogger>();

        using IDisposable fakeScope = TestBase.GetSubstitute<IDisposable>();
        underlying.BeginScope("state").Returns(fakeScope);
        LoggerProxy<LoggerProxyTests> proxy = new(underlying);

        using IDisposable? result = proxy.BeginScope("state");

        Assert.Equal(expected: fakeScope, actual: result);

        using IDisposable? verifyScope = underlying.Received(1).BeginScope("state");
    }

    [Fact]
    public void BeginScopeReturnsNullWhenUnderlyingReturnsNull()
    {
        ILogger underlying = TestBase.GetSubstitute<ILogger>();
        underlying.BeginScope(Arg.Any<string>()).Returns((IDisposable?)null);
        LoggerProxy<LoggerProxyTests> proxy = new(underlying);

        IDisposable? result;

        using (result = proxy.BeginScope("state"))
        {
            Assert.Null(result);
        }
    }
}
