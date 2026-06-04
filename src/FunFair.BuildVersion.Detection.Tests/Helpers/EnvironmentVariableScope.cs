using System;

namespace FunFair.BuildVersion.Detection.Tests.Helpers;

internal sealed class EnvironmentVariableScope : IDisposable
{
    private readonly string? _originalValue;
    private readonly string _variableName;
    private bool _disposed;

    public EnvironmentVariableScope(string variableName, string? value)
    {
        this._variableName = variableName;
        this._originalValue = Environment.GetEnvironmentVariable(variableName);
        Environment.SetEnvironmentVariable(variable: variableName, value: value);
    }

    public void Dispose()
    {
        if (!this._disposed)
        {
            this._disposed = true;
            Environment.SetEnvironmentVariable(variable: this._variableName, value: this._originalValue);
        }
    }
}
