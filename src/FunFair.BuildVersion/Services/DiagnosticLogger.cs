using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Services
{
    /// <summary>
    ///     Diagnostic logger.
    /// </summary>
    public sealed class DiagnosticLogger : IDiagnosticLogger
    {
        private readonly bool _warningsAsErrors;
        private long _errors;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="warningsAsErrors">Whether warnings should be considered errors.</param>
        public DiagnosticLogger(bool warningsAsErrors)
        {
            this._warningsAsErrors = warningsAsErrors;

            if (this._warningsAsErrors)
            {
                this.LogInformation("** Running with Warnings as Errors");
            }
        }

        /// <inheritdoc />
        public long Errors => this._errors;

        /// <inheritdoc />
        public bool IsErrored => this.Errors > 0;

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (this.IsWarningAsError(logLevel))
            {
                // ReSharper disable once TailRecursiveCall - this is not tail recursive despite what R# thinks
                this.Log(logLevel: LogLevel.Error, eventId: eventId, state: state, exception: exception, formatter: formatter);

                return;
            }

            if (logLevel == LogLevel.Information)
            {
                OutputInformationalMessage(state: state, exception: exception, formatter: formatter);

                return;
            }

            this.OutputMessageWithStatus(logLevel: logLevel, state: state, exception: exception, formatter: formatter);
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.Debug;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
        {
            return null;
        }

        private void OutputMessageWithStatus<TState>(LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            string msg = formatter(arg1: state, arg2: exception);

            Action<string> output = Console.WriteLine;

            if (IsError(logLevel))
            {
                Interlocked.Increment(ref this._errors);

                output = Console.Error.WriteLine;
            }

            string status = logLevel.ToString()
                                    .ToUpperInvariant();

            output($"{status}: {msg}");
        }

        private static void OutputInformationalMessage<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string msg = formatter(arg1: state, arg2: exception);
            Console.WriteLine(msg);
        }

        private static bool IsError(LogLevel logLevel)
        {
            return logLevel is LogLevel.Critical or LogLevel.Error;
        }

        private bool IsWarningAsError(LogLevel logLevel)
        {
            return this._warningsAsErrors && logLevel == LogLevel.Warning;
        }
    }
}